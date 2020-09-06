using Ingvilt.Dto;
using Ingvilt.Dto.Videos;
using Ingvilt.Dto.WatchHistory;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;

using IntegrationTesting.Util;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class WatchHistoryRepositoryTests : BaseTest {
        private WatchHistoryRepository repository = new WatchHistoryRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private VideoRepository videoRepository = new VideoRepository();

        private Library testLibrary = null;

        private Video testVideo = null;
        private Video otherVideo = null;

        private Video CreateVideo() {
            var dto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(dto);
            return videoRepository.GetVideo(videoId).Result;
        }

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public WatchHistoryRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));

            testVideo = CreateVideo();
            otherVideo = CreateVideo();
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new WatchHistoryWatchDateSortCriteria(true));
        }

        [TestMethod]
        public void AddWatchedVideo() {
            repository.AddWatchedVideo(testVideo.VideoId).ConfigureAwait(false);
            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;

            var watchedVideo = watched[0];
            var timeDelta = DateTime.Now - watchedVideo.WatchDate;

            Assert.AreEqual(1, watched.Count);
            Assert.AreEqual(testVideo.VideoId, watchedVideo.VideoId);
            Assert.AreEqual(testVideo.Title, watchedVideo.VideoTitle);
            Assert.IsTrue(timeDelta.TotalSeconds < 10);
        }

        [TestMethod]
        public void GetWatchedVideosHistory_WithNoVideos() {
            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;
            Assert.AreEqual(0, watched.Count);
        }

        [TestMethod]
        public void DeletingWatchedVideo_ShouldRemoveHistory() {
            repository.AddWatchedVideo(testVideo.VideoId).ConfigureAwait(false);
            videoRepository.PermanentlyRemoveVideo(testVideo.VideoId).ConfigureAwait(false);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;
            
            Assert.AreEqual(0, watched.Count);
        }

        [TestMethod]
        public void DeletingNotWatchedVideo_ShouldntRemoveHistory() {
            repository.AddWatchedVideo(otherVideo.VideoId).ConfigureAwait(false);
            videoRepository.PermanentlyRemoveVideo(testVideo.VideoId).ConfigureAwait(false);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;

            var watchedVideo = watched[0];
            Assert.AreEqual(1, watched.Count);
            Assert.AreEqual(otherVideo.VideoId, watchedVideo.VideoId);
        }

        [TestMethod]
        public void DeletingWatchedVideo_ShouldOnlyRemoveHistory_ForThatVideo() {
            repository.AddWatchedVideo(testVideo.VideoId).ConfigureAwait(false);
            repository.AddWatchedVideo(otherVideo.VideoId).ConfigureAwait(false);
            videoRepository.PermanentlyRemoveVideo(testVideo.VideoId).ConfigureAwait(false);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;

            var watchedVideo = watched[0];
            Assert.AreEqual(1, watched.Count);
            Assert.AreEqual(otherVideo.VideoId, watchedVideo.VideoId);
        }

        [TestMethod]
        public void AddWatchedVideo_MultipleTimes() {
            repository.AddWatchedVideo(testVideo.VideoId).ConfigureAwait(false);
            repository.AddWatchedVideo(testVideo.VideoId).ConfigureAwait(false);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;

            Assert.AreEqual(2, watched.Count);
            Assert.AreEqual(testVideo.VideoId, watched[0].VideoId);
            Assert.AreEqual(testVideo.VideoId, watched[1].VideoId);
        }

        [TestMethod]
        public void SyncWatchedVideos_ShouldntUpdateVideos_ThatAreNotFound() {
            var videosToSync = new List<ExternallyWatchedVideoDto> {
                new ExternallyWatchedVideoDto("https://ingvilt.test.ingvilt.com", DateTime.Now)
            };

            repository.SyncWatchedVideos(videosToSync);
            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;
            Assert.AreEqual(0, watched.Count);
        }

        [TestMethod]
        public void SyncWatchedVideos_ShouldUpdateVideos_ThatAreFound() {
            var video2InitialTimesWatched = 5;

            var video1Dto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            video1Dto.SourceURL = "https://ingvilt.test.ingvilt.com";
            var video1Id = videoRepository.CreateVideo(video1Dto);

            var video2Dto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            video2Dto.SourceURL = "https://ingvilt.test.2.ingvilt.com";
            video2Dto.TimesWatched = video2InitialTimesWatched;
            var video2Id = videoRepository.CreateVideo(video2Dto);

            var videosToSync = new List<ExternallyWatchedVideoDto> {
                new ExternallyWatchedVideoDto("https://ingvilt.not.found.test.ingvilt.com", DateTime.Now),
                new ExternallyWatchedVideoDto(video1Dto.SourceURL, DateTime.Now),
                new ExternallyWatchedVideoDto(video2Dto.SourceURL, DateTime.Now - TimeSpan.FromDays(20))
            };

            repository.SyncWatchedVideos(videosToSync);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;
            Assert.AreEqual(2, watched.Count);
            Assert.AreEqual(video2Id, watched[0].VideoId);
            Assert.AreEqual(videosToSync[2].Time, watched[0].WatchDate);
            Assert.AreEqual(video1Id, watched[1].VideoId);
            Assert.AreEqual(videosToSync[1].Time, watched[1].WatchDate);

            var video1 = videoRepository.GetVideo(video1Id).Result;
            var video2 = videoRepository.GetVideo(video2Id).Result;

            Assert.AreEqual(1, video1.TimesWatched);
            Assert.AreEqual(video2InitialTimesWatched + 1, video2.TimesWatched);
            Assert.AreEqual(videosToSync[1].Time.ToUniversalTime(), video1.LastWatchDate);
            Assert.AreEqual(videosToSync[2].Time.ToUniversalTime(), video2.LastWatchDate);
        }

        [TestMethod]
        public void SyncWatchedVideos_ShouldNotUpdateLastWatchDate_IfEarlierThanPreviousDate() {
            var video1Dto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            video1Dto.SourceURL = "https://ingvilt.test.ingvilt.com";
            var video1Id = videoRepository.CreateVideo(video1Dto);

            var videoLatestWatchDate = new DateTime(2016, 1, 1);

            var video1 = videoRepository.GetVideo(video1Id).Result;
            video1.LastWatchDate = videoLatestWatchDate;
            videoRepository.UpdateVideo(video1).ConfigureAwait(false);

            var videosToSync = new List<ExternallyWatchedVideoDto> {
                new ExternallyWatchedVideoDto(video1Dto.SourceURL, new DateTime(2012, 1, 1))
            };

            repository.SyncWatchedVideos(videosToSync);

            var watched = repository.GetWatchedVideosHistory(GetFirstPage(), testLibrary.LibraryId).Result.Results;
            Assert.AreEqual(1, watched.Count);
            Assert.AreEqual(video1Id, watched[0].VideoId);
            Assert.AreEqual(videosToSync[0].Time, watched[0].WatchDate);

            video1 = videoRepository.GetVideo(video1Id).Result;

            Assert.AreEqual(1, video1.TimesWatched);
            Assert.AreEqual(videoLatestWatchDate.ToUniversalTime(), video1.LastWatchDate);
        }
    }
}

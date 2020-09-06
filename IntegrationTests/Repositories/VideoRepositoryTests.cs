using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Publishers;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTesting.Util;
using IntegrationTestingRedo.Util;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class VideoRepositoryTests : BaseTest {
        private VideoRepository repository = new VideoRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private CharacterRepository characterRepository = new CharacterRepository();
        private LocationRepository locationRepository = new LocationRepository();

        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public VideoRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Character CreateAndRetrieveCharacter(CreateCharacterDto dto) {
            var characterId = characterRepository.CreateCharacter(dto);
            return characterRepository.GetCharacter(characterId);
        }

        private Video CreateAndRetrieveVideo(CreateVideoDto dto) {
            var id = repository.CreateVideo(dto);
            return repository.GetVideo(id).Result;
        }

        private CreateVideoDto GetNewVideoDetails(Library library, Publisher publisher, Series series) {
            if (library == null) {
                library = testLibrary;
            }

            long publisherId = publisher == null ? -1 : publisher.PublisherId;
            long seriesId = series == null ? -1 : series.SeriesId;

            return CreateVideoUtil.GetNewVideoDetails(library.LibraryId, publisherId, seriesId);
        }

        private CreateVideoDto GetNewVideoDetails(Publisher publisher) {
            return GetNewVideoDetails(null, publisher, null);
        }

        private CreateVideoDto GetNewVideoDetails(Series series) {
            return GetNewVideoDetails(null, null, series);
        }

        private CreateVideoDto GetNewVideoDetails(Library library = null) {
            return GetNewVideoDetails(library, null, null);
        }

        private Video CreateAndRetrieveVideo() {
            return CreateAndRetrieveVideo(GetNewVideoDetails());
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new VideoCreationDateSortCriteria(true));
        }

        [TestMethod]
        public void CreateVideo_WithNoCover() {
            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);

            var videoRetrieved = repository.GetVideo(videoId).Result;

            Assert.AreEqual(videoId, videoRetrieved.VideoId);
            Assert.AreEqual(videoToCreate.Title, videoRetrieved.Title);
            Assert.AreEqual(videoToCreate.Description, videoRetrieved.Description);
            Assert.AreEqual(videoToCreate.DurationInSeconds, videoRetrieved.DurationInSeconds);
            Assert.AreEqual(videoToCreate.ExternalRating, videoRetrieved.ExternalRating);
            Assert.AreEqual(videoToCreate.Notes, videoRetrieved.Notes);
            Assert.AreEqual(videoToCreate.PublisherId, videoRetrieved.PublisherId);
            Assert.AreEqual(videoToCreate.ReleaseDate.Value.ToUniversalTime(), videoRetrieved.ReleaseDate);
            Assert.AreEqual(videoToCreate.SeriesId, videoRetrieved.SeriesId);
            Assert.AreEqual(videoToCreate.SiteURL, videoRetrieved.SiteURL);
            Assert.AreEqual(videoToCreate.SourceURL, videoRetrieved.SourceURL);
            Assert.AreEqual(videoToCreate.TimelineDate.Value.ToUniversalTime(), videoRetrieved.TimelineDate);
            Assert.AreEqual(videoToCreate.UserRating, videoRetrieved.UserRating);
            Assert.AreEqual(videoToCreate.WatchStatus, videoRetrieved.WatchStatus);
            Assert.AreEqual(videoToCreate.LibraryId, videoRetrieved.LibraryId);
            Assert.AreEqual(0, videoRetrieved.TimesWatched);
            Assert.IsNull(videoRetrieved.LastWatchDate);
            Assert.IsNotNull(videoRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestUpdateVideo() {
            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);

            var videoRetrieved = repository.GetVideo(videoId).Result;
            videoRetrieved.Description += "1";
            videoRetrieved.Title += "2";
            videoRetrieved.DurationInSeconds += 2;
            videoRetrieved.ExternalRating += 2;
            videoRetrieved.Notes += "2";
            videoRetrieved.ReleaseDate = new DateTime(1, 1, 1);
            videoRetrieved.SourceURL += "https://testsourceurlingvilt.ingvilt.ingvilt";
            videoRetrieved.TimelineDate = new DateTime(2, 2, 2);
            videoRetrieved.UserRating += 2;
            videoRetrieved.WatchStatus = VideoWatchStatus.NEED_TO_WATCH;
            videoRetrieved.SiteURL = "https://ingvilt.test2.url";
            videoRetrieved.TimesWatched = 9;
            videoRetrieved.LastWatchDate = DateTime.Now;
            repository.UpdateVideo(videoRetrieved).ConfigureAwait(false);

            var updatedVideoRetrieved = repository.GetVideo(videoId).Result;

            Assert.AreEqual(videoRetrieved.VideoId, updatedVideoRetrieved.VideoId);
            Assert.AreEqual(videoRetrieved.Title, updatedVideoRetrieved.Title);
            Assert.AreEqual(videoRetrieved.Description, updatedVideoRetrieved.Description);
            Assert.AreEqual(videoRetrieved.DurationInSeconds, updatedVideoRetrieved.DurationInSeconds);
            Assert.AreEqual(videoRetrieved.ExternalRating, updatedVideoRetrieved.ExternalRating);
            Assert.AreEqual(videoRetrieved.Notes, updatedVideoRetrieved.Notes);
            Assert.AreEqual(videoRetrieved.PublisherId, updatedVideoRetrieved.PublisherId);
            Assert.AreEqual(videoRetrieved.ReleaseDate.Value.ToUniversalTime(), updatedVideoRetrieved.ReleaseDate);
            Assert.AreEqual(videoRetrieved.SeriesId, updatedVideoRetrieved.SeriesId);
            Assert.AreEqual(videoRetrieved.SiteURL, updatedVideoRetrieved.SiteURL);
            Assert.AreEqual(videoRetrieved.SourceURL, updatedVideoRetrieved.SourceURL);
            Assert.AreEqual(videoRetrieved.TimelineDate.Value.ToUniversalTime(), updatedVideoRetrieved.TimelineDate);
            Assert.AreEqual(videoRetrieved.UserRating, updatedVideoRetrieved.UserRating);
            Assert.AreEqual(videoRetrieved.WatchStatus, updatedVideoRetrieved.WatchStatus);
            Assert.AreEqual(videoRetrieved.LibraryId, updatedVideoRetrieved.LibraryId);
            Assert.AreEqual(videoRetrieved.TimesWatched, updatedVideoRetrieved.TimesWatched);
            Assert.AreEqual(videoRetrieved.LastWatchDate.Value.ToUniversalTime(), updatedVideoRetrieved.LastWatchDate);
            CollectionAssert.AreEquivalent(new List<Video>() { updatedVideoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdateVideo_ShouldNotUpdateDifferentVideo() {
            var videoDto = GetNewVideoDetails();

            var videoToUpdateId = repository.CreateVideo(videoDto);
            var videoNotUpdatedId = repository.CreateVideo(videoDto);

            var videoToUpdate = repository.GetVideo(videoToUpdateId).Result;
            videoToUpdate.Description += "1";
            repository.UpdateVideo(videoToUpdate).ConfigureAwait(false);

            var videoToNotUpdate = repository.GetVideo(videoNotUpdatedId).Result;

            Assert.AreNotEqual(videoToUpdate.Description, videoToNotUpdate.Description);
        }

        [TestMethod]
        public void UpdateVideo_ChangingPublisherToNotNull_ShouldDeleteSeries() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));
            var publisher2 = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoDto = GetNewVideoDetails(testLibrary, publisher, series);
            var videoToUpdateId = repository.CreateVideo(videoDto);
            var videoToUpdate = repository.GetVideo(videoToUpdateId).Result;

            videoToUpdate.PublisherId = publisher2.PublisherId;
            videoToUpdate.SeriesId = DatabaseConstants.DEFAULT_ID;
            repository.UpdateVideo(videoToUpdate).ConfigureAwait(false);

            var updatedVideo = repository.GetVideo(videoToUpdateId).Result;

            Assert.AreEqual(publisher2.PublisherId, updatedVideo.PublisherId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, updatedVideo.SeriesId);
        }

        [TestMethod]
        public void UpdateVideo_ChangingPublisherToNull_ShouldNotDeleteSeries() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoDto = GetNewVideoDetails(testLibrary, publisher, series);
            var videoToUpdateId = repository.CreateVideo(videoDto);
            var videoToUpdate = repository.GetVideo(videoToUpdateId).Result;

            videoToUpdate.PublisherId = DatabaseConstants.DEFAULT_ID;
            repository.UpdateVideo(videoToUpdate).ConfigureAwait(false);

            var updatedVideo = repository.GetVideo(videoToUpdateId).Result;

            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, updatedVideo.PublisherId);
            Assert.AreEqual(videoToUpdate.SeriesId, updatedVideo.SeriesId);
        }

        [TestMethod]
        public void UpdateVideo_ChangingSeries_ShouldRemoveVideoFromSequence() {
            var seriesService = new SeriesService();
            var sequenceRepository = new SeriesSequenceRepository();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoDto = GetNewVideoDetails(testLibrary, null, series);
            var videoToUpdateId = repository.CreateVideo(videoDto);
            var videoToUpdate = repository.GetVideo(videoToUpdateId).Result;

            var sequenceDto = new CreateSeriesSequenceDto(series.SeriesId, "title", "desc", -1, true, 2);
            var sequenceId = sequenceRepository.CreateVideoSequence(sequenceDto);
            var sequence = sequenceRepository.GetSeriesSequence(sequenceId);
            sequenceRepository.AddVideoToSequence(videoToUpdateId, sequence.SequenceId);

            videoToUpdate.SeriesId = DatabaseConstants.DEFAULT_ID;
            repository.UpdateVideo(videoToUpdate).ConfigureAwait(false);

            var updatedVideo = repository.GetVideo(videoToUpdateId).Result;
            var details = sequenceRepository.GetVideoSeriesChronologyDetails(videoToUpdateId);

            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, updatedVideo.SeriesId);
            Assert.AreEqual(null, details);
        }

        [TestMethod]
        public void TestGetVideosWithNoneCreated() {
            var videos = repository.GetVideos(GetFirstPage()).Result.Results;
            Assert.AreEqual(0, videos.Count);
        }

        [TestMethod]
        public void TestGetVideosWithOneCreated() {
            var video = CreateAndRetrieveVideo();
            var videos = repository.GetVideos(GetFirstPage()).Result.Results;

            var expectedVideos = new List<Video>();
            expectedVideos.Add(video);

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void TestGetVideosWithMultipleCreated() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 5; ++i) {
                var video = CreateAndRetrieveVideo();
                expectedVideos.Add(video);
            }

            var videos = repository.GetVideos(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetVideos_ShouldntReturnDeletedVideos() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 5; ++i) {
                var video = CreateAndRetrieveVideo();
                expectedVideos.Add(video);
            }
            var deletedVideo = CreateAndRetrieveVideo();
            repository.DeleteVideo(deletedVideo.VideoId).ConfigureAwait(false);

            var videos = repository.GetVideos(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetDeletedVideos_ShouldOnlyReturnDeletedVideos() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 5; ++i) {
                var video = CreateAndRetrieveVideo();
                repository.DeleteVideo(video.VideoId).ConfigureAwait(false);
                expectedVideos.Add(video);
            }
            CreateAndRetrieveVideo();

            var videos = repository.GetDeletedVideos(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void ShouldNotDeleteVideo_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var videoToCreate = GetNewVideoDetails(otherLibrary);
            var videoId = repository.CreateVideo(videoToCreate);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteVideo_WhenLibraryRestored() {
            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteVideo_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var videoToCreate = GetNewVideoDetails(otherLibrary);
            var videoId = repository.CreateVideo(videoToCreate);

            repository.DeleteVideo(videoId).ConfigureAwait(false);
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreVideoDeletedNormally_WhenLibraryRestored() {
            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);
            repository.DeleteVideo(videoId).ConfigureAwait(false);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetVideosInLibrary_ShouldntReturnDeletedVideos() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 3; ++i) {
                var video = CreateAndRetrieveVideo();
                expectedVideos.Add(video);
            }
            var deletedVideo = CreateAndRetrieveVideo();
            repository.DeleteVideo(deletedVideo.VideoId).ConfigureAwait(false);

            var videos = repository.GetVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetVideosInLibrary_ShouldntReturnVideosInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            repository.CreateVideo(GetNewVideoDetails(otherLibrary));

            var videos = repository.GetVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void GetDeletedVideosInLibrary_ShouldOnlyReturnDeletedVideos() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 3; ++i) {
                var video = CreateAndRetrieveVideo();
                repository.DeleteVideo(video.VideoId).ConfigureAwait(false);
                expectedVideos.Add(video);
            }
            CreateAndRetrieveVideo();

            var videos = repository.GetDeletedVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetDeletedVideosInLibrary_ShouldntReturnVideosInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var videoId = repository.CreateVideo(GetNewVideoDetails(otherLibrary));
            repository.GetVideo(videoId).ConfigureAwait(false);
            repository.DeleteVideo(videoId).ConfigureAwait(false);

            var videos = repository.GetDeletedVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void RestoreDeleteVideo_ShouldUndeleteVideo() {
            var expectedVideos = new List<Video>();
            for (int i = 0; i < 2; ++i) {
                var video = CreateAndRetrieveVideo();
                expectedVideos.Add(video);
            }
            var deletedVideo = CreateAndRetrieveVideo();
            expectedVideos.Add(deletedVideo);
            repository.DeleteVideo(deletedVideo.VideoId).ConfigureAwait(false);
            repository.RestoreDeletedVideo(deletedVideo.VideoId).ConfigureAwait(false);

            var videos = repository.GetVideos(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void ShouldDeleteVideo_WhenPublisherDeleted() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));
            var videoToCreate = GetNewVideoDetails(publisher);
            var videoId = repository.CreateVideo(videoToCreate);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotDeleteVideo_WhenDifferentPublisherDeleted() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);

            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteVideo_WhenPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var videoToCreate = GetNewVideoDetails(publisher);
            var videoId = repository.CreateVideo(videoToCreate);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteVideo_WhenDifferentPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);

            repository.DeleteVideo(videoId).ConfigureAwait(false);
            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreVideoDeletedNormally_WhenPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var videoToCreate = GetNewVideoDetails(publisher);
            var videoId = repository.CreateVideo(videoToCreate);
            repository.DeleteVideo(videoId).ConfigureAwait(false);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        private CreateSeriesDto GetNewSeriesDetails() {
            return new CreateSeriesDto("", "", -1, "", -1, testLibrary.LibraryId);
        }

        [TestMethod]
        public void ShouldDeleteVideo_WhenSeriesDeleted() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());
            var videoToCreate = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(videoToCreate);

            seriesService.DeleteSeries(series).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotDeleteVideo_WhenDifferentSeriesDeleted() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            seriesService.DeleteSeries(series).ConfigureAwait(false);

            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteVideo_WhenSeriesRestored() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoToCreate = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(videoToCreate);

            seriesService.DeleteSeries(series).ConfigureAwait(false);
            seriesService.RestoreSeries(series).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteVideo_WhenDifferentSeriesRestored() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoToCreate = GetNewVideoDetails();
            var videoId = repository.CreateVideo(videoToCreate);

            repository.DeleteVideo(videoId).ConfigureAwait(false);
            seriesService.DeleteSeries(series).ConfigureAwait(false);
            seriesService.RestoreSeries(series).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreVideoDeletedNormally_WhenSeriesRestored() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var videoToCreate = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(videoToCreate);
            repository.DeleteVideo(videoId).ConfigureAwait(false);

            seriesService.DeleteSeries(series).ConfigureAwait(false);
            seriesService.RestoreSeries(series).ConfigureAwait(false);
            var videoRetrieved = repository.GetVideo(videoId).Result;

            CollectionAssert.AreEquivalent(new List<Video>() { videoRetrieved }, repository.GetDeletedVideos(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Video>(), repository.GetVideos(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetVideosByPublisher_ShouldntReturnDeletedVideos() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var expectedVideos = new List<Video>();
            for (int i = 0; i < 3; ++i) {
                var video = CreateAndRetrieveVideo(GetNewVideoDetails(publisher));
                expectedVideos.Add(video);
            }
            var deletedVideo = CreateAndRetrieveVideo(GetNewVideoDetails(publisher));
            repository.DeleteVideo(deletedVideo.VideoId).ConfigureAwait(false);

            
            var videos = repository.GetVideosByPublisher(publisher.PublisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetVideosByPublisher_ShouldntReturnVideosInOtherPublisher() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var otherPublisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));
            repository.CreateVideo(GetNewVideoDetails(otherPublisher));

            var videos = repository.GetVideosByPublisher(publisher.PublisherId, GetFirstPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void GetVideosInSeries_ShouldntReturnDeletedVideos() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var expectedVideos = new List<Video>();
            for (int i = 0; i < 3; ++i) {
                var video = CreateAndRetrieveVideo(GetNewVideoDetails(series));
                expectedVideos.Add(video);
            }
            var deletedVideo = CreateAndRetrieveVideo(GetNewVideoDetails(series));
            repository.DeleteVideo(deletedVideo.VideoId).ConfigureAwait(false);


            var videos = repository.GetVideosInSeries(series.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideos, videos);
        }

        [TestMethod]
        public void GetVideosInSeries_ShouldntReturnVideosInOtherSeries() {
            var seriesService = new SeriesService();
            var series = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());

            var otherSeries = seriesService.CreateAndRetrieveSeries(GetNewSeriesDetails());
            repository.CreateVideo(GetNewVideoDetails(otherSeries));

            var videos = repository.GetVideosInSeries(series.SeriesId, GetFirstPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void GetLastWatchedVideoInSeries_WithNoWatchedVideos_ShouldBeNull() {
            var seriesRepository = new SeriesRepository();
            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
            var series = seriesRepository.GetSeries(seriesId).Result;

            var dto = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(dto);

            var lastWatchedVideo = repository.GetLastWatchedVideoInSeries(seriesId).Result;
            Assert.AreEqual(null, lastWatchedVideo);
        }

        [TestMethod]
        public void GetLastWatchedVideoInSeries_WithWatchedVideos_ShouldReturnNull_IfNotInSeasonSequence() {
            var seriesRepository = new SeriesRepository();
            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
            var series = seriesRepository.GetSeries(seriesId).Result;

            var dto = GetNewVideoDetails(series);
            var video1Id = repository.CreateVideo(dto);
            var video2Id = repository.CreateVideo(dto);

            var sequenceRepository = new SeriesSequenceRepository();
            var sequenceId = sequenceRepository.CreateVideoSequence(new CreateSeriesSequenceDto(series.SeriesId, "", "", -1, false, 1));
            sequenceRepository.AddVideosToSequence(new List<long> { video1Id, video2Id }, sequenceId);

            var historyRepository = new WatchHistoryRepository();
            historyRepository.AddWatchedVideo(video1Id).ConfigureAwait(false);
            historyRepository.AddWatchedVideo(video2Id).ConfigureAwait(false);

            var lastWatchedVideo = repository.GetLastWatchedVideoInSeries(seriesId).Result;
            Assert.AreEqual(null, lastWatchedVideo);
        }

        [TestMethod]
        public void GetLastWatchedVideoInSeries_WithWatchedVideos_ShouldReturnLast() {
            var seriesRepository = new SeriesRepository();
            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
            var series = seriesRepository.GetSeries(seriesId).Result;

            var dto = GetNewVideoDetails(series);
            var video1Id = repository.CreateVideo(dto);

            var video2Id = repository.CreateVideo(dto);
            var video2 = repository.GetVideo(video2Id).Result;

            var sequenceRepository = new SeriesSequenceRepository();
            var sequenceId = sequenceRepository.CreateVideoSequence(new CreateSeriesSequenceDto(series.SeriesId, "", "", -1, true, 1));
            sequenceRepository.AddVideosToSequence(new List<long> { video1Id, video2Id }, sequenceId);

            var historyRepository = new WatchHistoryRepository();
            historyRepository.AddWatchedVideo(video1Id).ConfigureAwait(false);
            historyRepository.AddWatchedVideo(video2Id).ConfigureAwait(false);

            var lastWatchedVideo = repository.GetLastWatchedVideoInSeries(seriesId).Result;
            Assert.AreEqual(video2, lastWatchedVideo);
        }

        [TestMethod]
        public void GetVideosCharacterIsNotIn_WhenCharacterIsInNoVideos() {
            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var expectedList = new List<Video> { video };
            var actualList = repository.GetVideosCharacterIsNotIn(GetFirstPage(), testLibrary.LibraryId, character.CharacterId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosCharacterIsNotIn_ShouldFilterVideosByTitle() {
            var videoDto = GetNewVideoDetails();
            videoDto.Title = "Test";
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var expectedList = new List<Video>();
            var actualList = repository.GetVideosCharacterIsNotIn(GetFirstPage(), testLibrary.LibraryId, character.CharacterId, "x").Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosCharacterIsNotIn_WhenCharacterIsInVideo() {
            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var video2Id = repository.CreateVideo(videoDto);
            var video2 = repository.GetVideo(video2Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            characterRepository.AddCharacterToVideos(character, new List<Video> { video });

            var expectedList = new List<Video> { video2 };
            var actualList = repository.GetVideosCharacterIsNotIn(GetFirstPage(), testLibrary.LibraryId, character.CharacterId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosCharacterIsNotIn_WhenDifferentCharacterIsInVideo() {
            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);
            var characterInVideo = CreateAndRetrieveCharacter(characterDto);

            characterRepository.AddCharacterToVideos(characterInVideo, new List<Video> { video });

            var expectedList = new List<Video> { video };
            var actualList = repository.GetVideosCharacterIsNotIn(GetFirstPage(), testLibrary.LibraryId, character.CharacterId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosWithFile_WithNoVideos() {
            var fileRepository = new MediaFileRepository();
            var fileId = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            
            var expectedList = new List<Video> {};
            var actualList = repository.GetVideosWithFile(GetFirstPage(), fileId).Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosWithFile_WithOtherFileOnVideos() {
            var fileRepository = new MediaFileRepository();
            var fileId = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file2Id = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file2 = fileRepository.GetMediaFile(file2Id);

            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);

            fileRepository.AddFileToVideo(video1Id, file2).ConfigureAwait(false);

            var expectedList = new List<Video> { };
            var actualList = repository.GetVideosWithFile(GetFirstPage(), fileId).Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosWithFile_WithFileAttachedToVideo() {
            var fileRepository = new MediaFileRepository();
            var fileId = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = fileRepository.GetMediaFile(fileId);

            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video1 = repository.GetVideo(video1Id).Result;

            fileRepository.AddFileToVideo(video1Id, file).ConfigureAwait(false);

            var expectedList = new List<Video> { video1 };
            var actualList = repository.GetVideosWithFile(GetFirstPage(), fileId).Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosWithFile_WithFileCoverOnVideo() {
            var fileRepository = new MediaFileRepository();
            var fileId = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = fileRepository.GetMediaFile(fileId);

            var videoDto = GetNewVideoDetails();
            videoDto.CoverFileId = fileId;
            var video1Id = repository.CreateVideo(videoDto);
            var video1 = repository.GetVideo(video1Id).Result;

            fileRepository.AddFileToVideo(video1Id, file).ConfigureAwait(false);

            var expectedList = new List<Video> { video1 };
            var actualList = repository.GetVideosWithFile(GetFirstPage(), fileId).Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void GetVideosWithFile_WithFileAttachedToVideo_AndCover_ShouldntReturnDuplicates() {
            var fileRepository = new MediaFileRepository();
            var fileId = fileRepository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = fileRepository.GetMediaFile(fileId);

            var videoDto = GetNewVideoDetails();
            videoDto.CoverFileId = fileId;
            var video1Id = repository.CreateVideo(videoDto);
            var video1 = repository.GetVideo(video1Id).Result;

            fileRepository.AddFileToVideo(video1Id, file).ConfigureAwait(false);

            var expectedList = new List<Video> { video1 };
            var actualList = repository.GetVideosWithFile(GetFirstPage(), fileId).Result.Results;

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void UpsertVideos_ShouldInsertNewItems() {
            var videos = new List<ExportedVideoSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var v = new Video(-1, "t" + i, 0, null, null, 0, 0, "", "", "", "", -1, VideoWatchStatus.NEED_TO_WATCH, null, -1, testLibrary.LibraryId, null, null, -1, false, UniqueIdUtil.GenerateUniqueId());
                videos.Add(new ExportedVideoSimpleDto(v, null, null, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertVideos(videos, ids).ConfigureAwait(false);

            var retVideos = repository.GetVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var s in retVideos) {
                expectedIds[s.UniqueId] = s.VideoId;
            }

            var expectedVideos = videos.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedVideos, retVideos);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertVideos_ShouldUpdateExistingItems() {
            var videos = new List<ExportedVideoSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var v = new Video(-1, "t" + i, 0, null, null, 0, 0, "", "", "", "", -1, VideoWatchStatus.NEED_TO_WATCH, null, -1, testLibrary.LibraryId, null, null, -1, false, UniqueIdUtil.GenerateUniqueId());
                videos.Add(new ExportedVideoSimpleDto(v, null, null, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertVideos(videos, ids).ConfigureAwait(false);
            videos[0].Details.Title = "new 0";
            videos[2].Details.Title = "new 2";
            repository.UpsertVideos(videos, ids).ConfigureAwait(false);

            var retVideos = repository.GetVideosInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            var expectedVideos = videos.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedVideos, retVideos);
        }

        [TestMethod]
        public void UpsertCharactersInVideos_ShouldAddCharacters() {
            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);
            var idsMap = new Dictionary<string, long>();
            idsMap.Add(video.UniqueId, video1Id);
            idsMap.Add(character.UniqueId, character.CharacterId);

            repository.UpsertCharactersInVideos(new List<VideoCharacterActorExportDto> {
                new VideoCharacterActorExportDto(video.UniqueId, character.UniqueId, -1),
            }, idsMap).ConfigureAwait(false);

            var expectedList = new List<Video> { video };
            var actualList = repository.GetVideosCharacterIsIn(character.CharacterId).Result.Select(c => c.Video).ToList();

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public void UpsertCharactersInVideos_ShouldntAddExistingCharacters() {
            var videoDto = GetNewVideoDetails();
            var video1Id = repository.CreateVideo(videoDto);
            var video = repository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);
            var idsMap = new Dictionary<string, long>();
            idsMap.Add(video.UniqueId, video1Id);
            idsMap.Add(character.UniqueId, character.CharacterId);

            characterRepository.AddCharacterToVideos(character, new List<Video> { video });
            repository.UpsertCharactersInVideos(new List<VideoCharacterActorExportDto> {
                new VideoCharacterActorExportDto(video.UniqueId, character.UniqueId, -1),
            }, idsMap).ConfigureAwait(false);

            var expectedList = new List<Video> { video };
            var actualList = repository.GetVideosCharacterIsIn(character.CharacterId).Result.Select(c => c.Video).ToList();

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }
    }
}

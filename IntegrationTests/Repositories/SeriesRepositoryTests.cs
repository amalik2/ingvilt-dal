using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.Publishers;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTesting.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class SeriesRepositoryTests : BaseTest {
        private SeriesRepository repository = new SeriesRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();
        private PublisherRepository publisherRepository = new PublisherRepository();
        private VideoRepository videoRepository = new VideoRepository();
        private WatchHistoryRepository historyRepository = new WatchHistoryRepository();

        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public SeriesRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Series CreateAndRetrieveSeries(CreateSeriesDto dto) {
            var id = repository.CreateSeries(dto);
            return repository.GetSeries(id).Result;
        }

        private Series CreateAndRetrieveSeries() {
            return CreateAndRetrieveSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new SeriesSortCriteria(true));
        }

        [TestMethod]
        public void TestCreateSeries_WithNullLogo() {
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            Assert.AreEqual(seriesId, seriesRetrieved.SeriesId);
            Assert.AreEqual(seriesToCreate.Name, seriesRetrieved.Name);
            Assert.AreEqual(seriesToCreate.Description, seriesRetrieved.Description);
            Assert.AreEqual(seriesToCreate.LibraryId, seriesRetrieved.LibraryId);
            Assert.AreEqual(seriesToCreate.LogoFileId, seriesRetrieved.LogoFileId);
            Assert.AreEqual(seriesToCreate.SiteURL, seriesRetrieved.SiteURL);
            Assert.IsNotNull(seriesRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestCreateSeries_WithCalendar() {
            var calendarRepository = new CalendarRepository();
            var calendarId = calendarRepository.CreateCalendar(new CreateCalendarDto("MM-DD-YYYY", "", "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, calendarId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            Assert.AreEqual(seriesId, seriesRetrieved.SeriesId);
            Assert.AreEqual(seriesToCreate.Name, seriesRetrieved.Name);
            Assert.AreEqual(seriesToCreate.Description, seriesRetrieved.Description);
            Assert.AreEqual(seriesToCreate.LibraryId, seriesRetrieved.LibraryId);
            Assert.AreEqual(seriesToCreate.LogoFileId, seriesRetrieved.LogoFileId);
            Assert.AreEqual(seriesToCreate.SiteURL, seriesRetrieved.SiteURL);
            Assert.AreEqual(seriesToCreate.CalendarId, seriesRetrieved.CalendarId);
            Assert.AreNotEqual(DatabaseConstants.DEFAULT_ID, seriesRetrieved.CalendarId);
        }

        [TestMethod]
        public void TestCreateSeries_WithLogo() {
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", fileId, "test desc", -1, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            Assert.AreEqual(seriesId, seriesRetrieved.SeriesId);
            Assert.AreEqual(seriesToCreate.Name, seriesRetrieved.Name);
            Assert.AreEqual(seriesToCreate.Description, seriesRetrieved.Description);
            Assert.AreEqual(seriesToCreate.LibraryId, seriesRetrieved.LibraryId);
            Assert.AreEqual(seriesToCreate.LogoFileId, seriesRetrieved.LogoFileId);
            Assert.AreEqual(seriesToCreate.SiteURL, seriesRetrieved.SiteURL);
        }

        [TestMethod]
        public void TestUpdateSeries() {
            var calendarRepository = new CalendarRepository();
            var calendarId = calendarRepository.CreateCalendar(new CreateCalendarDto("MM-DD-YYYY", "", "", testLibrary.LibraryId));

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, calendarId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var seriesRetrieved = repository.GetSeries(seriesId).Result;
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            seriesRetrieved.Description += "1";
            seriesRetrieved.Name += "2";
            seriesRetrieved.SiteURL = "https://ingvilt.test2.url";
            seriesRetrieved.LogoFileId = fileId;
            repository.UpdateSeries(seriesRetrieved);

            var updatedSeriesRetrieved = repository.GetSeries(seriesId).Result;

            Assert.AreEqual(seriesRetrieved.SeriesId, updatedSeriesRetrieved.SeriesId);
            Assert.AreEqual(seriesRetrieved.Name, updatedSeriesRetrieved.Name);
            Assert.AreEqual(seriesRetrieved.Description, updatedSeriesRetrieved.Description);
            Assert.AreEqual(seriesRetrieved.LibraryId, updatedSeriesRetrieved.LibraryId);
            Assert.AreEqual(seriesRetrieved.LogoFileId, updatedSeriesRetrieved.LogoFileId);
            Assert.AreEqual(seriesRetrieved.SiteURL, updatedSeriesRetrieved.SiteURL);
            Assert.AreEqual(seriesRetrieved.CalendarId, updatedSeriesRetrieved.CalendarId);
            CollectionAssert.AreEquivalent(new List<Series>() { updatedSeriesRetrieved }, repository.GetSeries(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdateSeries_ShouldNotUpdateDifferentSeries() {
            var seriesDto = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);

            var seriesToUpdateId = repository.CreateSeries(seriesDto);
            var seriesNotUpdatedId = repository.CreateSeries(seriesDto);

            var seriesToUpdate = repository.GetSeries(seriesToUpdateId).Result;
            seriesToUpdate.Description += "1";
            repository.UpdateSeries(seriesToUpdate);

            var seriesToNotUpdate = repository.GetSeries(seriesNotUpdatedId).Result;

            Assert.AreNotEqual(seriesToUpdate.Description, seriesToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetSeriesWithNoneCreated() {
            var series = repository.GetSeries(GetFirstPage()).Result.Results;
            Assert.AreEqual(0, series.Count);
        }

        [TestMethod]
        public void TestGetSeriesWithOneCreated() {
            var series = CreateAndRetrieveSeries();
            var allSeries = repository.GetSeries(GetFirstPage()).Result.Results;

            var expectedSeries = new List<Series>();
            expectedSeries.Add(series);

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void TestGetSeriesWithMultipleCreated() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 5; ++i) {
                var series = CreateAndRetrieveSeries();
                expectedSeries.Add(series);
            }

            var allSeries = repository.GetSeries(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void GetSeries_ShouldntReturnDeletedSeries() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 5; ++i) {
                var series = CreateAndRetrieveSeries();
                expectedSeries.Add(series);
            }
            var deletedSeries = CreateAndRetrieveSeries();
            repository.DeleteSeries(deletedSeries.SeriesId).ConfigureAwait(false);

            var allSeries = repository.GetSeries(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void GetDeletedSeries_ShouldOnlyReturnDeletedSeries() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 5; ++i) {
                var series = CreateAndRetrieveSeries();
                repository.DeleteSeries(series.SeriesId).ConfigureAwait(false);
                expectedSeries.Add(series);
                series.Deleted = true;
            }
            CreateAndRetrieveSeries();

            var allSeries = repository.GetDeletedSeries(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void ShouldNotDeleteSeries_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, otherLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);
            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Series>(), repository.GetDeletedSeries(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Series>() { seriesRetrieved }, repository.GetSeries(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteSeries_WhenLibraryRestored() {
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            CollectionAssert.AreEquivalent(new List<Series>(), repository.GetDeletedSeries(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Series>() { seriesRetrieved }, repository.GetSeries(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteSeries_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, otherLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            repository.DeleteSeries(seriesId).ConfigureAwait(false);
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            CollectionAssert.AreEquivalent(new List<Series>() { seriesRetrieved }, repository.GetDeletedSeries(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Series>(), repository.GetSeries(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreSeriesDeletedNormally_WhenLibraryRestored() {
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);
            repository.DeleteSeries(seriesId).ConfigureAwait(false);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var seriesRetrieved = repository.GetSeries(seriesId).Result;

            CollectionAssert.AreEquivalent(new List<Series>() { seriesRetrieved }, repository.GetDeletedSeries(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Series>(), repository.GetSeries(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetSeriesInLibrary_ShouldntReturnDeletedSeries() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 3; ++i) {
                var series = CreateAndRetrieveSeries();
                expectedSeries.Add(series);
            }
            var deletedSeries = CreateAndRetrieveSeries();
            repository.DeleteSeries(deletedSeries.SeriesId).ConfigureAwait(false);

            var allSeries = repository.GetSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void GetSeriesInLibrary_ShouldntReturnSeriesInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            repository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, otherLibrary.LibraryId));

            var series = repository.GetSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Series>(), series);
        }

        [TestMethod]
        public void GetDeletedSeriesInLibrary_ShouldOnlyReturnDeletedSeries() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 3; ++i) {
                var series = CreateAndRetrieveSeries();
                repository.DeleteSeries(series.SeriesId).ConfigureAwait(false);
                expectedSeries.Add(series);
                series.Deleted = true;
            }
            CreateAndRetrieveSeries();

            var allSeries = repository.GetDeletedSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void GetDeletedSeriesInLibrary_ShouldntReturnSeriesInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var seriesId = repository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, otherLibrary.LibraryId));
            repository.GetSeries(seriesId).ConfigureAwait(false);
            repository.DeleteSeries(seriesId).ConfigureAwait(false);

            var series = repository.GetDeletedSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Series>(), series);
        }

        [TestMethod]
        public void RestoreDeleteSeries_ShouldUndeleteSeries() {
            var expectedSeries = new List<Series>();
            for (int i = 0; i < 2; ++i) {
                var series = CreateAndRetrieveSeries();
                expectedSeries.Add(series);
            }
            var deletedSeries = CreateAndRetrieveSeries();
            expectedSeries.Add(deletedSeries);
            repository.DeleteSeries(deletedSeries.SeriesId).ConfigureAwait(false);
            repository.RestoreDeletedSeries(deletedSeries.SeriesId).ConfigureAwait(false);

            var allSeries = repository.GetSeries(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, allSeries);
        }

        [TestMethod]
        public void GetSeriesByPublisher_WithNoSeriesByAnyPublisher() {
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);
            repository.CreateSeries(seriesToCreate);

            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var allSeries = repository.GetSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>(), allSeries);
        }

        [TestMethod]
        public void GetSeriesByPublisher_WithSeriesByDifferentPublisher() {
            var seriesCreatorPublisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", seriesCreatorPublisherId, testLibrary.LibraryId);
            repository.CreateSeries(seriesToCreate);

            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var allSeries = repository.GetSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>(), allSeries);
        }

        [TestMethod]
        public void GetSeriesByPublisher_WithSeriesByPublisher() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", publisherId, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var series = repository.GetSeries(seriesId).Result;

            var allSeries = repository.GetSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>() { series }, allSeries);
        }

        [TestMethod]
        public void GetSeriesByPublisher_WithDeletedSeries() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", publisherId, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);
            repository.DeleteSeries(seriesId).ConfigureAwait(false);

            var allSeries = repository.GetSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>(), allSeries);
        }

        [TestMethod]
        public void GetDeletedSeriesByPublisher_WithNoDeletedSeries() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", publisherId, testLibrary.LibraryId);
            repository.CreateSeries(seriesToCreate);

            var allSeries = repository.GetDeletedSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>(), allSeries);
        }

        [TestMethod]
        public void GetDeletedSeriesByPublisher_WithDeletedSeries() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", publisherId, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);
            repository.DeleteSeries(seriesId).ConfigureAwait(false);
            var series = repository.GetSeries(seriesId).Result;

            var allSeries = repository.GetDeletedSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>() { series }, allSeries);
        }

        [TestMethod]
        public void ShouldDeleteSeries_WhenPublisherIsDeleted() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", publisherId, testLibrary.LibraryId);
            var seriesId = repository.CreateSeries(seriesToCreate);

            publisherRepository.DeletePublisher(publisherId).ConfigureAwait(false);

            var series = repository.GetSeries(seriesId).Result;
            var allSeries = repository.GetDeletedSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>() { series }, allSeries);
        }

        [TestMethod]
        public void ShouldNotDeleteSeries_WhenDifferentPublisherIsDeleted() {
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId);
            repository.CreateSeries(seriesToCreate);

            publisherRepository.DeletePublisher(publisherId).ConfigureAwait(false);

            var allSeries = repository.GetDeletedSeriesByPublisher(publisherId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(new List<Series>(), allSeries);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithNoVideosInSeries() {
            for (int i = 0; i < 2; ++i) {
                CreateAndRetrieveSeries();
            }

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(0, librariesCount);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithDeletedSeries() {
            var activeSeries = CreateAndRetrieveSeries();
            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, activeSeries.SeriesId);
            createVideoDto.TimesWatched = 1;
            videoRepository.CreateVideo(createVideoDto);

            var deletedSeries = CreateAndRetrieveSeries();
            repository.DeleteSeries(deletedSeries.SeriesId).ConfigureAwait(false);

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(1, librariesCount);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithUnfinishedSeries() {
            for (int i = 0; i < 1; ++i) {
                var series = CreateAndRetrieveSeries();
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                videoRepository.CreateVideo(createVideoDto);
            }

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(0, librariesCount);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithDeletedVideoInUnfinishedSeries() {
            var series = CreateAndRetrieveSeries();
            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);
            videoRepository.DeleteVideo(videoId).ConfigureAwait(false);

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(0, librariesCount);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithFinishedSeries() {
            for (int i = 0; i < 1; ++i) {
                var series = CreateAndRetrieveSeries();
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                var videoId = videoRepository.CreateVideo(createVideoDto);
                var video = videoRepository.GetVideo(videoId).Result;
                video.TimesWatched = 1;
                videoRepository.UpdateVideo(video).ConfigureAwait(false);
            }

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(1, librariesCount);
        }

        [TestMethod]
        public void GetNumberOfFinishedSeriesInLibrary_WithHalfFinishedSeries() {
            for (int i = 0; i < 1; ++i) {
                var series = CreateAndRetrieveSeries();
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                var videoId = videoRepository.CreateVideo(createVideoDto);
                var video = videoRepository.GetVideo(videoId).Result;
                video.TimesWatched = 1;
                videoRepository.UpdateVideo(video).ConfigureAwait(false);

                var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                videoRepository.CreateVideo(createVideoDto2);
            }

            var librariesCount = repository.GetNumberOfFinishedSeriesInLibrary(testLibrary.LibraryId).Result;
            Assert.AreEqual(0, librariesCount);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_WithNoWatchedVideos() {
            var series = CreateAndRetrieveSeries();

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 5).Result;
            CollectionAssert.AreEqual(new List<Series>(), recentlyWatched);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_WithWatchedVideo() {
            var series = CreateAndRetrieveSeries();
            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);
            historyRepository.AddWatchedVideo(videoId).ConfigureAwait(false);

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 2).Result;
            CollectionAssert.AreEqual(new List<Series> { series }, recentlyWatched);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_WithMultipleVideosInSameSeries() {
            var series = CreateAndRetrieveSeries();

            for (int i = 0; i < 3; ++i) {
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                var videoId = videoRepository.CreateVideo(createVideoDto);
                historyRepository.AddWatchedVideo(videoId).ConfigureAwait(false);
            }

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 2).Result;
            CollectionAssert.AreEqual(new List<Series> { series }, recentlyWatched);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_WithMultipleVideosInDifferentSeries() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();

            for (int i = 0; i < 2; ++i) {
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                var videoId = videoRepository.CreateVideo(createVideoDto);
                historyRepository.AddWatchedVideo(videoId).ConfigureAwait(false);

                var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
                var video2Id = videoRepository.CreateVideo(createVideoDto2);
                historyRepository.AddWatchedVideo(video2Id).ConfigureAwait(false);
            }

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 2).Result;
            CollectionAssert.AreEqual(new List<Series> { series2, series }, recentlyWatched);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_ShouldLimitReturnedCount() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);
            historyRepository.AddWatchedVideo(videoId).ConfigureAwait(false);

            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
            var video2Id = videoRepository.CreateVideo(createVideoDto2);
            historyRepository.AddWatchedVideo(video2Id).ConfigureAwait(false);

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 1).Result;
            CollectionAssert.AreEqual(new List<Series> { series2 }, recentlyWatched);
        }

        [TestMethod]
        public void GetRecentlyWatchedSeriesInLibrary_WithMoreVideosInOneSeries_ThanMaxReturnCount() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();

            for (int i = 0; i < 4; ++i) {
                var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
                var videoId = videoRepository.CreateVideo(createVideoDto);
                historyRepository.AddWatchedVideo(videoId).ConfigureAwait(false);
            }

            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
            var video2Id = videoRepository.CreateVideo(createVideoDto2);
            historyRepository.AddWatchedVideo(video2Id).ConfigureAwait(false);

            var recentlyWatched = repository.GetRecentlyWatchedSeriesInLibrary(testLibrary.LibraryId, 2).Result;
            CollectionAssert.AreEqual(new List<Series> { series2, series }, recentlyWatched);
        }

        [TestMethod]
        public void UpdatingSeriesPublisher_ShouldUpdatePublisherId_OfVideosInSeries() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);

            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
            var video2Id = videoRepository.CreateVideo(createVideoDto2);

            series.PublisherId = publisherId;
            repository.UpdateSeries(series);

            var video1 = videoRepository.GetVideo(videoId).Result;
            var video2 = videoRepository.GetVideo(video2Id).Result;

            Assert.AreEqual(publisherId, video1.PublisherId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, video2.PublisherId);
        }

        [TestMethod]
        public void UpdatingSeriesPublisher_ShouldUpdatePublisherId_OfVideosInSeries_WhenInitialPublisherSet() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();
            var publisherId = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
            var publisherId2 = publisherRepository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, publisherId2, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);

            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, publisherId2, series2.SeriesId);
            var video2Id = videoRepository.CreateVideo(createVideoDto2);

            series.PublisherId = publisherId;
            repository.UpdateSeries(series);

            var video1 = videoRepository.GetVideo(videoId).Result;
            var video2 = videoRepository.GetVideo(video2Id).Result;

            Assert.AreEqual(publisherId, video1.PublisherId);
            Assert.AreEqual(publisherId2, video2.PublisherId);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "A video was not deleted")]
        public void PermanentlyRemoveSeries_ShouldRemoveVideos() {
            var series = CreateAndRetrieveSeries();
            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            var videoId = videoRepository.CreateVideo(createVideoDto);

            repository.PermanentlyRemoveSeries(series.SeriesId).ConfigureAwait(false);
            var video = videoRepository.GetVideo(videoId).Result;
        }

        [TestMethod]
        public void GetSeriesRating_WithNoVideos() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();
            
            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
            createVideoDto2.UserRating = 3;
            videoRepository.CreateVideo(createVideoDto2);

            var seriesRating = repository.GetSeriesRating(series.SeriesId).Result;

            Assert.AreEqual(0, seriesRating.UserRating);
            Assert.AreEqual(0, seriesRating.ExternalRating);
        }

        [TestMethod]
        public void GetSeriesRating_WithVideos() {
            var series = CreateAndRetrieveSeries();
            var series2 = CreateAndRetrieveSeries();

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            createVideoDto.UserRating = 3;
            createVideoDto.ExternalRating = 5;

            videoRepository.CreateVideo(createVideoDto);
            createVideoDto.UserRating = 5;
            createVideoDto.ExternalRating = 6;
            videoRepository.CreateVideo(createVideoDto);

            var createVideoDto2 = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series2.SeriesId);
            createVideoDto2.UserRating = 3;
            videoRepository.CreateVideo(createVideoDto2);

            var seriesRating = repository.GetSeriesRating(series.SeriesId).Result;

            Assert.AreEqual(4, seriesRating.UserRating);
            Assert.AreEqual(5.5, seriesRating.ExternalRating);
        }

        [TestMethod]
        public void GetSeriesRating_WithVideos_ShouldntIncludeVideosWithZeroAsRating() {
            var series = CreateAndRetrieveSeries();

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            createVideoDto.UserRating = 3;
            createVideoDto.ExternalRating = 5;

            videoRepository.CreateVideo(createVideoDto);
            createVideoDto.UserRating = 0;
            createVideoDto.ExternalRating = 0;
            videoRepository.CreateVideo(createVideoDto);

            var seriesRating = repository.GetSeriesRating(series.SeriesId).Result;

            Assert.AreEqual(3, seriesRating.UserRating);
            Assert.AreEqual(5, seriesRating.ExternalRating);
        }

        [TestMethod]
        public void GetSeriesRating_WithVideos_NullExternalRating() {
            var series = CreateAndRetrieveSeries();

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            createVideoDto.UserRating = 3;
            createVideoDto.ExternalRating = 0;
            videoRepository.CreateVideo(createVideoDto);

            var seriesRating = repository.GetSeriesRating(series.SeriesId).Result;

            Assert.AreEqual(3, seriesRating.UserRating);
            Assert.AreEqual(0, seriesRating.ExternalRating);
        }

        [TestMethod]
        public void GetSeriesRating_WithVideos_NullUserRating() {
            var series = CreateAndRetrieveSeries();

            var createVideoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, series.SeriesId);
            createVideoDto.UserRating = 0;
            createVideoDto.ExternalRating = 3;
            videoRepository.CreateVideo(createVideoDto);

            var seriesRating = repository.GetSeriesRating(series.SeriesId).Result;

            Assert.AreEqual(0, seriesRating.UserRating);
            Assert.AreEqual(3, seriesRating.ExternalRating);
        }

        [TestMethod]
        public void UpsertSeries_ShouldInsertNewItems() {
            var series = new List<ExportedSeriesSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new Series(-1, "test " + i, "", -1, "", -1, testLibrary.LibraryId, -1, false, UniqueIdUtil.GenerateUniqueId());
                series.Add(new ExportedSeriesSimpleDto(s, null, null, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertSeries(series, ids).ConfigureAwait(false);

            var retSeries = repository.GetSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var s in retSeries) {
                expectedIds[s.UniqueId] = s.SeriesId;
            }

            var expectedSeries = series.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSeries, retSeries);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertSeries_ShouldUpdateExistingItems() {
            var series = new List<ExportedSeriesSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new Series(-1, "test " + i, "", -1, "", -1, testLibrary.LibraryId, -1, false, UniqueIdUtil.GenerateUniqueId());
                series.Add(new ExportedSeriesSimpleDto(s, null, null, null));
            }

            repository.UpsertSeries(series, new Dictionary<string, long>()).ConfigureAwait(false);
            series[0].Details.Name = "new 0";
            series[2].Details.Name = "new 2";
            repository.UpsertSeries(series, new Dictionary<string, long>()).ConfigureAwait(false);

            var retSeries = repository.GetSeriesInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            var expectedSeries = series.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSeries, retSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_WithUnfinishedSeriesToWatch() {
            var videoRepository = new VideoRepository();
            
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            videoRepository.CreateVideo(videoToCreate);
            videoToCreate.TimesWatched = 1;
            videoRepository.CreateVideo(videoToCreate);

            var expectedSeries = new List<Series> { repository.GetSeries(seriesId).Result };
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_WithNotStartedSeriesToWatch() {
            var videoRepository = new VideoRepository();

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            videoRepository.CreateVideo(videoToCreate);
            videoRepository.CreateVideo(videoToCreate);

            var expectedSeries = new List<Series> { repository.GetSeries(seriesId).Result };
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_WithUnfinishedSeriesToWatch_OneVideoWatchCountGreaterThanTotalCount() {
            var videoRepository = new VideoRepository();

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            videoRepository.CreateVideo(videoToCreate);
            videoToCreate.TimesWatched = 5;
            videoRepository.CreateVideo(videoToCreate);

            var expectedSeries = new List<Series> { repository.GetSeries(seriesId).Result };
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_WithFinishedSeriesToWatch() {
            var videoRepository = new VideoRepository();

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            videoToCreate.TimesWatched = 1;
            videoRepository.CreateVideo(videoToCreate);
            videoRepository.CreateVideo(videoToCreate);

            var expectedSeries = new List<Series> {};
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_ShouldntIncludeSeriesNotWorthWatching() {
            var videoRepository = new VideoRepository();

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, false);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            videoRepository.CreateVideo(videoToCreate);
            videoToCreate.TimesWatched = 5;
            videoRepository.CreateVideo(videoToCreate);

            var expectedSeries = new List<Series> {};
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_ShouldntIncludeDeletedVideos() {
            var videoRepository = new VideoRepository();

            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);

            var videoToCreate = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, seriesId);
            var videoToDeleteId = videoRepository.CreateVideo(videoToCreate);
            videoToCreate.TimesWatched = 5;
            videoRepository.CreateVideo(videoToCreate);

            videoRepository.DeleteVideo(videoToDeleteId).ConfigureAwait(false);

            var expectedSeries = new List<Series> {};
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }

        [TestMethod]
        public void GetSeriesToWatchInLibrary_WithNoVideosInSeries() {
            var seriesToCreate = new CreateSeriesDto("Test library", "https://ingvilt.test.url", -1, "test desc", -1, testLibrary.LibraryId, -1, true);
            var seriesId = repository.CreateSeries(seriesToCreate);
            var series = repository.GetSeries(seriesId).Result;

            var expectedSeries = new List<Series> { series };
            var actualSeries = repository.GetSeriesToWatchInLibrary(GetFirstPage(), testLibrary.LibraryId, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedSeries, actualSeries);
        }
    }
}

using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTesting.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class SeriesSequenceRepositoryTests : BaseTest {
        private SeriesSequenceRepository repository = new SeriesSequenceRepository();
        private SeriesService seriesService = new SeriesService();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();
        private VideoRepository videoRepository = new VideoRepository();

        private Series testSeries = null;
        private Series otherSeries = null;
        private Video testVideo = null;
        private Video otherVideo = null;

        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = new LibraryRepository().CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }
        private Video CreateAndRetrieveVideo(long libraryId, long seriesId) {
            var dto = CreateVideoUtil.GetNewVideoDetails(libraryId, -1, seriesId);
            var videoId = videoRepository.CreateVideo(dto);
            return videoRepository.GetVideo(videoId).Result;
        }

        public SeriesSequenceRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));

            var seriesDto = new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId);

            testSeries = seriesService.CreateAndRetrieveSeries(seriesDto);
            otherSeries = seriesService.CreateAndRetrieveSeries(seriesDto);

            testVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, testSeries.SeriesId);
            otherVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, otherSeries.SeriesId);
        }

        private SeriesSequence CreateAndRetrieveVideoSequence(CreateVideoSequenceDto dto) {
            var id = repository.CreateVideoSequence(dto);
            return repository.GetSeriesSequence(id);
        }

        private SeriesSequence CreateAndRetrieveVideoSequence() {
            return CreateAndRetrieveVideoSequence(CreateDto());
        }

        protected CreateVideoSequenceDto CreateDto(long seriesId, long coverFileId = DatabaseConstants.DEFAULT_ID) {
            return new CreateSeriesSequenceDto(seriesId, "title", "desc", coverFileId, false, -1);
        }

        protected CreateVideoSequenceDto CreateDto() {
            return CreateDto(testSeries.SeriesId);
        }

        protected CreateVideoSequenceDto CreateDtoWithCover(long coverFileId) {
            return CreateDto(testSeries.SeriesId, coverFileId);
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new SequenceSortCriteria(true));
        }

        private Pagination GetFirstVideoPage() {
            return Pagination.FirstPageWithDefaultCount(new VideoCreationDateSortCriteria(true));
        }

        [TestMethod]
        public void TestCreateVideoSequence_WithNullLogo() {
            var sequenceToCreate = CreateDto();
            var sequenceId = repository.CreateVideoSequence(sequenceToCreate);

            var sequenceRetrieved = repository.GetSeriesSequence(sequenceId);

            Assert.AreEqual(sequenceId, sequenceRetrieved.SequenceId);
            Assert.AreEqual(sequenceToCreate.Title, sequenceRetrieved.Title);
            Assert.AreEqual(sequenceToCreate.Description, sequenceRetrieved.Description);
            Assert.AreEqual(sequenceToCreate.CoverFile, sequenceRetrieved.CoverFile);
            Assert.IsNotNull(sequenceRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestCreateVideoSequence_WithLogo() {
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            var sequenceToCreate = CreateDtoWithCover(fileId);
            var sequenceId = repository.CreateVideoSequence(sequenceToCreate);

            var sequenceRetrieved = repository.GetSeriesSequence(sequenceId);

            Assert.AreEqual(sequenceId, sequenceRetrieved.SequenceId);
            Assert.AreEqual(sequenceToCreate.Title, sequenceRetrieved.Title);
            Assert.AreEqual(sequenceToCreate.Description, sequenceRetrieved.Description);
            Assert.AreEqual(sequenceToCreate.CoverFile, sequenceRetrieved.CoverFile);
        }

        [TestMethod]
        public void TestUpdateVideoSequence() {
            var sequenceToCreate = CreateDto();
            var sequenceId = repository.CreateVideoSequence(sequenceToCreate);

            var sequenceRetrieved = repository.GetSeriesSequence(sequenceId);
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            sequenceRetrieved.Description += "1";
            sequenceRetrieved.Title += "2";
            sequenceRetrieved.CoverFile = fileId;
            repository.UpdateVideoSequence(sequenceRetrieved);

            var updatedVideoSequenceRetrieved = repository.GetSeriesSequence(sequenceId);

            Assert.AreEqual(sequenceRetrieved.SequenceId, updatedVideoSequenceRetrieved.SequenceId);
            Assert.AreEqual(sequenceRetrieved.Title, updatedVideoSequenceRetrieved.Title);
            Assert.AreEqual(sequenceRetrieved.Description, updatedVideoSequenceRetrieved.Description);
            Assert.AreEqual(sequenceRetrieved.CoverFile, updatedVideoSequenceRetrieved.CoverFile);
            CollectionAssert.AreEquivalent(new List<SeriesSequence>() { updatedVideoSequenceRetrieved }, repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdateVideoSequence_ShouldNotUpdateDifferentVideoSequence() {
            var sequenceDto = CreateDto();

            var sequenceToUpdateId = repository.CreateVideoSequence(sequenceDto);
            sequenceDto.UniqueId = UniqueIdUtil.GenerateUniqueId();
            var sequenceNotUpdatedId = repository.CreateVideoSequence(sequenceDto);

            var sequenceToUpdate = repository.GetSeriesSequence(sequenceToUpdateId);
            sequenceToUpdate.Description += "1";
            repository.UpdateVideoSequence(sequenceToUpdate);

            var sequenceToNotUpdate = repository.GetSeriesSequence(sequenceNotUpdatedId);

            Assert.AreNotEqual(sequenceToUpdate.Description, sequenceToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetVideoSequencesWithNoneCreated() {
            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;
            Assert.AreEqual(0, sequences.Count);
        }

        [TestMethod]
        public void TestGetVideoSequencesWithOneCreated() {
            var sequence = CreateAndRetrieveVideoSequence();
            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            var expectedVideoSequences = new List<SeriesSequence>();
            expectedVideoSequences.Add(sequence);

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void TestGetVideoSequencesWithMultipleCreated() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 5; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                expectedVideoSequences.Add(sequence);
            }

            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void GetVideoSequences_ShouldntReturnDeletedVideoSequences() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 5; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                expectedVideoSequences.Add(sequence);
            }
            var deletedVideoSequence = CreateAndRetrieveVideoSequence();
            repository.DeleteVideoSequence(deletedVideoSequence.SequenceId);

            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void GetDeletedVideoSequences_ShouldOnlyReturnDeletedVideoSequences() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 5; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                repository.DeleteVideoSequence(sequence.SequenceId);
                expectedVideoSequences.Add(sequence);
                sequence.Deleted = true;
            }
            CreateAndRetrieveVideoSequence();

            var sequences = repository.GetDeletedVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void ShouldNotUndeleteVideoSequence_WhenDifferentSeriesRestored() {
            var sequenceRetrieved = CreateAndRetrieveVideoSequence(CreateDto(otherSeries.SeriesId));

            repository.DeleteVideoSequence(sequenceRetrieved.SequenceId);
            sequenceRetrieved.Deleted = true;
            seriesService.DeleteSeries(testSeries);
            seriesService.RestoreSeries(testSeries);

            CollectionAssert.AreEquivalent(new List<SeriesSequence>() { sequenceRetrieved }, repository.GetDeletedVideoSequencesInSeries(otherSeries.SeriesId, GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<SeriesSequence>(), repository.GetVideoSequencesInSeries(otherSeries.SeriesId, GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreVideoSequenceDeletedNormally_WhenSeriesRestored() {
            var sequenceRetrieved = CreateAndRetrieveVideoSequence();

            repository.DeleteVideoSequence(sequenceRetrieved.SequenceId);
            sequenceRetrieved.Deleted = true;
            seriesService.DeleteSeries(testSeries);
            seriesService.RestoreSeries(testSeries);

            CollectionAssert.AreEquivalent(new List<SeriesSequence>() { sequenceRetrieved }, repository.GetDeletedVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<SeriesSequence>(), repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetVideoSequencesInSeries_ShouldntReturnDeletedVideoSequences() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 3; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                expectedVideoSequences.Add(sequence);
            }
            var deletedVideoSequence = CreateAndRetrieveVideoSequence();
            repository.DeleteVideoSequence(deletedVideoSequence.SequenceId);

            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void GetVideoSequencesInSeries_ShouldntReturnVideoSequencesInOtherSeries() {
            CreateAndRetrieveVideoSequence(CreateDto(otherSeries.SeriesId));

            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<SeriesSequence>(), sequences);
        }

        [TestMethod]
        public void GetDeletedVideoSequencesInSeries_ShouldOnlyReturnDeletedVideoSequences() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 3; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                repository.DeleteVideoSequence(sequence.SequenceId);
                expectedVideoSequences.Add(sequence);
                sequence.Deleted = true;
            }
            CreateAndRetrieveVideoSequence();

            var sequences = repository.GetDeletedVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void GetDeletedVideoSequencesInSeries_ShouldntReturnVideoSequencesInOtherSeries() {
            var sequence = CreateAndRetrieveVideoSequence(CreateDto(otherSeries.SeriesId));
            repository.GetSeriesSequence(sequence.SequenceId);
            repository.DeleteVideoSequence(sequence.SequenceId);

            var sequences = repository.GetDeletedVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<SeriesSequence>(), sequences);
        }

        [TestMethod]
        public void RestoreDeleteVideoSequence_ShouldUndeleteVideoSequence() {
            var expectedVideoSequences = new List<SeriesSequence>();
            for (int i = 0; i < 2; ++i) {
                var sequence = CreateAndRetrieveVideoSequence();
                expectedVideoSequences.Add(sequence);
            }
            var deletedVideoSequence = CreateAndRetrieveVideoSequence();
            expectedVideoSequences.Add(deletedVideoSequence);
            repository.DeleteVideoSequence(deletedVideoSequence.SequenceId);
            repository.RestoreDeletedVideoSequence(deletedVideoSequence.SequenceId);

            var sequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedVideoSequences, sequences);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithVideoNotInASequence() {
            CreateAndRetrieveVideoSequence();
            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(null, details);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithVideoInASequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(seasonNumber, details.SeasonNumber);
            Assert.AreEqual(1, details.EpisodeNumber);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithMultipleVideosInASequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(otherVideo.VideoId, sequence.SequenceId);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(seasonNumber, details.SeasonNumber);
            Assert.AreEqual(2, details.EpisodeNumber);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithVideoInADifferentSequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(otherVideo.VideoId, sequence.SequenceId);

            var dto2 = new CreateSeriesSequenceDto(otherSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence2 = CreateAndRetrieveVideoSequence(dto2);
            repository.AddVideoToSequence(testVideo.VideoId, sequence2.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(seasonNumber, details.SeasonNumber);
            Assert.AreEqual(1, details.EpisodeNumber);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithVideoInANonSeasonSequence() {
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 2);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(null, details);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithVideoInADeletedSeasonSequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);
            repository.DeleteVideoSequence(sequence.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(null, details);
        }

        [TestMethod]
        public void GetVideoSeriesChronologyDetails_WithAnotherVideoInASeasonSequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);

            var details = repository.GetVideoSeriesChronologyDetails(otherVideo.VideoId);
            Assert.AreEqual(null, details);
        }

        [TestMethod]
        public void TestGetSeriesSequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var videoSequence = CreateAndRetrieveVideoSequence(dto);
            var seriesSequence = repository.GetSeriesSequence(videoSequence.SequenceId);

            Assert.AreEqual(seasonNumber, seriesSequence.SeasonNumber);
            Assert.AreEqual(true, seriesSequence.IsSeason);
            Assert.AreEqual(videoSequence.Title, seriesSequence.Title);
            Assert.AreEqual(videoSequence.Description, seriesSequence.Description);
            Assert.AreEqual(videoSequence.CoverFile, seriesSequence.CoverFile);
        }

        [TestMethod]
        public void RemoveVideoFromSequence_ShouldDecrementOrderOfLaterVideos() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);

            var thirdVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, testSeries.SeriesId);
            var fourthVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, testSeries.SeriesId);

            repository.AddVideoToSequence(otherVideo.VideoId, sequence.SequenceId);
            repository.AddVideoToSequence(testVideo.VideoId, sequence.SequenceId);
            repository.AddVideoToSequence(thirdVideo.VideoId, sequence.SequenceId);
            repository.AddVideoToSequence(fourthVideo.VideoId, sequence.SequenceId);

            Assert.AreEqual(1, repository.GetVideoSeriesChronologyDetails(otherVideo.VideoId).EpisodeNumber);
            Assert.AreEqual(2, repository.GetVideoSeriesChronologyDetails(testVideo.VideoId).EpisodeNumber);
            Assert.AreEqual(3, repository.GetVideoSeriesChronologyDetails(thirdVideo.VideoId).EpisodeNumber);
            Assert.AreEqual(4, repository.GetVideoSeriesChronologyDetails(fourthVideo.VideoId).EpisodeNumber);

            repository.RemoveVideoFromSequence(testVideo.VideoId, sequence.SequenceId);

            Assert.AreEqual(1, repository.GetVideoSeriesChronologyDetails(otherVideo.VideoId).EpisodeNumber);
            Assert.AreEqual(2, repository.GetVideoSeriesChronologyDetails(thirdVideo.VideoId).EpisodeNumber);
            Assert.AreEqual(3, repository.GetVideoSeriesChronologyDetails(fourthVideo.VideoId).EpisodeNumber);
        }

        [TestMethod]
        public void AddVideosToSequence() {
            var seasonNumber = 5;
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, seasonNumber);
            var sequence = CreateAndRetrieveVideoSequence(dto);

            var thirdVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, testSeries.SeriesId);
            var fourthVideo = CreateAndRetrieveVideo(testLibrary.LibraryId, testSeries.SeriesId);
            var videos = new List<long>() { otherVideo.VideoId, testVideo.VideoId, thirdVideo.VideoId, fourthVideo.VideoId };

            repository.AddVideosToSequence(videos, sequence.SequenceId);

            var video1Details = repository.GetVideoSeriesChronologyDetails(otherVideo.VideoId);
            Assert.AreEqual(seasonNumber, video1Details.SeasonNumber);
            Assert.AreEqual(1, video1Details.EpisodeNumber);

            var video2Details = repository.GetVideoSeriesChronologyDetails(testVideo.VideoId);
            Assert.AreEqual(seasonNumber, video2Details.SeasonNumber);
            Assert.AreEqual(2, video2Details.EpisodeNumber);

            var video3Details = repository.GetVideoSeriesChronologyDetails(thirdVideo.VideoId);
            Assert.AreEqual(seasonNumber, video3Details.SeasonNumber);
            Assert.AreEqual(3, video3Details.EpisodeNumber);

            var video4Details = repository.GetVideoSeriesChronologyDetails(fourthVideo.VideoId);
            Assert.AreEqual(seasonNumber, video4Details.SeasonNumber);
            Assert.AreEqual(4, video4Details.EpisodeNumber);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_ShouldntIncludeVideosInOtherSeries() {
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 1);
            var sequence = CreateAndRetrieveVideoSequence(dto);

            var videos = videoRepository.GetVideosNotInSeriesSequence(sequence, sequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { testVideo }, videos);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_ShouldntIncludeDeletedVideos() {
            var dto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 1);
            var sequence = CreateAndRetrieveVideoSequence(dto);

            videoRepository.DeleteVideo(testVideo.VideoId);

            var videos = videoRepository.GetVideosNotInSeriesSequence(sequence, sequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_WithNonSeasonSequence_ShouldIncludeVideosInSeasons() {
            var seasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, 1);
            var seasonSequence = CreateAndRetrieveVideoSequence(seasonDto);
            repository.AddVideoToSequence(testVideo.VideoId, seasonSequence.SequenceId);

            var nonSeasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 1);
            var nonSeasonSequence = CreateAndRetrieveVideoSequence(nonSeasonDto);

            var videos = videoRepository.GetVideosNotInSeriesSequence(nonSeasonSequence, nonSeasonSequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { testVideo }, videos);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_WithNonSeasonSequence_ShouldntIncludeAlreadyIncludedVideos() {
            var nonSeasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 1);
            var nonSeasonSequence = CreateAndRetrieveVideoSequence(nonSeasonDto);
            repository.AddVideoToSequence(testVideo.VideoId, nonSeasonSequence.SequenceId);

            var videos = videoRepository.GetVideosNotInSeriesSequence(nonSeasonSequence, nonSeasonSequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_WithSeasonSequence_ShouldIncludeVideosInNonSeasonSequences() {
            var nonSeasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, false, 1);
            var nonSeasonSequence = CreateAndRetrieveVideoSequence(nonSeasonDto);
            repository.AddVideoToSequence(testVideo.VideoId, nonSeasonSequence.SequenceId);
            
            var seasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, 1);
            var seasonSequence = CreateAndRetrieveVideoSequence(seasonDto);

            var videos = videoRepository.GetVideosNotInSeriesSequence(seasonSequence, seasonSequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { testVideo }, videos);
        }

        [TestMethod]
        public void GetVideosNotInSeriesSequence_WithSeasonSequence_ShouldntIncludeVideosInSeasonSequences() {
            var seasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, 1);
            var seasonSequence = CreateAndRetrieveVideoSequence(seasonDto);
            repository.AddVideoToSequence(testVideo.VideoId, seasonSequence.SequenceId);

            var otherSeasonDto = new CreateSeriesSequenceDto(testSeries.SeriesId, "title", "desc", -1, true, 2);
            var otherSeasonSequence = CreateAndRetrieveVideoSequence(otherSeasonDto);

            var videos = videoRepository.GetVideosNotInSeriesSequence(otherSeasonSequence, otherSeasonSequence.SeriesId, "", GetFirstVideoPage()).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void UpsertSeriesSequences_ShouldInsertNewItems() {
            var sequences = new List<ExportedSeriesSequenceSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new SeriesSequence(-1, "t" + i, "", -1, false, false, 0, testSeries.SeriesId, UniqueIdUtil.GenerateUniqueId());
                sequences.Add(new ExportedSeriesSequenceSimpleDto(s, null, testSeries.UniqueId));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertSeriesSequences(sequences, ids);

            var retSequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var s in retSequences) {
                expectedIds[s.UniqueId] = s.SequenceId;
            }

            var expectedSequences = sequences.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSequences, retSequences);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertSeriesSequences_ShouldUpdateExistingItems() {
            var sequences = new List<ExportedSeriesSequenceSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new SeriesSequence(-1, "t" + i, "", -1, false, false, 0, testSeries.SeriesId, UniqueIdUtil.GenerateUniqueId());
                sequences.Add(new ExportedSeriesSequenceSimpleDto(s, null, testSeries.UniqueId));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertSeriesSequences(sequences, ids);
            sequences[0].Details.Title = "new 0";
            sequences[2].Details.Title = "new 2";
            repository.UpsertSeriesSequences(sequences, ids);

            var retSequences = repository.GetVideoSequencesInSeries(testSeries.SeriesId, GetFirstPage()).Result.Results;

            var expectedSequences = sequences.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSequences, retSequences);
        }
    }
}

using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories;
using Ingvilt.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Dto.Sequences;
using Ingvilt.Models.DataAccess.Sorting;
using IntegrationTesting.Util;
using System;
using Ingvilt.Util;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class PlaylistRepositoryTests : BaseTest {
        private PlaylistRepository repository = new PlaylistRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();

        private Library testLibrary = null;
        private Library otherLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public PlaylistRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("other"));
        }

        private PlaylistDto CreateAndRetrievePlaylist(CreateVideoSequenceDto dto) {
            var id = repository.CreateVideoSequence(dto);
            return repository.GetPlaylist(id);
        }

        private PlaylistDto CreateAndRetrievePlaylist() {
            return CreateAndRetrievePlaylist(CreateDto());
        }

        protected CreateVideoSequenceDto CreateDto(long libraryId, long coverFileId = DatabaseConstants.DEFAULT_ID) {
            return new CreatePlaylistDto(libraryId, "title", "desc", coverFileId);
        }

        protected CreateVideoSequenceDto CreateDto() {
            return CreateDto(testLibrary.LibraryId);
        }

        protected CreateVideoSequenceDto CreateDtoWithCover(long coverFileId) {
            return CreateDto(testLibrary.LibraryId, coverFileId);
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new SequenceSortCriteria(true));
        }

        [TestMethod]
        public void TestCreateVideoSequence_WithNullLogo() {
            var sequenceToCreate = CreateDto();
            var sequenceId = repository.CreateVideoSequence(sequenceToCreate);

            var sequenceRetrieved = repository.GetPlaylist(sequenceId);

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

            var sequenceRetrieved = repository.GetPlaylist(sequenceId);

            Assert.AreEqual(sequenceId, sequenceRetrieved.SequenceId);
            Assert.AreEqual(sequenceToCreate.Title, sequenceRetrieved.Title);
            Assert.AreEqual(sequenceToCreate.Description, sequenceRetrieved.Description);
            Assert.AreEqual(sequenceToCreate.CoverFile, sequenceRetrieved.CoverFile);
        }

        [TestMethod]
        public void TestUpdateVideoSequence() {
            var sequenceToCreate = CreateDto();
            var sequenceId = repository.CreateVideoSequence(sequenceToCreate);

            var sequenceRetrieved = repository.GetPlaylist(sequenceId);
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            sequenceRetrieved.Description += "1";
            sequenceRetrieved.Title += "2";
            sequenceRetrieved.CoverFile = fileId;
            repository.UpdateVideoSequence(sequenceRetrieved);

            var updatedPlaylistRetrieved = repository.GetPlaylist(sequenceId);

            Assert.AreEqual(sequenceRetrieved.SequenceId, updatedPlaylistRetrieved.SequenceId);
            Assert.AreEqual(sequenceRetrieved.Title, updatedPlaylistRetrieved.Title);
            Assert.AreEqual(sequenceRetrieved.Description, updatedPlaylistRetrieved.Description);
            Assert.AreEqual(sequenceRetrieved.CoverFile, updatedPlaylistRetrieved.CoverFile);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { updatedPlaylistRetrieved }, repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results);
        }

        [TestMethod]
        public void TestUpdateVideoSequence_ShouldNotUpdateDifferentPlaylist() {
            var sequenceDto = CreateDto();

            var sequenceToUpdateId = repository.CreateVideoSequence(sequenceDto);
            sequenceDto.UniqueId = UniqueIdUtil.GenerateUniqueId();
            var sequenceNotUpdatedId = repository.CreateVideoSequence(sequenceDto);

            var sequenceToUpdate = repository.GetPlaylist(sequenceToUpdateId);
            sequenceToUpdate.Description += "1";
            repository.UpdateVideoSequence(sequenceToUpdate);

            var sequenceToNotUpdate = repository.GetPlaylist(sequenceNotUpdatedId);

            Assert.AreNotEqual(sequenceToUpdate.Description, sequenceToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetPlaylistsWithNoneCreated() {
            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            Assert.AreEqual(0, sequences.Count);
        }

        [TestMethod]
        public void TestGetPlaylistsWithOneCreated() {
            var sequence = CreateAndRetrievePlaylist();
            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            var expectedPlaylists = new List<PlaylistDto>();
            expectedPlaylists.Add(sequence);

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetPlaylists_WithNoLibrary() {
            var sequence = CreateAndRetrievePlaylist(new CreatePlaylistDto(-1, "test", "", -1));
            var sequences = repository.GetPlaylists(GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { sequence }, sequences);
        }

        [TestMethod]
        public void TestGetPlaylistsWithMultipleCreated() {
            var expectedPlaylists = new List<PlaylistDto>();
            for (int i = 0; i < 5; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
            }

            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetPlaylists_ShouldntReturnDeletedPlaylists() {
            var expectedPlaylists = new List<PlaylistDto>();
            for (int i = 0; i < 5; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
            }
            var deletedPlaylist = CreateAndRetrievePlaylist();
            repository.DeleteVideoSequence(deletedPlaylist.SequenceId);
            deletedPlaylist.Deleted = true;

            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void ShouldNotDeleteVideoSequence_WhenDifferentLibraryDeleted() {
            var sequenceRetrieved = CreateAndRetrievePlaylist(CreateDto(otherLibrary.LibraryId));

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), repository.GetDeletedPlaylistsInLibrary(otherLibrary.LibraryId, GetFirstPage(), "").Result.Results);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { sequenceRetrieved }, repository.GetPlaylistsInLibrary(otherLibrary.LibraryId, GetFirstPage(), "").Result.Results);
        }

        [TestMethod]
        public void ShouldUndeletePlaylist_WhenLibraryRestored() {
            var sequenceRetrieved = CreateAndRetrievePlaylist();

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);

            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), repository.GetDeletedPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { sequenceRetrieved }, repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeletePlaylist_WhenDifferentLibraryRestored() {
            var sequenceRetrieved = CreateAndRetrievePlaylist(CreateDto(otherLibrary.LibraryId));

            repository.DeleteVideoSequence(sequenceRetrieved.SequenceId);
            sequenceRetrieved.Deleted = true;
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);

            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { sequenceRetrieved }, repository.GetDeletedPlaylistsInLibrary(otherLibrary.LibraryId, GetFirstPage(), "").Result.Results);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), repository.GetPlaylistsInLibrary(otherLibrary.LibraryId, GetFirstPage(), "").Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestorePlaylistDeletedNormally_WhenLibraryRestored() {
            var sequenceRetrieved = CreateAndRetrievePlaylist();

            repository.DeleteVideoSequence(sequenceRetrieved.SequenceId);
            sequenceRetrieved.Deleted = true;
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);

            CollectionAssert.AreEquivalent(new List<PlaylistDto>() { sequenceRetrieved }, repository.GetDeletedPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results);
        }

        [TestMethod]
        public void GetPlaylistsInLibrary_ShouldntReturnDeletedPlaylists() {
            var expectedPlaylists = new List<PlaylistDto>();
            for (int i = 0; i < 3; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
            }
            var deletedPlaylist = CreateAndRetrievePlaylist();
            repository.DeleteVideoSequence(deletedPlaylist.SequenceId);

            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetPlaylistsInLibrary_ShouldntReturnPlaylistsInOtherLibrary() {
            CreateAndRetrievePlaylist(CreateDto(otherLibrary.LibraryId));

            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), sequences);
        }

        [TestMethod]
        public void GetDeletedPlaylistsInLibrary_ShouldOnlyReturnDeletedPlaylists() {
            var expectedPlaylists = new List<PlaylistDto>();
            for (int i = 0; i < 3; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                repository.DeleteVideoSequence(sequence.SequenceId);
                expectedPlaylists.Add(sequence);
                sequence.Deleted = true;
            }
            CreateAndRetrievePlaylist();

            var sequences = repository.GetDeletedPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetDeletedPlaylistsInLibrary_ShouldntReturnPlaylistsInOtherLibrary() {
            var sequence = CreateAndRetrievePlaylist(CreateDto(otherLibrary.LibraryId));
            repository.GetPlaylist(sequence.SequenceId);
            repository.DeleteVideoSequence(sequence.SequenceId);

            var sequences = repository.GetDeletedPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), sequences);
        }

        [TestMethod]
        public void RestoreDeleteVideoSequence_ShouldUndeletePlaylist() {
            var expectedPlaylists = new List<PlaylistDto>();
            for (int i = 0; i < 2; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
            }
            var deletedPlaylist = CreateAndRetrievePlaylist();
            expectedPlaylists.Add(deletedPlaylist);
            repository.DeleteVideoSequence(deletedPlaylist.SequenceId);
            repository.RestoreDeletedVideoSequence(deletedPlaylist.SequenceId);

            var sequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetAllPlaylistsContainingVideo_WithPlaylistsContainingVideo() {
            var expectedPlaylists = new List<PlaylistDto>();
            var videoRepository = new VideoRepository();
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);

            for (int i = 0; i < 2; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
                repository.AddVideoToSequence(videoId, sequence.SequenceId);
            }

            var sequences = repository.GetAllPlaylistsContainingVideo(videoId);
            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetAllPlaylistsContainingVideo_WithPlaylistNotContainingVideo() {
            var videoRepository = new VideoRepository();
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);

            for (int i = 0; i < 1; ++i) {
                var sequence = CreateAndRetrievePlaylist();
            }

            var sequences = repository.GetAllPlaylistsContainingVideo(videoId);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), sequences);
        }

        [TestMethod]
        public void GetAllPlaylistsContainingVideo_WithDeletedPlaylistNotContainingVideo() {
            var expectedPlaylists = new List<PlaylistDto>();
            var videoRepository = new VideoRepository();
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);

            for (int i = 0; i < 1; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                expectedPlaylists.Add(sequence);
                repository.AddVideoToSequence(videoId, sequence.SequenceId);
            }
            var deletedSequence = CreateAndRetrievePlaylist();
            repository.AddVideoToSequence(videoId, deletedSequence.SequenceId);
            repository.DeleteVideoSequence(deletedSequence.SequenceId);

            var sequences = repository.GetAllPlaylistsContainingVideo(videoId);
            CollectionAssert.AreEquivalent(expectedPlaylists, sequences);
        }

        [TestMethod]
        public void GetAllPlaylistsContainingVideo_WithPlaylistContainingDifferentVideo() {
            var videoRepo = new VideoRepository();
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepo.CreateVideo(videoDto);

            var otherVideoId = videoRepo.CreateVideo(videoDto);

            for (int i = 0; i < 1; ++i) {
                var sequence = CreateAndRetrievePlaylist();
                repository.AddVideoToSequence(otherVideoId, sequence.SequenceId);
            }

            var sequences = repository.GetAllPlaylistsContainingVideo(videoId);
            CollectionAssert.AreEquivalent(new List<PlaylistDto>(), sequences);
        }

        [TestMethod]
        public void UpsertPlaylists_ShouldInsertNewItems() {
            var sequences = new List<ExportedPlaylistSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new PlaylistDto(-1, "p" + i, "", -1, false, testLibrary.LibraryId, DateTime.Now, UniqueIdUtil.GenerateUniqueId());
                sequences.Add(new ExportedPlaylistSimpleDto(s, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertPlaylists(sequences, ids);

            var retSequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var s in retSequences) {
                expectedIds[s.UniqueId] = s.SequenceId;
            }

            var expectedSequences = sequences.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSequences, retSequences);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertPlaylists_ShouldUpdateExistingItems() {
            var sequences = new List<ExportedPlaylistSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var s = new PlaylistDto(-1, "p" + i, "", -1, false, testLibrary.LibraryId, DateTime.Now, UniqueIdUtil.GenerateUniqueId());
                sequences.Add(new ExportedPlaylistSimpleDto(s, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertPlaylists(sequences, ids);
            sequences[0].Details.Title = "new 0";
            sequences[0].Details.Title = "new 2";
            repository.UpsertPlaylists(sequences, ids);

            var retSequences = repository.GetPlaylistsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            var expectedSequences = sequences.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedSequences, retSequences);
        }

        [TestMethod]
        public void GetPercentageOfVideosWatchedInSequence_WithNoVideosInAnySequence() {
            var sequence1 = CreateAndRetrievePlaylist();
            CreateAndRetrievePlaylist();

            var percentWatched = repository.GetPercentageOfVideosWatchedInSequence(sequence1.SequenceId);
            Assert.AreEqual(0, percentWatched);
        }

        [TestMethod]
        public void GetPercentageOfVideosWatchedInSequence_WithNoVideosInSpecifiedSequence() {
            var sequence1 = CreateAndRetrievePlaylist();
            var sequence2 = CreateAndRetrievePlaylist();

            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoDto.TimesWatched = 1;
            var videoId = new VideoRepository().CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence2.SequenceId);

            var percentWatched = repository.GetPercentageOfVideosWatchedInSequence(sequence1.SequenceId);
            Assert.AreEqual(0, percentWatched);
        }

        [TestMethod]
        public void GetPercentageOfVideosWatchedInSequence_WithVideosInSpecifiedSequence() {
            var videoRepository = new VideoRepository();
            var sequence1 = CreateAndRetrievePlaylist();
            var sequence2 = CreateAndRetrievePlaylist();

            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoDto.TimesWatched = 1;
            var videoId = videoRepository.CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence1.SequenceId);

            videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoId = videoRepository.CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence1.SequenceId);

            videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoDto.TimesWatched = 1;
            videoId = videoRepository.CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence2.SequenceId);

            var percentWatched = repository.GetPercentageOfVideosWatchedInSequence(sequence1.SequenceId);
            Assert.AreEqual(50, percentWatched);
        }

        [TestMethod]
        public void GetPercentageOfVideosWatchedInSequence_WithAllVideosWatchedInSpecifiedSequence() {
            var sequence1 = CreateAndRetrievePlaylist();
            CreateAndRetrievePlaylist();

            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoDto.TimesWatched = 1;
            var videoId = new VideoRepository().CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence1.SequenceId);

            videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            videoDto.TimesWatched = 5;
            videoId = new VideoRepository().CreateVideo(videoDto);
            repository.AddVideoToSequence(videoId, sequence1.SequenceId);

            var percentWatched = repository.GetPercentageOfVideosWatchedInSequence(sequence1.SequenceId);
            Assert.AreEqual(100, percentWatched);
        }
    }
}

using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Tags;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Models.DataAccess.Search.Media;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTestingRedo.Util;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class MediaFileRepositoryTests : BaseTest {
        private MediaFileRepository repository = new MediaFileRepository();
        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = new LibraryRepository().CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        private List<MediaFile> GetSavedMediaFiles(Pagination pagination = null) {
            if (pagination == null) {
                pagination = Pagination.FirstPageWithDefaultCount(new MediaFileDateSortCriteria(true));
            }

            return repository.GetMediaFiles(pagination).Result.Results;
        }

        public MediaFileRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private void TestCreateMediaFile(bool isImage) {
            var type = isImage ? MediaFileType.IMAGE_TYPE : MediaFileType.VIDEO_TYPE;
            var fileToCreate = new CreateMediaFileDto("C:/test/test.jpg", type, "Test");
            var fileId = repository.CreateMediaFile(fileToCreate);

            var fileRetrieved = repository.GetMediaFile(fileId);

            Assert.IsTrue(fileRetrieved.MediaId > -1);
            Assert.AreEqual(fileToCreate.SourceURL, fileRetrieved.SourceURL);
            Assert.AreEqual(fileToCreate.FileType, fileRetrieved.FileType);
            Assert.AreEqual(fileToCreate.Name, fileRetrieved.Name);
            Assert.IsNotNull(fileRetrieved.UniqueId);

            var expectedFiles = new List<MediaFile>();
            expectedFiles.Add(fileRetrieved);

            var pagination = Pagination.FirstPageWithDefaultCount(new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        private Pagination GetFirstPage() {
            return new Pagination(new MediaFileDateSortCriteria(false), 10);
        }

        [TestMethod]
        public void TestCreateMediaFileImage() {
            TestCreateMediaFile(true);
        }

        [TestMethod]
        public void TestCreateMediaFileNonImage() {
            TestCreateMediaFile(false);
        }

        [TestMethod]
        public void TestCreateMultipleFilesWithDifferentUrls() {
            var fileToCreate1 = new CreateMediaFileDto("C:/test/test.jpg", MediaFileType.IMAGE_TYPE, "");
            var fileToCreate2 = new CreateMediaFileDto("C:/test/test2.mp4", MediaFileType.VIDEO_TYPE, "");

            var file1Id = repository.CreateMediaFile(fileToCreate1);
            var file2Id = repository.CreateMediaFile(fileToCreate2);

            Assert.AreNotEqual(file1Id, file2Id);
        }

        [TestMethod]
        public void TestDeleteMediaFile() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            repository.DeleteMediaFile(fileId);

            var pagination = Pagination.FirstPageWithDefaultCount(new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(new List<MediaFile>(), GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void TestDeleteMediaFileShouldDeleteOnlySpecifiedFile() {
            var deletedFileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var expectedRemainingFiles = new List<MediaFile>();
            for (int i = 0; i < 10; ++i) {
                var fileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
                var createdFile = repository.GetMediaFile(fileId);
                expectedRemainingFiles.Add(createdFile);
            }

            repository.DeleteMediaFile(deletedFileId);

            var pagination = Pagination.FirstPageWithDefaultCount(new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedRemainingFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void GetFiles_ShouldntReturnDeletedFiles() {
            var expectedFiles = new List<MediaFile>();
            for (int i = 0; i < 3; ++i) {
                var fileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
                var createdFile = repository.GetMediaFile(fileId);
                expectedFiles.Add(createdFile);
            }
            var deletedFileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/testDeleted.jpg", MediaFileType.IMAGE_TYPE, ""));
            repository.DeleteMediaFile(deletedFileId);

            var pagination = Pagination.FirstPage(10, new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void TestGetFilesPage1() {
            var expectedFiles = new List<MediaFile>();
            for (int i = 0; i < 10; ++i) {
                var fileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
                var createdFile = repository.GetMediaFile(fileId);
                expectedFiles.Add(createdFile);
            }
            for (int i = 10; i < 30; ++i) {
                repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
            }

            var pagination = Pagination.FirstPage(10, new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void TestGetFilesPage2() {
            var expectedFiles = new List<MediaFile>();
            for (int i = 0; i < 10; ++i) {
                repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
            }

            for (int i = 10; i < 20; ++i) {
                var fileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
                var createdFile = repository.GetMediaFile(fileId);
                expectedFiles.Add(createdFile);
            }

            for (int i = 20; i < 30; ++i) {
                repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
            }

            var pagination = Pagination.GetForPage(2, 10, new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void TestGetFilesPage3() {
            var expectedFiles = new List<MediaFile>();
            for (int i = 0; i < 20; ++i) {
                repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
            }

            for (int i = 20; i < 30; ++i) {
                var fileId = repository.CreateMediaFile(new CreateMediaFileDto($"C:/test/test{i}.jpg", MediaFileType.IMAGE_TYPE, ""));
                var createdFile = repository.GetMediaFile(fileId);
                expectedFiles.Add(createdFile);
            }

            var pagination = Pagination.GetForPage(3, 10, new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        public void TestGetFilesWithNoResults() {
            var expectedFiles = new List<MediaFile>();
            var pagination = Pagination.GetForPage(1, 10, new MediaFileDateSortCriteria(true));
            CollectionAssert.AreEquivalent(expectedFiles, GetSavedMediaFiles(pagination));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An invalid media file was returned")]
        public void TestGetFileWithNoMatch() {
            repository.CreateMediaFile(new CreateMediaFileDto("C:/test/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            repository.GetMediaFile(10);
        }

        [TestMethod]
        public void TestDoesFileExistWithUrlWithoutExistingFile() {
            repository.CreateMediaFile(new CreateMediaFileDto("C:/test/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            Assert.IsNull(repository.GetFileWithUrl("C:/test"));
        }

        [TestMethod]
        public void TestDoesFileExistWithUrlWithExistingFile() {
            var url = "C:/test/test.jpg";
            repository.CreateMediaFile(new CreateMediaFileDto(url, MediaFileType.IMAGE_TYPE, ""));
            Assert.IsNotNull(repository.GetFileWithUrl(url));
        }

        [TestMethod]
        public void GetMediaFileTags_WithNoTags() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var tags = repository.GetMediaFileTags(file);

            CollectionAssert.AreEquivalent(new List<VideoTag>(), tags);
        }

        [TestMethod]
        public void GetMediaFileTags_WithTags() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var tagRepository = new TagRepository();
            var expectedTags = new List<VideoTag>();

            for (int i = 0; i < 5; ++i) {
                var tagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag " + i, "test"));
                var tag = tagRepository.GetVideoTag(tagId);
                expectedTags.Add(tag);

                repository.AddTagToMediaFile(file, tag);
            }

            var tags = repository.GetMediaFileTags(file);
            CollectionAssert.AreEquivalent(expectedTags, tags);
        }

        [TestMethod]
        public void RemoveTag_ShouldRemoveCorrectTag() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var tagRepository = new TagRepository();
            var expectedTags = new List<VideoTag>();

            for (int i = 0; i < 3; ++i) {
                var tagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag " + i, "test"));
                var tag = tagRepository.GetVideoTag(tagId);
                expectedTags.Add(tag);

                repository.AddTagToMediaFile(file, tag);
            }

            var deletedTagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag deleted", "test"));
            var deletedTag = tagRepository.GetVideoTag(deletedTagId);
            repository.AddTagToMediaFile(file, deletedTag);
            repository.RemoveTagFromMediaFile(file, deletedTag);

            var tags = repository.GetMediaFileTags(file);
            CollectionAssert.AreEquivalent(expectedTags, tags);
        }

        [TestMethod]
        public void RemoveTag_ShouldRemoveTagFromCorrectFile() {
            var tagRepository = new TagRepository();

            var fileToRemoveTagId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileToRemoveTag = repository.GetMediaFile(fileToRemoveTagId);

            var fileToKeepTagId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileToKeepTag = repository.GetMediaFile(fileToKeepTagId);

            var deletedTagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag deleted", "test"));
            var deletedTag = tagRepository.GetVideoTag(deletedTagId);
            repository.AddTagToMediaFile(fileToRemoveTag, deletedTag);
            repository.AddTagToMediaFile(fileToKeepTag, deletedTag);
            repository.RemoveTagFromMediaFile(fileToRemoveTag, deletedTag);

            var tagsOnFileToKeep = repository.GetMediaFileTags(fileToKeepTag);
            var tagsOnFileToRemove = repository.GetMediaFileTags(fileToRemoveTag);

            CollectionAssert.AreEquivalent(new List<VideoTag>() {deletedTag}, tagsOnFileToKeep);
            CollectionAssert.AreEquivalent(new List<VideoTag>(), tagsOnFileToRemove);
        }

        [TestMethod]
        public void GetMediaFileTags_WithNoTags_ForFile() {
            var fileWithoutTagsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithoutTags = repository.GetMediaFile(fileWithoutTagsId);
            var tagsOnFileWithoutTags = repository.GetMediaFileTags(fileWithoutTags);

            var fileWithTagsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithTags = repository.GetMediaFile(fileWithTagsId);

            var expectedTags = new List<VideoTag>();
            var tagRepository = new TagRepository();
            var tagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", "test"));
            var tag = tagRepository.GetVideoTag(tagId);
            expectedTags.Add(tag);
            repository.AddTagToMediaFile(fileWithTags, tag);

            var tagsOnFileWithTags = repository.GetMediaFileTags(fileWithTags);

            CollectionAssert.AreEquivalent(new List<VideoTag>(), tagsOnFileWithoutTags);
            CollectionAssert.AreEquivalent(expectedTags, tagsOnFileWithTags);
        }

        [TestMethod]
        public void GetMediaFileTags_ShouldNotReturnDeletedTags() {
            var tagRepository = new TagRepository();
            var expectedTags = new List<VideoTag>();

            var fileWithTagsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithTags = repository.GetMediaFile(fileWithTagsId);

            var deletedTagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", "test"));
            var deletedTag = tagRepository.GetVideoTag(deletedTagId);

            var notDeletedTagId = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", "test"));
            var notDeletedTag = tagRepository.GetVideoTag(notDeletedTagId);

            repository.AddTagToMediaFile(fileWithTags, notDeletedTag);
            repository.AddTagToMediaFile(fileWithTags, deletedTag);
            tagRepository.DeleteVideoTag(deletedTag);

            expectedTags.Add(notDeletedTag);
            var tagsOnFileWithTags = repository.GetMediaFileTags(fileWithTags);

            CollectionAssert.AreEquivalent(expectedTags, tagsOnFileWithTags);
        }

        [TestMethod]
        public void GetMediaFileLocations_WithNoLocations() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var Locations = repository.GetMediaFileLocations(file);

            CollectionAssert.AreEquivalent(new List<Location>(), Locations);
        }

        [TestMethod]
        public void GetMediaFileLocations_WithLocations() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var LocationRepository = new LocationRepository();
            var expectedLocations = new List<Location>();

            for (int i = 0; i < 5; ++i) {
                var LocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location " + i, "test", testLibrary.LibraryId, -1, -1));
                var Location = LocationRepository.GetLocation(LocationId);
                expectedLocations.Add(Location);

                repository.AddLocationToMediaFile(file, Location);
            }

            var Locations = repository.GetMediaFileLocations(file);
            CollectionAssert.AreEquivalent(expectedLocations, Locations);
        }

        [TestMethod]
        public void RemoveLocation_ShouldRemoveCorrectLocation() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var file = repository.GetMediaFile(fileId);
            var LocationRepository = new LocationRepository();
            var expectedLocations = new List<Location>();

            for (int i = 0; i < 3; ++i) {
                var LocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location " + i, "test", testLibrary.LibraryId, -1, -1));
                var Location = LocationRepository.GetLocation(LocationId);
                expectedLocations.Add(Location);

                repository.AddLocationToMediaFile(file, Location);
            }

            var deletedLocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location deleted", "test", testLibrary.LibraryId, -1, -1));
            var deletedLocation = LocationRepository.GetLocation(deletedLocationId);
            repository.AddLocationToMediaFile(file, deletedLocation);
            repository.RemoveLocationFromMediaFile(file, deletedLocation);

            var Locations = repository.GetMediaFileLocations(file);
            CollectionAssert.AreEquivalent(expectedLocations, Locations);
        }

        [TestMethod]
        public void RemoveLocation_ShouldRemoveLocationFromCorrectFile() {
            var LocationRepository = new LocationRepository();

            var fileToRemoveLocationId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileToRemoveLocation = repository.GetMediaFile(fileToRemoveLocationId);

            var fileToKeepLocationId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileToKeepLocation = repository.GetMediaFile(fileToKeepLocationId);

            var deletedLocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location deleted", "test", testLibrary.LibraryId, -1, -1));
            var deletedLocation = LocationRepository.GetLocation(deletedLocationId);
            repository.AddLocationToMediaFile(fileToRemoveLocation, deletedLocation);
            repository.AddLocationToMediaFile(fileToKeepLocation, deletedLocation);
            repository.RemoveLocationFromMediaFile(fileToRemoveLocation, deletedLocation);

            var LocationsOnFileToKeep = repository.GetMediaFileLocations(fileToKeepLocation);
            var LocationsOnFileToRemove = repository.GetMediaFileLocations(fileToRemoveLocation);

            CollectionAssert.AreEquivalent(new List<Location>() { deletedLocation }, LocationsOnFileToKeep);
            CollectionAssert.AreEquivalent(new List<Location>(), LocationsOnFileToRemove);
        }

        [TestMethod]
        public void GetMediaFileLocations_WithNoLocations_ForFile() {
            var fileWithoutLocationsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithoutLocations = repository.GetMediaFile(fileWithoutLocationsId);
            var LocationsOnFileWithoutLocations = repository.GetMediaFileLocations(fileWithoutLocations);

            var fileWithLocationsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithLocations = repository.GetMediaFile(fileWithLocationsId);

            var expectedLocations = new List<Location>();
            var LocationRepository = new LocationRepository();
            var LocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location 1", "test", testLibrary.LibraryId, -1, -1));
            var Location = LocationRepository.GetLocation(LocationId);
            expectedLocations.Add(Location);
            repository.AddLocationToMediaFile(fileWithLocations, Location);

            var LocationsOnFileWithLocations = repository.GetMediaFileLocations(fileWithLocations);

            CollectionAssert.AreEquivalent(new List<Location>(), LocationsOnFileWithoutLocations);
            CollectionAssert.AreEquivalent(expectedLocations, LocationsOnFileWithLocations);
        }

        [TestMethod]
        public void GetMediaFileLocations_ShouldNotReturnDeletedLocations() {
            var LocationRepository = new LocationRepository();
            var expectedLocations = new List<Location>();

            var fileWithLocationsId = repository.CreateMediaFile(new CreateMediaFileDto("C:/test2.jpg", MediaFileType.IMAGE_TYPE, ""));
            var fileWithLocations = repository.GetMediaFile(fileWithLocationsId);

            var deletedLocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location 1", "test", testLibrary.LibraryId, -1, -1));
            var deletedLocation = LocationRepository.GetLocation(deletedLocationId);

            var notDeletedLocationId = LocationRepository.CreateLocation(new CreateLocationDto("Location 1", "test", testLibrary.LibraryId, -1, -1));
            var notDeletedLocation = LocationRepository.GetLocation(notDeletedLocationId);

            repository.AddLocationToMediaFile(fileWithLocations, notDeletedLocation).ConfigureAwait(false);
            repository.AddLocationToMediaFile(fileWithLocations, deletedLocation).ConfigureAwait(false);
            LocationRepository.DeleteLocation(deletedLocation).ConfigureAwait(false);

            expectedLocations.Add(notDeletedLocation);
            var LocationsOnFileWithLocations = repository.GetMediaFileLocations(fileWithLocations);

            CollectionAssert.AreEquivalent(expectedLocations, LocationsOnFileWithLocations);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_PartialMatch() {
            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);
            characterDto.Name = "test 2";
            var character2 = characterRepository.CreateCharacter(characterDto);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddFileToCharacter(character1, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"1\", \"2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile>();
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_AllMatch() {
            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);
            characterDto.Name = "test 2";
            var character2 = characterRepository.CreateCharacter(characterDto);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddFileToCharacter(character1, file).ConfigureAwait(false);
            repository.AddFileToCharacter(character2, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"1\", \"2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_MatchWithCoverFile() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);
            characterDto.Name = "test 2";
            characterDto.CoverMediaId = fileId;
            var character2 = characterRepository.CreateCharacter(characterDto);

            repository.AddFileToCharacter(character1, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"1\", \"2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_CharacterWithBothCoverFileAndAttached() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);
            characterDto.Name = "test 2";
            characterDto.CoverMediaId = fileId;
            var character2 = characterRepository.CreateCharacter(characterDto);

            repository.AddFileToCharacter(character1, file).ConfigureAwait(false);
            repository.AddFileToCharacter(character2, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"1\", \"2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_CharacterWithBothCoverFileAndAttached_OtherCharacterNotFound() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);
            characterDto.Name = "test 2";
            characterDto.CoverMediaId = fileId;
            var character2 = characterRepository.CreateCharacter(characterDto);

            repository.AddFileToCharacter(character2, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"1\", \"2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile>();
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithCharacters_MatchUppercase() {
            var characterRepository = new CharacterRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "test 1";
            var character1 = characterRepository.CreateCharacter(characterDto);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddFileToCharacter(character1, file).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithCharacterNameGenerator("[\"TEST 1\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithTags_PartialMatch() {
            var tagRepository = new TagRepository();
            var tagDto = new CreateVideoTagDto("test 1", "");
            var tag1Id = tagRepository.CreateVideoTag(tagDto);
            var tag1 = tagRepository.GetVideoTag(tag1Id);
            tagDto = new CreateVideoTagDto("test 2", "");
            var tag2Id = tagRepository.CreateVideoTag(tagDto);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddTagToMediaFile(file, tag1);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithTagsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithTags_AllMatch() {
            var tagRepository = new TagRepository();
            var tagDto = new CreateVideoTagDto("test 1", "");
            var tag1Id = tagRepository.CreateVideoTag(tagDto);
            var tag1 = tagRepository.GetVideoTag(tag1Id);
            tagDto = new CreateVideoTagDto("test 2", "");
            var tag2Id = tagRepository.CreateVideoTag(tagDto);
            var tag2 = tagRepository.GetVideoTag(tag2Id);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddTagToMediaFile(file, tag1);
            repository.AddTagToMediaFile(file, tag2);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithTagsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithLocations_PartialMatch() {
            var locationRepository = new LocationRepository();
            var locationDto = new CreateLocationDto("test 1", "", testLibrary.LibraryId, -1, -1);
            var location1Id = locationRepository.CreateLocation(locationDto);
            var location1 = locationRepository.GetLocation(location1Id);
            locationDto = new CreateLocationDto("test 2", "", testLibrary.LibraryId, -1, -1);
            var location2Id = locationRepository.CreateLocation(locationDto);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddLocationToMediaFile(file, location1).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithLocationsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithLocations_MatchWithCoverFile() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var locationRepository = new LocationRepository();
            var locationDto = new CreateLocationDto("test 1", "", testLibrary.LibraryId, -1, -1);
            var location1Id = locationRepository.CreateLocation(locationDto);
            var location1 = locationRepository.GetLocation(location1Id);
            locationDto = new CreateLocationDto("test 2", "", testLibrary.LibraryId, -1, -1);
            locationDto.CoverFileId = fileId;
            var location2Id = locationRepository.CreateLocation(locationDto);
            var location2 = locationRepository.GetLocation(location2Id);

            repository.AddLocationToMediaFile(file, location1).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithLocationsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithLocations_MatchWithCoverFileAndAttached() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var locationRepository = new LocationRepository();
            var locationDto = new CreateLocationDto("test 1", "", testLibrary.LibraryId, -1, -1);
            var location1Id = locationRepository.CreateLocation(locationDto);
            var location1 = locationRepository.GetLocation(location1Id);
            locationDto = new CreateLocationDto("test 2", "", testLibrary.LibraryId, -1, -1);
            locationDto.CoverFileId = fileId;
            var location2Id = locationRepository.CreateLocation(locationDto);
            var location2 = locationRepository.GetLocation(location2Id);

            repository.AddLocationToMediaFile(file, location1).ConfigureAwait(false);
            repository.AddLocationToMediaFile(file, location2).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithLocationsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithLocations_MatchWithCoverFileAndAttached_OtherNoMatch() {
            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            var locationRepository = new LocationRepository();
            var locationDto = new CreateLocationDto("test 1", "", testLibrary.LibraryId, -1, -1);
            var location1Id = locationRepository.CreateLocation(locationDto);
            var location1 = locationRepository.GetLocation(location1Id);
            locationDto = new CreateLocationDto("test 2", "", testLibrary.LibraryId, -1, -1);
            locationDto.CoverFileId = fileId;
            var location2Id = locationRepository.CreateLocation(locationDto);
            var location2 = locationRepository.GetLocation(location2Id);

            repository.AddLocationToMediaFile(file, location2).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithLocationsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile>();
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void SearchForFiles_WithLocations_AllMatch() {
            var locationRepository = new LocationRepository();
            var locationDto = new CreateLocationDto("test 1", "", testLibrary.LibraryId, -1, -1);
            var location1Id = locationRepository.CreateLocation(locationDto);
            var location1 = locationRepository.GetLocation(location1Id);
            locationDto = new CreateLocationDto("test 2", "", testLibrary.LibraryId, -1, -1);
            var location2Id = locationRepository.CreateLocation(locationDto);
            var location2 = locationRepository.GetLocation(location2Id);

            var fileId = repository.CreateMediaFile(new CreateMediaFileDto("", MediaFileType.VIDEO_TYPE, ""));
            var file = repository.GetMediaFile(fileId);

            repository.AddLocationToMediaFile(file, location1).ConfigureAwait(false);
            repository.AddLocationToMediaFile(file, location2).ConfigureAwait(false);

            var queries = new List<IMediaFileSearchQueryGenerator>();
            queries.Add(new MediaFileWithLocationsGenerator("[\"test 1\", \"test 2\"]"));

            var actualFiles = repository.SearchForFiles(GetFirstPage(), queries).Result.Results;
            var expectedFiles = new List<MediaFile> { file };
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public void UpsertMediaFilesShouldInsertNewFiles() {
            var files = new List<MediaFile>();

            for (int i = 0; i < 3; ++i) {
                var newFile = new MediaFile(-1, "https://google.ca", MediaFileType.VIDEO_TYPE, $"file {i}", DateTime.Now, UniqueIdUtil.GenerateUniqueId());
                files.Add(newFile);
            }

            var fileIds = new Dictionary<string, long>();
            repository.UpsertMediaFiles(files, fileIds).ConfigureAwait(false);

            var retrievedFiles = repository.GetMediaFiles(GetFirstPage()).Result.Results;
            var expectedFileIds = new Dictionary<string, long>();

            foreach (var file in files) {
                foreach (var retrieved in retrievedFiles) {
                    if (file.UniqueId == retrieved.UniqueId) {
                        file.MediaId = retrieved.MediaId;
                        break;
                    }
                }
            }

            foreach (var file in retrievedFiles) {
                expectedFileIds[file.UniqueId] = file.MediaId;
            }

            CollectionAssert.AreEquivalent(files, retrievedFiles);
            CollectionAssert.AreEquivalent(expectedFileIds, fileIds);
        }

        [TestMethod]
        public void UpsertMediaFiles_ShouldUpdateExistingFiles() {
            var files = new List<MediaFile>();

            for (int i = 0; i < 3; ++i) {
                var newFile = new MediaFile(-1, "https://google.ca", MediaFileType.VIDEO_TYPE, $"file {i}", DateTime.Now, UniqueIdUtil.GenerateUniqueId());
                files.Add(newFile);
            }

            repository.UpsertMediaFiles(files, new Dictionary<string, long>()).ConfigureAwait(false);
            files[0].Name = "new 1";
            files[2].Name = "new 2";
            repository.UpsertMediaFiles(files, new Dictionary<string, long>()).ConfigureAwait(false);

            var retrievedFiles = repository.GetMediaFiles(GetFirstPage()).Result.Results;
            foreach (var file in files) {
                foreach (var retrieved in retrievedFiles) {
                    if (file.UniqueId == retrieved.UniqueId) {
                        file.MediaId = retrieved.MediaId;
                        break;
                    }
                }
            }

            CollectionAssert.AreEquivalent(files, retrievedFiles);
        }
    }
}

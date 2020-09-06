using System.Collections.Generic;
using Ingvilt.Dto;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class LibraryRepositoryTests : BaseTest {
        private LibraryRepository repository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();

        private Library CreateAndRetrieveLibrary(string libraryName) {
            CreateLibraryDto libraryToCreate = new CreateLibraryDto(libraryName);
            var libraryId = repository.CreateLibrary(libraryToCreate);
            return repository.GetLibrary(libraryId);
        }

        private Pagination GetDefaultPagination() {
            return Pagination.FirstPageWithDefaultCount(new LibrarySortCriteria(true));
        }

        private List<Library> GetSavedLibraries(bool deleted = false) {
            if (deleted) {
                return repository.GetDeletedLibraries(GetDefaultPagination(), "").Result.Results;
            }

            return repository.GetLibraries(GetDefaultPagination(), "").Result.Results;
        }

        private bool DoSettingsForLibraryExist(long libraryId) {
            bool hasSettingsForLibrary = true;

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT * FROM library_setting WHERE library_id = {libraryId}", db);
                var query = command.ExecuteReader();
                hasSettingsForLibrary = query.Read();

                db.Close();
            }

            return hasSettingsForLibrary;
        }

        [TestMethod]
        public void TestCreateLibrary() {
            CreateLibraryDto libraryToCreate = new CreateLibraryDto("Test library");
            var libraryId = repository.CreateLibrary(libraryToCreate);

            var libraryRetrieved = repository.GetLibrary(libraryId);

            Assert.IsTrue(libraryRetrieved.LibraryId > -1);
            Assert.AreEqual(libraryToCreate.Name, libraryRetrieved.Name);
            Assert.IsTrue(DoSettingsForLibraryExist(libraryRetrieved.LibraryId));
            Assert.IsNotNull(libraryRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestCreateLibrarySettings() {
            CreateLibraryDto libraryToCreate = new CreateLibraryDto("Test library");
            var libraryId = repository.CreateLibrary(libraryToCreate);

            var settings = repository.GetLibrarySettings(libraryId);
            Assert.AreEqual(LibrarySettings.DEFAULT_PUBLISHER_LABEL, settings.PublisherLabel);
            Assert.AreEqual(LibrarySettings.DEFAULT_SERIES_LABEL, settings.SeriesLabel);
            Assert.AreEqual(LibrarySettings.DEFAULT_CHARACTER_LABEL, settings.CharacterLabel);
            Assert.AreEqual(LibrarySettings.DEFAULT_VIDEO_LABEL, settings.VideoLabel);
        }

        [TestMethod]
        public void TestUpdateLibrarySettings() {
            CreateLibraryDto libraryToCreate = new CreateLibraryDto("Test library");
            var libraryId = repository.CreateLibrary(libraryToCreate);
            var settings = repository.GetLibrarySettings(libraryId);

            var newPublisherLabel = "modified_publisher";
            var newSeriesLabel = "modified_series";
            var newVideoLabel = "modified_video";
            var newCharacterLabel = "modified_character";

            settings.PublisherLabel = newPublisherLabel;
            settings.SeriesLabel = newSeriesLabel;
            settings.VideoLabel = newVideoLabel;
            settings.CharacterLabel = newCharacterLabel;

            repository.UpdateLibrarySettings(libraryId, settings);
            settings = repository.GetLibrarySettings(libraryId);
            
            Assert.AreEqual(newPublisherLabel, settings.PublisherLabel);
            Assert.AreEqual(newSeriesLabel, settings.SeriesLabel);
            Assert.AreEqual(newCharacterLabel, settings.CharacterLabel);
            Assert.AreEqual(newVideoLabel, settings.VideoLabel);
        }

        [TestMethod]
        public void TestGetLibrariesWithNoneCreated() {
            var libraries = GetSavedLibraries();
            Assert.AreEqual(0, libraries.Count);
        }

        [TestMethod]
        public void TestGetLibrariesWithOneCreated() {
            var library = CreateAndRetrieveLibrary("Test library");
            var libraries = GetSavedLibraries();

            var expectedLibraries = new List<Library>();
            expectedLibraries.Add(library);

            CollectionAssert.AreEquivalent(expectedLibraries, libraries);
        }

        [TestMethod]
        public void TestGetLibrariesWithMultipleCreated() {
            var expectedLibraries = new List<Library>();
            for (int i = 0; i < 10; ++i) {
                var library = CreateAndRetrieveLibrary("Test library " + i.ToString());
                expectedLibraries.Add(library);
            }

            var libraries = GetSavedLibraries();

            CollectionAssert.AreEquivalent(expectedLibraries, libraries);
        }

        [TestMethod]
        public void TestPermanentlyRemoveLibrary() {
            var library = CreateAndRetrieveLibrary("test library");
            repository.PermanentlyDeleteLibrary(library.LibraryId);

            var libraries = GetSavedLibraries();

            Assert.AreEqual(0, libraries.Count);
            Assert.IsFalse(DoSettingsForLibraryExist(library.LibraryId));
        }

        [TestMethod]
        public void TestRemoveLibrary() {
            var library = CreateAndRetrieveLibrary("test library");
            repository.DeleteLibrary(library.LibraryId);

            var libraries = GetSavedLibraries();

            Assert.AreEqual(0, libraries.Count);
            Assert.IsTrue(DoSettingsForLibraryExist(library.LibraryId));
        }

        [TestMethod]
        public void TestRemoveLibraryShouldOnlyRemoveOne() {
            var libraryToRemove = CreateAndRetrieveLibrary("test library");

            var expectedLibraries = new List<Library>();
            for (int i = 0; i < 10; ++i) {
                var library = CreateAndRetrieveLibrary("Test library " + i.ToString());
                expectedLibraries.Add(library);
            }

            repository.DeleteLibrary(libraryToRemove.LibraryId);

            var libraries = GetSavedLibraries();
            CollectionAssert.AreEquivalent(expectedLibraries, libraries);
        }

        [TestMethod]
        public void TestGetDeletedLibrariesWithNoDeletedLibraries() {
            for (int i = 0; i < 10; ++i) {
                CreateAndRetrieveLibrary("Test library " + i.ToString());
            }

            var libraries = GetSavedLibraries(true);
            CollectionAssert.AreEquivalent(new List<Library>(), libraries);
        }

        [TestMethod]
        public void TestGetDeletedLibrariesWithDeletedLibraries() {
            var expectedLibraries = new List<Library>();
            for (int i = 0; i < 3; ++i) {
                var library = CreateAndRetrieveLibrary("Test library " + i.ToString());
                expectedLibraries.Add(library);
            }

            for (int i = 3; i < 6; ++i) {
                CreateAndRetrieveLibrary("Test library " + i.ToString());
            }

            foreach (var library in expectedLibraries) {
                repository.DeleteLibrary(library.LibraryId);
                library.Deleted = true;
            }

            var libraries = GetSavedLibraries(true);
            CollectionAssert.AreEquivalent(expectedLibraries, libraries);
        }

        [TestMethod]
        public void TestUpdateLibrary() {
            var libraryId = repository.CreateLibrary(new CreateLibraryDto("Test library"));
            var libraryRetrieved = repository.GetLibrary(libraryId);

            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("https://localhost.com", MediaFileType.IMAGE_TYPE, "Test Name"));
            var modifiedLibrary = new Library(libraryId, libraryRetrieved.Name + "2", fileId);

            repository.UpdateLibrary(modifiedLibrary);
            libraryRetrieved = repository.GetLibrary(libraryId);

            Assert.AreEqual(modifiedLibrary.Name, libraryRetrieved.Name);
            Assert.AreEqual(modifiedLibrary.BackgroundImageId, libraryRetrieved.BackgroundImageId);
        }
    }
}

using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Publishers;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using Ingvilt.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class LocationRepositoryTests : BaseTest {
        private LocationRepository repository = new LocationRepository();
        private LibraryRepository libraryRepo = new LibraryRepository();

        private Library testLibrary = null;

        private List<Location> GetSavedLocations(bool deleted = false) {
            var pagination = Pagination.FirstPageWithDefaultCount(new LocationSortCriteria(true));
            if (deleted) {
                return repository.GetDeletedLocations(pagination).Result.Results;
            }

            return repository.GetLocations(pagination).Result.Results;
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new LocationSortCriteria(true));
        }

        private long CreateNewLocation(Library library = null, Publisher publisher = null) {
            long publisherId = -1;
            if (publisher != null) {
                publisherId = publisher.PublisherId;
            }

            if (library == null) {
                library = testLibrary;
            }

            var locationToCreate = new CreateLocationDto("Test library", "test desc", library.LibraryId, publisherId, -1);
            return repository.CreateLocation(locationToCreate);
        }

        private Location CreateAndRetrieveLocation(Library library = null, Publisher publisher = null) {
            var locationId = CreateNewLocation(library, publisher);
            return repository.GetLocation(locationId);
        }

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepo.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public LocationRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        [TestMethod]
        public void TestCreateLocation() {
            var tagName = "Test name";
            var description = "Test tag";
            var locationId = repository.CreateLocation(new CreateLocationDto(tagName, description, testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
            var location = repository.GetLocation(locationId);

            Assert.AreEqual(tagName, location.Name);
            Assert.AreEqual(description, location.Description);
            Assert.AreEqual(locationId, location.LocationId);
            Assert.AreEqual(testLibrary.LibraryId, location.LibraryId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, location.PublisherId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, location.CoverFileId);
            Assert.IsNotNull(location.UniqueId);
        }

        [TestMethod]
        public void TestCreateLocation_WithCover() {
            var tagName = "Test name";
            var description = "Test tag";
            var coverFileId = new MediaFileRepository().CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            
            var locationId = repository.CreateLocation(new CreateLocationDto(tagName, description, testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, coverFileId));
            var location = repository.GetLocation(locationId);

            Assert.AreEqual(tagName, location.Name);
            Assert.AreEqual(description, location.Description);
            Assert.AreEqual(locationId, location.LocationId);
            Assert.AreEqual(testLibrary.LibraryId, location.LibraryId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, location.PublisherId);
            Assert.AreEqual(coverFileId, location.CoverFileId);
        }

        [TestMethod]
        public void TestCreateLocation_WithPublisher() {
            var publisherDto = new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId);
            var publisher = new PublisherService().CreateAndRetrievePublisher(publisherDto);

            var tagName = "Test name";
            var description = "Test tag";
            var locationId = repository.CreateLocation(new CreateLocationDto(tagName, description, testLibrary.LibraryId, publisher.PublisherId, -1));
            var location = repository.GetLocation(locationId);

            Assert.AreEqual(tagName, location.Name);
            Assert.AreEqual(description, location.Description);
            Assert.AreEqual(locationId, location.LocationId);
            Assert.AreEqual(testLibrary.LibraryId, location.LibraryId);
            Assert.AreEqual(publisher.PublisherId, location.PublisherId);
            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, location.CoverFileId);
        }

        [TestMethod]
        public void GetLocation_ShouldReturnDeletedTag() {
            var tagName = "Test name";
            var description = "Test tag";
            var locationId = repository.CreateLocation(new CreateLocationDto(tagName, description, testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
            var location = repository.GetLocation(locationId);

            repository.DeleteLocation(location).ConfigureAwait(false);
            Assert.AreEqual(location, repository.GetLocation(locationId));
        }

        [TestMethod]
        public void GetLocation_ShouldntReturnPermanentlyDeletedTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var locationId = repository.CreateLocation(new CreateLocationDto(tagName, tagType, testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
            var tag = repository.GetLocation(locationId);

            repository.PermanentlyRemoveLocation(tag).ConfigureAwait(false);
            try {
                repository.GetLocation(locationId);
                Assert.IsFalse(true, "A deleted video tag was returned");
            } catch (ArgumentException) {

            }
        }

        [TestMethod]
        public void GetLocations_ShouldReturnNonDeletedTags() {
            var tags = new List<Location>();
            for (int i = 0; i < 3; ++i) {
                var locationId = repository.CreateLocation(new CreateLocationDto("name", "type", testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
                tags.Add(repository.GetLocation(locationId));
            }

            var deletedLocationId = repository.CreateLocation(new CreateLocationDto("name", "type", testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
            var deletedTag = repository.GetLocation(deletedLocationId);
            repository.DeleteLocation(deletedTag).ConfigureAwait(false);

            CollectionAssert.AreEquivalent(tags, GetSavedLocations());
        }

        [TestMethod]
        public void GetDeletedLocations_ShouldReturnDeletedTags() {
            var tags = new List<Location>();
            for (int i = 0; i < 3; ++i) {
                var locationId = repository.CreateLocation(new CreateLocationDto("name", "type", testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
                repository.GetLocation(locationId);
            }

            for (int i = 0; i < 3; ++i) {
                var deletedLocationId = repository.CreateLocation(new CreateLocationDto("name", "type", testLibrary.LibraryId, DatabaseConstants.DEFAULT_ID, -1));
                var deletedTag = repository.GetLocation(deletedLocationId);
                tags.Add(deletedTag);
                repository.DeleteLocation(deletedTag).ConfigureAwait(false);
            }

            CollectionAssert.AreEquivalent(tags, GetSavedLocations(true));
        }

        [TestMethod]
        public void TestUpdateLocation() {
            var locationToCreate = new CreateLocationDto("Test library", "test desc", testLibrary.LibraryId, -1, -1);
            var locationId = repository.CreateLocation(locationToCreate);

            var locationRetrieved = repository.GetLocation(locationId);
            locationRetrieved.Description += "1";
            locationRetrieved.Name += "2";
            repository.UpdateLocation(locationRetrieved);

            var updatedLocationRetrieved = repository.GetLocation(locationId);

            Assert.AreEqual(locationRetrieved.LocationId, updatedLocationRetrieved.LocationId);
            Assert.AreEqual(locationRetrieved.Name, updatedLocationRetrieved.Name);
            Assert.AreEqual(locationRetrieved.Description, updatedLocationRetrieved.Description);
            Assert.AreEqual(locationRetrieved.LibraryId, updatedLocationRetrieved.LibraryId);
            Assert.AreEqual(locationRetrieved.PublisherId, updatedLocationRetrieved.PublisherId);
            CollectionAssert.AreEquivalent(new List<Location>() { updatedLocationRetrieved }, repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdateLocation_ShouldNotUpdateDifferentLocation() {
            var locationDto = new CreateLocationDto("Test library", "test desc", testLibrary.LibraryId, -1, -1);

            var locationToUpdateId = repository.CreateLocation(locationDto);
            var locationNotUpdatedId = repository.CreateLocation(locationDto);

            var locationToUpdate = repository.GetLocation(locationToUpdateId);
            locationToUpdate.Description += "1";
            repository.UpdateLocation(locationToUpdate);

            var locationToNotUpdate = repository.GetLocation(locationNotUpdatedId);

            Assert.AreNotEqual(locationToUpdate.Description, locationToNotUpdate.Description);
        }

        [TestMethod]
        public void ShouldNotDeleteLocation_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var locationId = CreateNewLocation(otherLibrary);
            var locationRetrieved = repository.GetLocation(locationId);

            libraryRepo.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteLocation_WhenLibraryRestored() {
            var locationId = CreateNewLocation();

            libraryRepo.DeleteLibrary(testLibrary.LibraryId);
            libraryRepo.RestoreDeletedLibrary(testLibrary.LibraryId);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteLocation_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var locationId = CreateNewLocation(otherLibrary);

            repository.DeleteLocation(locationId).ConfigureAwait(false);
            libraryRepo.DeleteLibrary(testLibrary.LibraryId);
            libraryRepo.RestoreDeletedLibrary(testLibrary.LibraryId);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreLocationDeletedNormally_WhenLibraryRestored() {
            var locationId = CreateNewLocation();
            repository.DeleteLocation(locationId).ConfigureAwait(false);

            libraryRepo.DeleteLibrary(testLibrary.LibraryId);
            libraryRepo.RestoreDeletedLibrary(testLibrary.LibraryId);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetLocationsInLibrary_ShouldntReturnDeletedLocations() {
            var expectedLocations = new List<Location>();
            for (int i = 0; i < 3; ++i) {
                var location = CreateAndRetrieveLocation();
                expectedLocations.Add(location);
            }
            var deletedLocation = CreateAndRetrieveLocation();
            repository.DeleteLocation(deletedLocation.LocationId).ConfigureAwait(false);

            var locations = repository.GetLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedLocations, locations);
        }

        [TestMethod]
        public void GetLocationsInLibrary_ShouldntReturnLocationsInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            CreateNewLocation(otherLibrary);

            var locations = repository.GetLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Location>(), locations);
        }

        [TestMethod]
        public void GetDeletedLocationsInLibrary_ShouldOnlyReturnDeletedLocations() {
            var expectedLocations = new List<Location>();
            for (int i = 0; i < 3; ++i) {
                var location = CreateAndRetrieveLocation();
                repository.DeleteLocation(location.LocationId).ConfigureAwait(false);
                expectedLocations.Add(location);
            }
            CreateAndRetrieveLocation();

            var locations = repository.GetDeletedLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedLocations, locations);
        }

        [TestMethod]
        public void GetDeletedLocationsInLibrary_ShouldntReturnLocationsInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var locationId = CreateNewLocation(otherLibrary);

            repository.GetLocation(locationId);
            repository.DeleteLocation(locationId).ConfigureAwait(false);

            var locations = repository.GetDeletedLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Location>(), locations);
        }

        [TestMethod]
        public void RestoreDeleteLocation_ShouldUndeleteLocation() {
            var expectedLocations = new List<Location>();
            for (int i = 0; i < 2; ++i) {
                var location = CreateAndRetrieveLocation();
                expectedLocations.Add(location);
            }
            var deletedLocation = CreateAndRetrieveLocation();
            expectedLocations.Add(deletedLocation);
            repository.DeleteLocation(deletedLocation.LocationId).ConfigureAwait(false);
            repository.RestoreLocation(deletedLocation.LocationId).ConfigureAwait(false);

            var locations = repository.GetLocations(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedLocations, locations);
        }

        [TestMethod]
        public void ShouldDeleteLocation_WhenPublisherDeleted() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));
            var locationId = CreateNewLocation(testLibrary, publisher);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotDeleteLocation_WhenDifferentPublisherDeleted() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var locationId = CreateNewLocation();
            var locationRetrieved = repository.GetLocation(locationId);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);

            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteLocation_WhenPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var locationId = CreateNewLocation(testLibrary, publisher);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteLocation_WhenDifferentPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var locationId = CreateNewLocation();

            repository.DeleteLocation(locationId).ConfigureAwait(false);
            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreLocationDeletedNormally_WhenPublisherRestored() {
            var publisherService = new PublisherService();
            var publisher = publisherService.CreateAndRetrievePublisher(new CreatePublisherDto("", "", -1, "", testLibrary.LibraryId));

            var locationId = CreateNewLocation(testLibrary, publisher);
            repository.DeleteLocation(locationId).ConfigureAwait(false);

            publisherService.DeletePublisher(publisher).ConfigureAwait(false);
            publisherService.RestorePublisher(publisher).ConfigureAwait(false);
            var locationRetrieved = repository.GetLocation(locationId);

            CollectionAssert.AreEquivalent(new List<Location>() { locationRetrieved }, repository.GetDeletedLocations(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Location>(), repository.GetLocations(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void UpsertLocations_ShouldInsertNewItems() {
            var locations = new List<ExportedLocationSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var location = new Location(-1, "test " + i, "desc", testLibrary.LibraryId, -1, -1, false, UniqueIdUtil.GenerateUniqueId());
                locations.Add(new ExportedLocationSimpleDto(location, null, null));
            }

            var idsMap = new Dictionary<string, long>();
            repository.UpsertLocations(locations, idsMap).ConfigureAwait(false);

            var retLocs = repository.GetLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            foreach (var item in locations) {
                foreach (var retrieved in retLocs) {
                    if (item.Details.UniqueId == retrieved.UniqueId) {
                        item.Details.CoverFileId = retrieved.CoverFileId;
                        break;
                    }
                }
            }
            var expectedIds = new Dictionary<string, long>();
            foreach (var loc in retLocs) {
                expectedIds[loc.UniqueId] = loc.LocationId;
            }

            var expectedLocs = locations.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedLocs, retLocs);
            CollectionAssert.AreEquivalent(expectedIds, idsMap);
        }

        [TestMethod]
        public void UpsertLocations_ShouldUpdateExistingItems() {
            var locations = new List<ExportedLocationSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var location = new Location(-1, "test " + i, "desc", testLibrary.LibraryId, -1, -1, false, UniqueIdUtil.GenerateUniqueId());
                locations.Add(new ExportedLocationSimpleDto(location, null, null));
            }

            repository.UpsertLocations(locations, new Dictionary<string, long>()).ConfigureAwait(false);
            locations[0].Details.Name = "new 0";
            locations[2].Details.Name = "new 2";
            repository.UpsertLocations(locations, new Dictionary<string, long>()).ConfigureAwait(false);

            var retLocs = repository.GetLocationsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            foreach (var item in locations) {
                foreach (var retrieved in retLocs) {
                    if (item.Details.UniqueId == retrieved.UniqueId) {
                        item.Details.CoverFileId = retrieved.CoverFileId;
                        break;
                    }
                }
            }

            var expectedLocs = locations.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedLocs, retLocs);
        }
    }
}

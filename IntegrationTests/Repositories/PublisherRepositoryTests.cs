using Ingvilt.Dto;
using Ingvilt.Dto.Publishers;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using Ingvilt.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class PublisherRepositoryTests : BaseTest {
        private PublisherRepository repository = new PublisherRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();

        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public PublisherRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Publisher CreateAndRetrievePublisher(CreatePublisherDto dto) {
            var id = repository.CreatePublisher(dto);
            return repository.GetPublisher(id).Result;
        }

        private Publisher CreateAndRetrievePublisher() {
            return CreateAndRetrievePublisher(new CreatePublisherDto("test", "", -1, "", testLibrary.LibraryId));
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new PublisherSortCriteria(true));
        }

        [TestMethod]
        public void TestCreatePublisher_WithNullLogo() {
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", testLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);

            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            Assert.AreEqual(publisherId, publisherRetrieved.PublisherId);
            Assert.AreEqual(publisherToCreate.Name, publisherRetrieved.Name);
            Assert.AreEqual(publisherToCreate.Description, publisherRetrieved.Description);
            Assert.AreEqual(publisherToCreate.LibraryId, publisherRetrieved.LibraryId);
            Assert.AreEqual(publisherToCreate.LogoFileId, publisherRetrieved.LogoFileId);
            Assert.AreEqual(publisherToCreate.SiteURL, publisherRetrieved.SiteURL);
            Assert.IsNotNull(publisherRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestCreatePublisher_WithLogo() {
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", fileId, "test desc", testLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);

            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            Assert.AreEqual(publisherId, publisherRetrieved.PublisherId);
            Assert.AreEqual(publisherToCreate.Name, publisherRetrieved.Name);
            Assert.AreEqual(publisherToCreate.Description, publisherRetrieved.Description);
            Assert.AreEqual(publisherToCreate.LibraryId, publisherRetrieved.LibraryId);
            Assert.AreEqual(publisherToCreate.LogoFileId, publisherRetrieved.LogoFileId);
            Assert.AreEqual(publisherToCreate.SiteURL, publisherRetrieved.SiteURL);
        }

        [TestMethod]
        public void TestUpdatePublisher() {
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", testLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);

            var publisherRetrieved = repository.GetPublisher(publisherId).Result;
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            publisherRetrieved.Description += "1";
            publisherRetrieved.Name += "2";
            publisherRetrieved.SiteURL = "https://ingvilt.test2.url";
            publisherRetrieved.LogoFileId = fileId;
            repository.UpdatePublisher(publisherRetrieved);

            var updatedPublisherRetrieved = repository.GetPublisher(publisherId).Result;

            Assert.AreEqual(publisherRetrieved.PublisherId, updatedPublisherRetrieved.PublisherId);
            Assert.AreEqual(publisherRetrieved.Name, updatedPublisherRetrieved.Name);
            Assert.AreEqual(publisherRetrieved.Description, updatedPublisherRetrieved.Description);
            Assert.AreEqual(publisherRetrieved.LibraryId, updatedPublisherRetrieved.LibraryId);
            Assert.AreEqual(publisherRetrieved.LogoFileId, updatedPublisherRetrieved.LogoFileId);
            Assert.AreEqual(publisherRetrieved.SiteURL, updatedPublisherRetrieved.SiteURL);
            CollectionAssert.AreEquivalent(new List<Publisher>() { updatedPublisherRetrieved }, repository.GetPublishers(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdatePublisher_ShouldNotUpdateDifferentPublisher() {
            var publisherDto = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", testLibrary.LibraryId);
            
            var publisherToUpdateId = repository.CreatePublisher(publisherDto);
            var publisherNotUpdatedId = repository.CreatePublisher(publisherDto);

            var publisherToUpdate = repository.GetPublisher(publisherToUpdateId).Result;
            publisherToUpdate.Description += "1";
            repository.UpdatePublisher(publisherToUpdate);

            var publisherToNotUpdate = repository.GetPublisher(publisherNotUpdatedId).Result;

            Assert.AreNotEqual(publisherToUpdate.Description, publisherToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetPublishersWithNoneCreated() {
            var publishers = repository.GetPublishers(GetFirstPage()).Result.Results;
            Assert.AreEqual(0, publishers.Count);
        }

        [TestMethod]
        public void TestGetPublishersWithOneCreated() {
            var publisher = CreateAndRetrievePublisher();
            var publishers = repository.GetPublishers(GetFirstPage()).Result.Results;

            var expectedPublishers = new List<Publisher>();
            expectedPublishers.Add(publisher);

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void TestGetPublishersWithMultipleCreated() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 5; ++i) {
                var publisher = CreateAndRetrievePublisher();
                expectedPublishers.Add(publisher);
            }

            var publishers = repository.GetPublishers(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void GetPublishers_ShouldntReturnDeletedPublishers() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 5; ++i) {
                var publisher = CreateAndRetrievePublisher();
                expectedPublishers.Add(publisher);
            }
            var deletedPublisher = CreateAndRetrievePublisher();
            repository.DeletePublisher(deletedPublisher.PublisherId).ConfigureAwait(false);

            var publishers = repository.GetPublishers(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void GetDeletedPublishers_ShouldOnlyReturnDeletedPublishers() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 5; ++i) {
                var publisher = CreateAndRetrievePublisher();
                repository.DeletePublisher(publisher.PublisherId).ConfigureAwait(false);
                expectedPublishers.Add(publisher);
                publisher.Deleted = true;
            }
            CreateAndRetrievePublisher();

            var publishers = repository.GetDeletedPublishers(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void ShouldNotDeletePublisher_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", otherLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);
            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Publisher>(), repository.GetDeletedPublishers(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Publisher>() { publisherRetrieved }, repository.GetPublishers(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeletePublisher_WhenLibraryRestored() {
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", testLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            CollectionAssert.AreEquivalent(new List<Publisher>(), repository.GetDeletedPublishers(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Publisher>() { publisherRetrieved }, repository.GetPublishers(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeletePublisher_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", otherLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);

            repository.DeletePublisher(publisherId).ConfigureAwait(false);
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            CollectionAssert.AreEquivalent(new List<Publisher>() { publisherRetrieved }, repository.GetDeletedPublishers(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Publisher>(), repository.GetPublishers(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestorePublisherDeletedNormally_WhenLibraryRestored() {
            var publisherToCreate = new CreatePublisherDto("Test library", "https://ingvilt.test.url", -1, "test desc", testLibrary.LibraryId);
            var publisherId = repository.CreatePublisher(publisherToCreate);
            repository.DeletePublisher(publisherId).ConfigureAwait(false);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var publisherRetrieved = repository.GetPublisher(publisherId).Result;

            CollectionAssert.AreEquivalent(new List<Publisher>() { publisherRetrieved }, repository.GetDeletedPublishers(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Publisher>(), repository.GetPublishers(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetPublishersInLibrary_ShouldntReturnDeletedPublishers() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 3; ++i) {
                var publisher = CreateAndRetrievePublisher();
                expectedPublishers.Add(publisher);
            }
            var deletedPublisher = CreateAndRetrievePublisher();
            repository.DeletePublisher(deletedPublisher.PublisherId).ConfigureAwait(false);

            var publishers = repository.GetPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void GetPublishersInLibrary_ShouldntReturnPublishersInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            repository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", otherLibrary.LibraryId));

            var publishers = repository.GetPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Publisher>(), publishers);
        }

        [TestMethod]
        public void GetDeletedPublishersInLibrary_ShouldOnlyReturnDeletedPublishers() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 3; ++i) {
                var publisher = CreateAndRetrievePublisher();
                repository.DeletePublisher(publisher.PublisherId).ConfigureAwait(false);
                expectedPublishers.Add(publisher);
                publisher.Deleted = true;
            }
            CreateAndRetrievePublisher();

            var publishers = repository.GetDeletedPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void GetDeletedPublishersInLibrary_ShouldntReturnPublishersInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var publisherId = repository.CreatePublisher(new CreatePublisherDto("test", "", -1, "", otherLibrary.LibraryId));
            repository.GetPublisher(publisherId).ConfigureAwait(false);
            repository.DeletePublisher(publisherId).ConfigureAwait(false);

            var publishers = repository.GetDeletedPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Publisher>(), publishers);
        }

        [TestMethod]
        public void RestoreDeletePublisher_ShouldUndeletePublisher() {
            var expectedPublishers = new List<Publisher>();
            for (int i = 0; i < 2; ++i) {
                var publisher = CreateAndRetrievePublisher();
                expectedPublishers.Add(publisher);
            }
            var deletedPublisher = CreateAndRetrievePublisher();
            expectedPublishers.Add(deletedPublisher);
            repository.DeletePublisher(deletedPublisher.PublisherId).ConfigureAwait(false);
            repository.RestoreDeletedPublisher(deletedPublisher.PublisherId).ConfigureAwait(false);

            var publishers = repository.GetPublishers(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedPublishers, publishers);
        }

        [TestMethod]
        public void UpsertPublishers_ShouldInsertNewPublishers() {
            var publishers = new List<ExportedPublisherSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var publisher = new Publisher(-1, "test " + i, "https://google.ca", -1, "desc", testLibrary.LibraryId, false, UniqueIdUtil.GenerateUniqueId());
                publishers.Add(new ExportedPublisherSimpleDto(publisher, null));
            }

            var publisherIds = new Dictionary<string, long>();
            repository.UpsertPublishers(publishers, publisherIds).ConfigureAwait(false);

            var retrievedPublishers = repository.GetPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            foreach (var publisher in publishers) {
                foreach (var retrieved in retrievedPublishers) {
                    if (publisher.Details.UniqueId == retrieved.UniqueId) {
                        publisher.Details.LogoFileId = retrieved.LogoFileId;
                        break;
                    }
                }
            }

            var expectedPublishers = publishers.Select(p => p.Details).ToList();
            var expectedPublisherIds = new Dictionary<string, long>();

            foreach (var publisher in publishers) {
                expectedPublisherIds[publisher.Details.UniqueId] = publisher.Details.PublisherId;
            }

            CollectionAssert.AreEquivalent(expectedPublishers, retrievedPublishers);
            CollectionAssert.AreEquivalent(expectedPublisherIds, publisherIds);
        }

        [TestMethod]
        public void UpsertPublishers_ShouldUpdateExistingPublishers() {
            var publishers = new List<ExportedPublisherSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var publisher = new Publisher(-1, "test " + i, "https://google.ca", -1, "desc", testLibrary.LibraryId, false, UniqueIdUtil.GenerateUniqueId());
                publishers.Add(new ExportedPublisherSimpleDto(publisher, null));
            }

            repository.UpsertPublishers(publishers, new Dictionary<string, long>()).ConfigureAwait(false);
            publishers[0].Details.Name = "new 0";
            publishers[2].Details.Name = "new 2";
            repository.UpsertPublishers(publishers, new Dictionary<string, long>()).ConfigureAwait(false);

            var retrievedPublishers = repository.GetPublishersInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            foreach (var publisher in publishers) {
                foreach (var retrieved in retrievedPublishers) {
                    if (publisher.Details.UniqueId == retrieved.UniqueId) {
                        publisher.Details.LogoFileId = retrieved.LogoFileId;
                        break;
                    }
                }
            }

            var expectedPublishers = publishers.Select(p => p.Details).ToList();

            CollectionAssert.AreEquivalent(expectedPublishers, retrievedPublishers);
        }
    }
}

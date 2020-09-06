using Ingvilt.Constants;
using Ingvilt.Core;
using Ingvilt.Dto.Publishers;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class PublisherService {
        private PublisherRepository publisherRepository;

        private PublisherBasicDetails GetDefaultPublisherBasicDetails() {
            return new PublisherBasicDetails(DatabaseConstants.DEFAULT_ID, "-");
        }

        public PublisherService() {
            publisherRepository = DependencyInjectionContainer.Container.Resolve<PublisherRepository>();
        }

        public async Task<PaginationResult<Publisher>> GetPublishers(Pagination pagination) {
            return await publisherRepository.GetPublishers(pagination);
        }

        public async Task<PaginationResult<Publisher>> GetDeletedPublishers(Pagination pagination) {
            return await publisherRepository.GetDeletedPublishers(pagination);
        }

        public async Task<PaginationResult<Publisher>> GetPublishersInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await publisherRepository.GetPublishersInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<Publisher>> GetDeletedPublishersInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await publisherRepository.GetDeletedPublishersInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<Publisher> GetPublisher(long publisherId) {
            return await publisherRepository.GetPublisher(publisherId);
        }

        public long CreatePublisher(CreatePublisherDto dto) {
            return publisherRepository.CreatePublisher(dto);
        }

        public Publisher CreateAndRetrievePublisher(CreatePublisherDto dto) {
            long libraryId = CreatePublisher(dto);
            return new Publisher(libraryId, dto);
        }

        public async Task DeletePublisher(Publisher publisher) {
            await publisherRepository.DeletePublisher(publisher.PublisherId);
        }

        public void UpdatePublisher(Publisher publisher) {
            publisherRepository.UpdatePublisher(publisher);
        }

        public async Task RestorePublisher(Publisher publisher) {
            await publisherRepository.RestoreDeletedPublisher(publisher.PublisherId);
        }

        public async Task PermanentlyRemovePublisher(Publisher publisher) {
            await publisherRepository.PermanentlyRemovePublisher(publisher.PublisherId);
        }

        public async Task<List<PublisherBasicDetails>> GetAllActivePublishersInLibrary(long libraryId) {
            var allPublishers = await publisherRepository.GetAllActivePublishersInLibrary(libraryId);
            allPublishers.Insert(0, GetDefaultPublisherBasicDetails());
            return allPublishers;
        }

        public async Task<PublisherBasicDetails> GetPublisherBasicDetails(long publisherId) {
            if (publisherId == DatabaseConstants.DEFAULT_ID) {
                return GetDefaultPublisherBasicDetails();
            }

            return await publisherRepository.GetPublisherBasicDetails(publisherId);
        }
    }
}

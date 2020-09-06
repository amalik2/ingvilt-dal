using Ingvilt.Core;
using Ingvilt.Dto.Locations;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class LocationService {
        private LocationRepository locationRepository;

        public LocationService() {
            locationRepository = DependencyInjectionContainer.Container.Resolve<LocationRepository>();
        }

        public async Task<PaginationResult<Location>> GetLocations(Pagination pagination) {
            return await locationRepository.GetLocations(pagination);
        }

        public async Task<PaginationResult<Location>> GetDeletedLocations(Pagination pagination) {
            return await locationRepository.GetDeletedLocations(pagination);
        }

        public async Task<PaginationResult<Location>> GetLocationsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await locationRepository.GetLocationsInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<Location>> GetDeletedLocationsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await locationRepository.GetDeletedLocationsInLibrary(libraryId, pagination, nameFilter);
        }

        public Location GetLocation(long locationId) {
            return locationRepository.GetLocation(locationId);
        }

        public long CreateLocation(CreateLocationDto dto) {
            return locationRepository.CreateLocation(dto);
        }

        public Location CreateAndRetrieveLocation(CreateLocationDto dto) {
            long locationId = CreateLocation(dto);
            return new Location(locationId, dto);
        }

        public async Task DeleteLocation(Location location) {
            await locationRepository.DeleteLocation(location);
        }

        public void UpdateLocation(Location location) {
            locationRepository.UpdateLocation(location);
        }

        public async Task RestoreLocation(Location location) {
            await locationRepository.RestoreLocation(location);
        }

        public async Task PermanentlyRemoveLocation(Location location) {
            await locationRepository.PermanentlyRemoveLocation(location);
        }

        public async Task<PaginationResult<Location>> GetLocationsVideoIsAt(long videoId, Pagination pagination) {
            return await locationRepository.GetLocationsVideoIsAt(videoId, pagination);
        }

        public async Task<PaginationResult<Location>> GetLocationsForSelection(string locationTableName, string parentIdColumnName, long parentId, Pagination pagination) {
            return await locationRepository.GetLocationsForSelection(locationTableName, parentIdColumnName, parentId, pagination);
        }

        public async Task AddLocationsToVideo(List<long> locationIds, long videoId) {
            await locationRepository.AddLocationsToVideo(locationIds, videoId);
        }

        public async Task RemoveLocationFromVideo(long locationId, long videoId) {
            await locationRepository.RemoveLocationFromVideo(locationId, videoId);
        }

        public async Task<PaginationResult<Location>> GetLocationsWithFile(Pagination pagination, long fileId) {
            return await locationRepository.GetLocationsWithFile(pagination, fileId);
        }
    }
}

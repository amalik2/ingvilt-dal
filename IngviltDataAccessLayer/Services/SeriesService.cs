using Ingvilt.Constants;
using Ingvilt.Core;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search.SeriesNS;
using Ingvilt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class SeriesService {
        private SeriesRepository seriesRepository;

        private SeriesBasicDetails GetDefaultSeriesBasicDetails() {
            return new SeriesBasicDetails(DatabaseConstants.DEFAULT_ID, "-", DatabaseConstants.DEFAULT_ID);
        }

        public SeriesService() {
            seriesRepository = DependencyInjectionContainer.Container.Resolve<SeriesRepository>();
        }

        public async Task<PaginationResult<Series>> GetSeries(Pagination pagination) {
            return await seriesRepository.GetSeries(pagination);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeries(Pagination pagination) {
            return await seriesRepository.GetDeletedSeries(pagination);
        }

        public async Task<PaginationResult<Series>> GetSeriesInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await seriesRepository.GetSeriesInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeriesInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await seriesRepository.GetDeletedSeriesInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<Series>> GetSeriesByPublisher(Pagination pagination, long publisherId) {
            return await seriesRepository.GetSeriesByPublisher(publisherId, pagination);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeriesByPublisher(Pagination pagination, long publisherId) {
            return await seriesRepository.GetDeletedSeriesByPublisher(publisherId, pagination);
        }

        public async Task<Series> GetSeries(long seriesId) {
            return await seriesRepository.GetSeries(seriesId);
        }

        public long CreateSeries(CreateSeriesDto dto) {
            return seriesRepository.CreateSeries(dto);
        }

        public Series CreateAndRetrieveSeries(CreateSeriesDto dto) {
            long libraryId = CreateSeries(dto);
            return new Series(libraryId, dto);
        }

        public async Task DeleteSeries(Series series) {
            await seriesRepository.DeleteSeries(series.SeriesId);
        }

        public void UpdateSeries(Series series) {
            seriesRepository.UpdateSeries(series);
        }

        public async Task RestoreSeries(Series series) {
            await seriesRepository.RestoreDeletedSeries(series.SeriesId);
        }

        public async Task PermanentlyRemoveSeries(Series series) {
            await seriesRepository.PermanentlyRemoveSeries(series.SeriesId);
        }

        public async Task<List<SeriesBasicDetails>> GetAllActiveSeriesInLibrary(long libraryId) {
            var allSeries = await seriesRepository.GetAllActiveSeriesInLibrary(libraryId);
            allSeries.Insert(0, GetDefaultSeriesBasicDetails());
            return allSeries;
        }

        public async Task<Series> GetVideoSeries(Video video) {
            return await seriesRepository.GetVideoSeries(video);
        }

        public async Task<SeriesBasicDetails> GetSeriesBasicDetails(long seriesId) {
            if (seriesId == DatabaseConstants.DEFAULT_ID) {
                return GetDefaultSeriesBasicDetails();
            }

            return await seriesRepository.GetSeriesBasicDetails(seriesId);
        }

        public async Task<PaginationResult<Series>> GetSeriesCharacterIsIn(long characterId, Pagination pagination) {
            return await seriesRepository.GetSeriesCharacterIsIn(characterId, pagination);
        }

        public async Task<PaginationResult<Series>> GetSeriesFeaturingCreator(long creatorId, Pagination pagination) {
            return await seriesRepository.GetSeriesFeaturingCreator(creatorId, pagination);
        }

        public async Task<int> GetNumberOfSeriesByPublisher(long publisherId) {
            return await seriesRepository.GetNumberOfSeriesByPublisher(publisherId);
        }

        public async Task<List<Series>> GetSeriesCalendarIsUsedIn(long calendarId) {
            return await seriesRepository.GetSeriesCalendarIsUsedIn(calendarId);
        }

        public async Task<int> GetPercentageOfVideosWatchedInSeries(long seriesId) {
            return await seriesRepository.GetPercentageOfVideosWatchedInSeries(seriesId);
        }

        public async Task<List<Series>> GetRecentlyWatchedSeriesInLibrary(long libraryId, int count) {
            return await seriesRepository.GetRecentlyWatchedSeriesInLibrary(libraryId, count);
        }

        public async Task<SeriesRatingDto> GetSeriesRating(long seriesId) {
            return await seriesRepository.GetSeriesRating(seriesId);
        }

        public async Task<PaginationResult<Series>> SearchForSeries(Pagination pagination, long libraryId, List<ISeriesSearchQueryGenerator> subqueryGenerators) {
            return await seriesRepository.SearchForSeries(pagination, libraryId, subqueryGenerators);
        }

        public async Task<PaginationResult<Series>> GetSeriesToWatchInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await seriesRepository.GetSeriesToWatchInLibrary(pagination, libraryId, nameFilter);
        }
    }
}

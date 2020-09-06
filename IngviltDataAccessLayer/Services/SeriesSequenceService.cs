using Ingvilt.Core;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories.Sequences;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class SeriesSequenceService {
        private SeriesSequenceRepository sequencesRepository;

        public SeriesSequenceService() {
            sequencesRepository = DependencyInjectionContainer.Container.Resolve<SeriesSequenceRepository>();
        }

        public async Task<PaginationResult<VideoSequence>> GetSeriesSequences(Pagination pagination) {
            return await sequencesRepository.GetVideoSequences(pagination);
        }

        public async Task<PaginationResult<VideoSequence>> GetDeletedSeriesSequences(Pagination pagination) {
            return await sequencesRepository.GetDeletedVideoSequences(pagination);
        }

        public async Task<PaginationResult<SeriesSequence>> GetSeriesSequencesInSeries(Pagination pagination, long seriesId) {
            return await sequencesRepository.GetVideoSequencesInSeries(seriesId, pagination);
        }

        public async Task<PaginationResult<SeriesSequence>> GetDeletedSeriesSequencesInSeries(Pagination pagination, long seriesId) {
            return await sequencesRepository.GetDeletedVideoSequencesInSeries(seriesId, pagination);
        }

        public long CreateSeriesSequence(CreateSeriesSequenceDto dto) {
            return sequencesRepository.CreateVideoSequence(dto);
        }

        public SeriesSequence CreateAndRetrieveSeriesSequence(CreateSeriesSequenceDto dto) {
            var sequencesId = CreateSeriesSequence(dto);
            return new SeriesSequence(sequencesId, dto);
        }

        public async Task DeleteSeriesSequence(VideoSequence sequences) {
            sequencesRepository.DeleteVideoSequence(sequences.SequenceId);
        }

        public void UpdateSeriesSequence(VideoSequence sequences) {
            sequencesRepository.UpdateVideoSequence(sequences);
        }

        public async Task RestoreSeriesSequence(VideoSequence sequences) {
            sequencesRepository.RestoreDeletedVideoSequence(sequences.SequenceId);
        }

        public async Task PermanentlyRemoveSeriesSequence(VideoSequence sequences) {
            sequencesRepository.PermanentlyRemoveVideoSequence(sequences.SequenceId);
        }

        public async Task<VideoSeriesChronologyDetails> GetVideoSeriesChronologyDetails(long videoId) {
            return sequencesRepository.GetVideoSeriesChronologyDetails(videoId);
        }

        public async Task<List<int>> GetSeasonNumbersInSeries(long seriesId) {
            return sequencesRepository.GetSeasonNumbersInSeries(seriesId);
        }

        public async Task RemoveVideoFromSequence(long videoId, long sequenceId) {
            sequencesRepository.RemoveVideoFromSequence(videoId, sequenceId);
        }

        public async Task AddVideosToSequence(List<long> videoIds, long sequenceId) {
            sequencesRepository.AddVideosToSequence(videoIds, sequenceId);
        }

        public async Task ReorderVideosInSequence(List<VideoInSequenceOrder> videos, long sequenceId) {
            sequencesRepository.ReorderVideosInSequence(videos, sequenceId);
        }

        public int GetPercentageOfVideosWatchedInSequence(long sequenceId) {
            return sequencesRepository.GetPercentageOfVideosWatchedInSequence(sequenceId);
        }
    }
}

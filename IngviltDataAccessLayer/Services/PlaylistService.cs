using Ingvilt.Core;
using Ingvilt.Dto.Sequences;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories.Sequences;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class PlaylistService {
        private PlaylistRepository playlistRepository;

        public PlaylistService() {
            playlistRepository = DependencyInjectionContainer.Container.Resolve<PlaylistRepository>();
        }

        public async Task<PaginationResult<PlaylistDto>> GetPlaylists(Pagination pagination, string nameFilter) {
            return await playlistRepository.GetPlaylists(pagination, nameFilter);
        }

        public async Task<PaginationResult<PlaylistDto>> GetDeletedPlaylists(Pagination pagination, string nameFilter) {
            return await playlistRepository.GetDeletedPlaylists(pagination, nameFilter);
        }

        public async Task<PaginationResult<PlaylistDto>> GetPlaylistsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await playlistRepository.GetPlaylistsInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<PlaylistDto>> GetDeletedPlaylistsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await playlistRepository.GetDeletedPlaylistsInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PlaylistDto> GetPlaylist(long playlistId) {
            return playlistRepository.GetPlaylist(playlistId);
        }

        public long CreatePlaylist(CreatePlaylistDto dto) {
            return playlistRepository.CreateVideoSequence(dto);
        }

        public PlaylistDto CreateAndRetrievePlaylist(CreatePlaylistDto dto) {
            var playlistId = CreatePlaylist(dto);
            return new PlaylistDto(playlistId, dto);
        }

        public async Task DeletePlaylist(VideoSequence playlist) {
            playlistRepository.DeleteVideoSequence(playlist.SequenceId);
        }

        public void UpdatePlaylist(VideoSequence playlist) {
            playlistRepository.UpdateVideoSequence(playlist);
        }

        public async Task RestorePlaylist(VideoSequence playlist) {
            playlistRepository.RestoreDeletedVideoSequence(playlist.SequenceId);
        }

        public async Task PermanentlyRemovePlaylist(VideoSequence playlist) {
            playlistRepository.PermanentlyRemoveVideoSequence(playlist.SequenceId);
        }

        public async Task<List<PlaylistDto>> GetAllPlaylistsContainingVideo(long videoId) {
            return playlistRepository.GetAllPlaylistsContainingVideo(videoId);
        }

        public async Task RemoveVideoFromSequence(long videoId, long sequenceId) {
            playlistRepository.RemoveVideoFromSequence(videoId, sequenceId);
        }

        public async Task AddVideosToSequence(List<long> videoIds, long sequenceId) {
            playlistRepository.AddVideosToSequence(videoIds, sequenceId);
        }

        public async Task ReorderVideosInSequence(List<VideoInSequenceOrder> videos, long sequenceId) {
            playlistRepository.ReorderVideosInSequence(videos, sequenceId);
        }

        public int GetPercentageOfVideosWatchedInSequence(long sequenceId) {
            return playlistRepository.GetPercentageOfVideosWatchedInSequence(sequenceId);
        }
    }
}

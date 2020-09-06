using Ingvilt.Constants;
using Ingvilt.Core;
using Ingvilt.Dto;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Libraries;
using Ingvilt.Repositories;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Util;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class ImportLibraryService {
        private LibraryRepository libraryRepository;
        private VideoRepository videoRepository;
        private CharacterRepository characterRepository;
        private SeriesRepository seriesRepository;
        private PublisherRepository publisherRepository;
        private CalendarRepository calendarRepository;
        private LocationRepository locationRepository;
        private PlaylistRepository playlistRepository;
        private TagRepository tagRepository;
        private MediaFileRepository mediaFileRepository;
        private SeriesSequenceRepository seriesSequenceRepository;

        private Dictionary<string, long> ids;
        private long libraryId;

        private void InitRepositories() {
            libraryRepository = DependencyInjectionContainer.Container.Resolve<LibraryRepository>();
            videoRepository = DependencyInjectionContainer.Container.Resolve<VideoRepository>();
            characterRepository = DependencyInjectionContainer.Container.Resolve<CharacterRepository>();
            seriesRepository = DependencyInjectionContainer.Container.Resolve<SeriesRepository>();
            publisherRepository = DependencyInjectionContainer.Container.Resolve<PublisherRepository>();
            calendarRepository = DependencyInjectionContainer.Container.Resolve<CalendarRepository>();
            locationRepository = DependencyInjectionContainer.Container.Resolve<LocationRepository>();
            playlistRepository = DependencyInjectionContainer.Container.Resolve<PlaylistRepository>();
            tagRepository = DependencyInjectionContainer.Container.Resolve<TagRepository>();
            mediaFileRepository = DependencyInjectionContainer.Container.Resolve<MediaFileRepository>();
            seriesSequenceRepository = DependencyInjectionContainer.Container.Resolve<SeriesSequenceRepository>();
        }

        public ImportLibraryService() {
            InitRepositories();
            ids = new Dictionary<string, long>();
        }

        private long UpsertLibrary(ExportedLibraryDto dto) {
            var fileId = mediaFileRepository.GetMediaFileLongId_FromGUID(dto.CoverFileId);
            var libraryDto = new CreateLibraryDto(dto.Name, fileId);
            return libraryRepository.UpsertLibrary(libraryDto, dto.UniqueId, dto.Settings);
        }

        private async Task UpsertFiles(ExportedLibraryDto dto) {
            await mediaFileRepository.UpsertMediaFiles(dto.Files, ids);
        }

        private void UpsertTags(ExportedLibraryDto dto) {
            tagRepository.UpsertCharacterTags(dto.CharacterTags);
            tagRepository.UpsertVideoTags(dto.VideoTags);
        }

        private long GetIdFromGuid(string guid) {
            if (guid == null) {
                return DatabaseConstants.DEFAULT_ID;
            }

            return ids[guid];
        }

        private long GetFileId(string fileGuid) {
            return GetIdFromGuid(fileGuid);
        }

        private long GetPublisherId(string publisherGuid) {
            return GetIdFromGuid(publisherGuid);
        }

        private long GetCalendarId(string guid) {
            return GetIdFromGuid(guid);
        }

        private long GetSeriesId(string guid) {
            return GetIdFromGuid(guid);
        }

        private async Task UpsertPublishers(ExportedLibraryDto dto) {
            foreach (var publisher in dto.Publishers) {
                publisher.Details.LibraryId = libraryId;
                publisher.Details.LogoFileId = GetFileId(publisher.FileId);
            }

            await publisherRepository.UpsertPublishers(dto.Publishers, ids);
        }

        private async Task UpsertCalendars(ExportedLibraryDto dto) {
            foreach (var calendar in dto.Calendars) {
                calendar.LibraryId = libraryId;
            }

            await calendarRepository.UpsertCalendars(dto.Calendars, ids);
        }

        private async Task UpsertLocations(ExportedLibraryDto dto) {
            foreach (var location in dto.Locations) {
                location.Details.LibraryId = libraryId;
                location.Details.CoverFileId = GetFileId(location.FileId);
                location.Details.PublisherId = GetPublisherId(location.PublisherId);
            }

            await locationRepository.UpsertLocations(dto.Locations, ids);
        }

        private async Task UpsertSeries(ExportedLibraryDto dto) {
            foreach (var series in dto.Series) {
                series.Details.LibraryId = libraryId;
                series.Details.LogoFileId = GetFileId(series.FileId);
                series.Details.PublisherId = GetPublisherId(series.PublisherId);
                series.Details.CalendarId = GetCalendarId(series.CalendarId);
            }

            await seriesRepository.UpsertSeries(dto.Series, ids);
        }

        private async Task UpsertVideos(ExportedLibraryDto dto) {
            foreach (var video in dto.Videos) {
                video.Details.LibraryId = libraryId;
                video.Details.CoverFileId = GetFileId(video.FileId);
                video.Details.PublisherId = GetPublisherId(video.PublisherId);
                video.Details.SeriesId = GetSeriesId(video.SeriesId);
            }

            await videoRepository.UpsertVideos(dto.Videos, ids);
        }

        private async Task UpsertCharacters(List<ExportedCharacterSimpleDto> list) {
            foreach (var character in list) {
                character.Details.LibraryId = libraryId;
                character.Details.CoverMediaId = GetFileId(character.FileId);
                character.Details.CalendarId = GetCalendarId(character.CalendarId);
            }

            await characterRepository.UpsertCharacters(list, ids);
        }

        private async Task UpsertSeriesSequences(ExportedLibraryDto dto) {
            foreach (var sequence in dto.SeriesSequences) {
                sequence.Details.CoverFile = GetFileId(sequence.FileId);
                sequence.Details.SeriesId = GetSeriesId(sequence.SeriesId);
            }

            await seriesSequenceRepository.UpsertSeriesSequences(dto.SeriesSequences, ids);
        }

        private async Task UpsertPlaylists(ExportedLibraryDto dto) {
            foreach (var sequence in dto.Playlists) {
                sequence.Details.CoverFile = GetFileId(sequence.FileId);
                sequence.Details.LibraryId = libraryId;
            }

            await playlistRepository.UpsertPlaylists(dto.Playlists, ids);
        }

        private async Task ImportEntities(ExportedLibraryDto dto) {
            await UpsertPublishers(dto);
            await UpsertCalendars(dto);
            await UpsertLocations(dto);
            await UpsertSeries(dto);
            await UpsertVideos(dto);
            await UpsertCharacters(dto.Characters);
            await UpsertCharacters(dto.Creators);
            await UpsertSeriesSequences(dto);
            await UpsertPlaylists(dto);
        }

        private async Task ImportRelationships(ExportedLibraryDto dto) {
            await videoRepository.UpsertCharactersInVideos(dto.CharactersInVideos, ids);
            await videoRepository.UpsertCreatorsOfVideos(dto.VideoCreators, ids);

            mediaFileRepository.UpsertFilesOnCharacters(dto.FilesOnCharacters, ids);
            mediaFileRepository.UpsertFilesOnLocations(dto.FilesOnLocations, ids);
            mediaFileRepository.UpsertFilesOnVideos(dto.FilesOnVideos, ids);

            tagRepository.UpsertTagsOnCharacters(dto.TagsOnCharacters, ids);
            tagRepository.UpsertTagsOnFiles(dto.TagsOnFiles, ids);
            tagRepository.UpsertTagsOnSeries(dto.TagsOnSeries, ids);
            tagRepository.UpsertTagsOnVideos(dto.TagsOnVideos, ids);

            seriesSequenceRepository.UpsertVideosInSequences(dto.VideosInSeriesSequences, ids);
            playlistRepository.UpsertVideosInSequences(dto.VideosInPlaylists, ids);

            await locationRepository.UpsertVideosAtLocations(dto.VideosOnLocations, ids);
        }

        public async Task ImportLibrary(ExportedLibraryDto dto) {
            var resetConnection = !DataAccessUtil.UsePersistentConnection;
            DataAccessUtil.UsePersistentConnection = true;

            await UpsertFiles(dto);
            UpsertTags(dto);

            libraryId = UpsertLibrary(dto);
            await ImportEntities(dto);
            await ImportRelationships(dto);

            if (resetConnection) {
                DataAccessUtil.ClosePersistentConnection();
                DataAccessUtil.UsePersistentConnection = false;
            }
        }
    }
}

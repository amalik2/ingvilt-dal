using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Publishers;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Videos;
using System.Collections.Generic;

namespace Ingvilt.Dto.Libraries {
    public class ExportedLibraryDto {
        public string Name;
        public string UniqueId;
        public string CoverFileId;

        public List<ExportedVideoSimpleDto> Videos;
        public List<ExportedCharacterSimpleDto> Characters;
        public List<ExportedCharacterSimpleDto> Creators;
        public List<ExportedSeriesSimpleDto> Series;
        public List<ExportedPublisherSimpleDto> Publishers;
        public List<ExportedLocationSimpleDto> Locations;
        public List<Calendar> Calendars;
        public List<ExportedPlaylistSimpleDto> Playlists;
        public List<Tag> CharacterTags;
        public List<Tag> VideoTags;
        public List<MediaFile> Files;
        public List<ExportedSeriesSequenceSimpleDto> SeriesSequences;

        public List<SeriesTagExportDto> TagsOnSeries;
        public List<VideoTagExportDto> TagsOnVideos;
        public List<FileTagExportDto> TagsOnFiles;
        public List<CharacterTagExportDto> TagsOnCharacters;
        public List<VideoMediaFilesExportDto> FilesOnVideos;
        public List<FileLocationExportDto> FilesOnLocations;
        public List<CharacterMediaFilesExportDto> FilesOnCharacters;
        public List<VideosInSequencesExportDto> VideosInSeriesSequences;
        public List<VideosInSequencesExportDto> VideosInPlaylists;
        public List<VideoLocationExportDto> VideosOnLocations;
        public List<VideoCharacterActorExportDto> CharactersInVideos;
        public List<VideoCreatorExportDto> VideoCreators;

        public LibrarySettings Settings;

        public ExportedLibraryDto(string name, string uniqueId, string coverFileId, List<ExportedVideoSimpleDto> videos, List<ExportedCharacterSimpleDto> characters, List<ExportedCharacterSimpleDto> creators, List<ExportedSeriesSimpleDto> series, List<ExportedPublisherSimpleDto> publishers, List<ExportedLocationSimpleDto> locations, List<Calendar> calendars, List<ExportedPlaylistSimpleDto> playlists, List<Tag> characterTags, List<Tag> videoTags, List<MediaFile> files, List<ExportedSeriesSequenceSimpleDto> seriesSequences, List<SeriesTagExportDto> tagsOnSeries, List<VideoTagExportDto> tagsOnVideos, List<FileTagExportDto> tagsOnFiles, List<CharacterTagExportDto> tagsOnCharacters, List<VideoMediaFilesExportDto> filesOnVideos, List<FileLocationExportDto> filesOnLocations, List<CharacterMediaFilesExportDto> filesOnCharacters, List<VideosInSequencesExportDto> videosInSeriesSequences, List<VideosInSequencesExportDto> videosInPlaylists, List<VideoLocationExportDto> videosOnLocations, List<VideoCharacterActorExportDto> charactersInVideos, List<VideoCreatorExportDto> videoCreators, LibrarySettings settings) {
            Name = name;
            UniqueId = uniqueId;
            CoverFileId = coverFileId;
            Videos = videos;
            Characters = characters;
            Creators = creators;
            Series = series;
            Publishers = publishers;
            Locations = locations;
            Calendars = calendars;
            Playlists = playlists;
            CharacterTags = characterTags;
            VideoTags = videoTags;
            Files = files;
            SeriesSequences = seriesSequences;
            TagsOnSeries = tagsOnSeries;
            TagsOnVideos = tagsOnVideos;
            TagsOnFiles = tagsOnFiles;
            TagsOnCharacters = tagsOnCharacters;
            FilesOnVideos = filesOnVideos;
            FilesOnLocations = filesOnLocations;
            FilesOnCharacters = filesOnCharacters;
            VideosInSeriesSequences = videosInSeriesSequences;
            VideosInPlaylists = videosInPlaylists;
            VideosOnLocations = videosOnLocations;
            CharactersInVideos = charactersInVideos;
            VideoCreators = videoCreators;
            Settings = settings;
        }
    }
}

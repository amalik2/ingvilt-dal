using System;
using System.Collections.Generic;
using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Libraries;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Publishers;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Repositories.Sequences;
using Ingvilt.Services;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class LibraryImportTests : BaseTest {
        private LibraryRepository repository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();

        [TestMethod]
        public void ImportLibrary_WithFile() {
            var publishers = new List<ExportedPublisherSimpleDto>();
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var libraryName = "lib";
            var libraryGUID = UniqueIdUtil.GenerateUniqueId();

            var dto = new ExportedLibraryDto(
                libraryName, libraryGUID, files[0].UniqueId,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                new List<ExportedSeriesSimpleDto>(), publishers, new List<ExportedLocationSimpleDto>(),
                new List<Ingvilt.Dto.Calendars.Calendar>(), new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);

            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination()).Result.Results;
            foreach (var file in files) {
                foreach (var retFile in retFiles) {
                    if (file.UniqueId == retFile.UniqueId) {
                        file.MediaId = retFile.MediaId;
                        break;
                    }
                }
            }

            var libraries = repository.GetLibraries(new InfinitePagination(), "").Result.Results;
            var library = libraries[0];
            var expectedLibrary = new Library(library.LibraryId, libraryName, files[0].MediaId, false, libraryGUID);


            Assert.IsTrue(retFiles.Count == 2);
            Assert.AreEqual(1, libraries.Count);
            Assert.AreEqual(expectedLibrary, library);
            Assert.AreNotSame(DatabaseConstants.DEFAULT_ID, library.BackgroundImageId);
        }

        [TestMethod]
        public void ImportLibrary_Publisher_ShouldReferenceFile() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> {
                new ExportedPublisherSimpleDto(
                    new Publisher(-1, "p1", "", -1, "", -1, false, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId
                ),
                new ExportedPublisherSimpleDto(
                    new Publisher(-1, "p2", "", -1, "", -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                new List<ExportedSeriesSimpleDto>(), publishers, new List<ExportedLocationSimpleDto>(),
                new List<Ingvilt.Dto.Calendars.Calendar>(), new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var pubs = new PublisherRepository().GetPublishers(new InfinitePagination()).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;

            var expectedPubs = new List<Publisher> {
                publishers[0].Details,
                publishers[1].Details
            };
            expectedPubs[0].LibraryId = library.LibraryId;
            expectedPubs[1].LibraryId = library.LibraryId;
            expectedPubs[0].LogoFileId = retFiles[0].MediaId;

            CollectionAssert.AreEquivalent(expectedPubs, pubs);
        }

        [TestMethod]
        public void ImportLibrary_Location_ShouldReferenceFile() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto>();
            var locations = new List<ExportedLocationSimpleDto> {
                new ExportedLocationSimpleDto(
                    new Location(-1, "l1", "d", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()), files[0].UniqueId, null
                ),
                new ExportedLocationSimpleDto(
                    new Location(-1, "l2", "d", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()), null, null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                new List<ExportedSeriesSimpleDto>(), publishers, locations,
                new List<Ingvilt.Dto.Calendars.Calendar>(), new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var locs = new LocationRepository().GetLocations(new InfinitePagination()).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;

            var expectedLocs = new List<Location> {
                locations[0].Details,
                locations[1].Details
            };
            expectedLocs[0].LibraryId = library.LibraryId;
            expectedLocs[1].LibraryId = library.LibraryId;
            expectedLocs[0].CoverFileId = retFiles[0].MediaId;

            CollectionAssert.AreEquivalent(expectedLocs, locs);
        }

        [TestMethod]
        public void ImportLibrary_Location_ShouldReferencePublisher() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> {
                new ExportedPublisherSimpleDto(
                    new Publisher(-1, "p1", "", -1, "", -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null
                )
            };
            var locations = new List<ExportedLocationSimpleDto> {
                new ExportedLocationSimpleDto(
                    new Location(-1, "l1", "d", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()), null, publishers[0].Details.UniqueId
                ),
                new ExportedLocationSimpleDto(
                    new Location(-1, "l2", "d", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()), null, null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                new List<ExportedSeriesSimpleDto>(), publishers, locations,
                new List<Ingvilt.Dto.Calendars.Calendar>(), new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var locs = new LocationRepository().GetLocations(new InfinitePagination()).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retPubs = new PublisherRepository().GetPublishers(new InfinitePagination()).Result.Results;

            var expectedLocs = new List<Location> {
                locations[0].Details,
                locations[1].Details
            };
            expectedLocs[0].LibraryId = library.LibraryId;
            expectedLocs[1].LibraryId = library.LibraryId;
            expectedLocs[0].PublisherId = retPubs[0].PublisherId;

            CollectionAssert.AreEquivalent(expectedLocs, locs);
        }

        [TestMethod]
        public void ImportLibrary_Series_ShouldReference_FilePublisherCalendar() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> {
                new ExportedPublisherSimpleDto(
                    new Publisher(-1, "p1", "", -1, "", -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null
                )
            };
            var calendars = new List<Calendar> {
                new Calendar(-1, "", "", "", -1, false, UniqueIdUtil.GenerateUniqueId())
            };
            var series = new List<ExportedSeriesSimpleDto> {
                new ExportedSeriesSimpleDto(
                    new Series(-1, "s1", "", -1, "", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId, publishers[0].Details.UniqueId, calendars[0].UniqueId
                ),
                new ExportedSeriesSimpleDto(
                    new Series(-1, "s1", "", -1, "", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null, null, null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                series, publishers, new List<ExportedLocationSimpleDto>(),
                calendars, new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var seriesList = new SeriesRepository().GetSeries(new InfinitePagination()).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;
            var retPublishers = new PublisherRepository().GetPublishers(new InfinitePagination()).Result.Results;
            var retCalendars = new CalendarRepository().GetCalendars(new InfinitePagination()).Result.Results;

            var expectedSeries = new List<Series> {
                series[0].Details,
                series[1].Details
            };
            expectedSeries[0].LibraryId = library.LibraryId;
            expectedSeries[1].LibraryId = library.LibraryId;
            expectedSeries[0].LogoFileId = retFiles[0].MediaId;
            expectedSeries[0].PublisherId = retPublishers[0].PublisherId;
            expectedSeries[0].CalendarId = retCalendars[0].CalendarId;


            CollectionAssert.AreEquivalent(expectedSeries, seriesList);
        }

        [TestMethod]
        public void ImportLibrary_Video_ShouldReference_FilePublisherSeries() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> {
                new ExportedPublisherSimpleDto(
                    new Publisher(-1, "p1", "", -1, "", -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null
                )
            };
            var series = new List<ExportedSeriesSimpleDto> {
                new ExportedSeriesSimpleDto(
                    new Series(-1, "s1", "", -1, "", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null, null, null
                )
            };
            var videos = new List<ExportedVideoSimpleDto> {
                new ExportedVideoSimpleDto(
                    new Video(-1, "v1", 0, null, null, 0, 0, "", "", "", "", -1, VideoWatchStatus.NEED_TO_WATCH, null, -1, -1, null, null, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId, publishers[0].Details.UniqueId, series[0].Details.UniqueId
                ),
                new ExportedVideoSimpleDto(
                    new Video(-1, "v2", 0, null, null, 0, 0, "", "", "", "", -1, VideoWatchStatus.NEED_TO_WATCH, null, -1, -1, null, null, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null, null, null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                videos, new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                series, publishers, new List<ExportedLocationSimpleDto>(),
                new List<Calendar>(), new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var videosList = new VideoRepository().GetVideos(new InfinitePagination()).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;
            var retPublishers = new PublisherRepository().GetPublishers(new InfinitePagination()).Result.Results;
            var retSeries = new SeriesRepository().GetSeries(new InfinitePagination()).Result.Results;

            var expectedVideos = new List<Video> {
                videos[0].Details,
                videos[1].Details
            };
            expectedVideos[0].LibraryId = library.LibraryId;
            expectedVideos[1].LibraryId = library.LibraryId;
            expectedVideos[0].CoverFileId = retFiles[0].MediaId;
            expectedVideos[0].PublisherId = retPublishers[0].PublisherId;
            expectedVideos[0].SeriesId = retSeries[0].SeriesId;


            CollectionAssert.AreEquivalent(expectedVideos, videosList);
        }

        private void ImportLibrary_Character_ShouldReference_FileCalendar(bool creator) {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> { };
            var series = new List<ExportedSeriesSimpleDto> { };
            var videos = new List<ExportedVideoSimpleDto> { };
            var calendars = new List<Calendar> {
                new Calendar(-1, "", "", "", -1, false, UniqueIdUtil.GenerateUniqueId())
            };
            var characters = new List<ExportedCharacterSimpleDto> {
                new ExportedCharacterSimpleDto(
                    new Character(-1, "c1", "", null, null, null, 0, -1, -1, -1, false, creator, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId, calendars[0].UniqueId
                ),
                new ExportedCharacterSimpleDto(
                    new Character(-1, "c1", "", null, null, null, 0, -1, -1, -1, false, creator, UniqueIdUtil.GenerateUniqueId()),
                    null, null
                )
            };

            var emptyCharacters = new List<ExportedCharacterSimpleDto>();

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                videos, creator ? emptyCharacters : characters, creator ? characters : emptyCharacters,
                series, publishers, new List<ExportedLocationSimpleDto>(),
                calendars, new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var charactersList = new CharacterRepository().GetCharacters(new InfinitePagination(), creator).Result.Results;
            var library = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0];
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;
            var retCalendars = new CalendarRepository().GetCalendars(new InfinitePagination()).Result.Results;

            var expectedChars = new List<Character> {
                characters[0].Details,
                characters[1].Details
            };
            expectedChars[0].LibraryId = library.LibraryId;
            expectedChars[1].LibraryId = library.LibraryId;
            expectedChars[0].CoverMediaId = retFiles[0].MediaId;
            expectedChars[0].CalendarId = retCalendars[0].CalendarId;

            CollectionAssert.AreEquivalent(expectedChars, charactersList);
        }

        [TestMethod]
        public void ImportLibrary_Character_ShouldReference_FileCalendar() {
            ImportLibrary_Character_ShouldReference_FileCalendar(false);
        }

        [TestMethod]
        public void ImportLibrary_Creator_ShouldReference_FileCalendar() {
            ImportLibrary_Character_ShouldReference_FileCalendar(true);
        }

        [TestMethod]
        public void ImportLibrary_SeriesSequence_ShouldReference_FileSeries() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> { };
            var calendars = new List<Calendar> { };
            var series = new List<ExportedSeriesSimpleDto> {
                new ExportedSeriesSimpleDto(
                    new Series(-1, "s1", "", -1, "", -1, -1, -1, false, UniqueIdUtil.GenerateUniqueId()),
                    null, null, null
                )
            };
            var sequences = new List<ExportedSeriesSequenceSimpleDto> {
                new ExportedSeriesSequenceSimpleDto(
                    new SeriesSequence(-1, "s1", "", -1, false, false, 0, -1, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId, series[0].Details.UniqueId
                ),
                new ExportedSeriesSequenceSimpleDto(
                    new SeriesSequence(-1, "s1", "", -1, false, false, 0, -1, UniqueIdUtil.GenerateUniqueId()),
                    null, series[0].Details.UniqueId
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                series, publishers, new List<ExportedLocationSimpleDto>(),
                calendars, new List<ExportedPlaylistSimpleDto>(),
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                sequences, new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var libraryId = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0].LibraryId;
            var sequencesList = new SeriesSequenceRepository().GetAllSequencesInAllSeries(new InfinitePagination(), libraryId).Result.Results;
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;
            var retSeries = new SeriesRepository().GetSeries(new InfinitePagination()).Result.Results;

            var expectedSequences = new List<SeriesSequence> {
                sequences[0].Details,
                sequences[1].Details
            };
            expectedSequences[0].SeriesId = retSeries[0].SeriesId;
            expectedSequences[1].SeriesId = retSeries[0].SeriesId;
            expectedSequences[0].CoverFile = retFiles[0].MediaId;


            CollectionAssert.AreEquivalent(expectedSequences, sequencesList);
        }

        [TestMethod]
        public void ImportLibrary_Playlist_ShouldReference_File() {
            var files = new List<MediaFile> {
                new MediaFile(-1, "https://google.ca", MediaFileType.IMAGE_TYPE, "f1", DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                new MediaFile(-1, "https://google.com", MediaFileType.IMAGE_TYPE, "f2", DateTime.Now, UniqueIdUtil.GenerateUniqueId())
            };
            var publishers = new List<ExportedPublisherSimpleDto> { };
            var calendars = new List<Calendar> { };
            var series = new List<ExportedSeriesSimpleDto> { };
            var playlists = new List<ExportedPlaylistSimpleDto> {
                new ExportedPlaylistSimpleDto(
                    new PlaylistDto(-1, "p1", "", -1, false, -1, DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                    files[0].UniqueId
                ),
                new ExportedPlaylistSimpleDto(
                    new PlaylistDto(-1, "p1", "", -1, false, -1, DateTime.Now, UniqueIdUtil.GenerateUniqueId()),
                    null
                )
            };

            var dto = new ExportedLibraryDto(
                "lib", UniqueIdUtil.GenerateUniqueId(), null,
                new List<ExportedVideoSimpleDto>(), new List<ExportedCharacterSimpleDto>(), new List<ExportedCharacterSimpleDto>(),
                series, publishers, new List<ExportedLocationSimpleDto>(),
                calendars, playlists,
                new List<Ingvilt.Dto.Tags.Tag>(), new List<Ingvilt.Dto.Tags.Tag>(), files,
                new List<ExportedSeriesSequenceSimpleDto>(), new List<Ingvilt.Dto.Export.SeriesTagExportDto>(),
                new List<Ingvilt.Dto.Export.VideoTagExportDto>(), new List<Ingvilt.Dto.Export.FileTagExportDto>(),
                new List<Ingvilt.Dto.Export.CharacterTagExportDto>(), new List<Ingvilt.Dto.Export.VideoMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.FileLocationExportDto>(), new List<Ingvilt.Dto.Export.CharacterMediaFilesExportDto>(),
                new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(), new List<Ingvilt.Dto.Export.VideosInSequencesExportDto>(),
                new List<Ingvilt.Dto.Export.VideoLocationExportDto>(), new List<Ingvilt.Dto.Export.VideoCharacterActorExportDto>(),
                new List<Ingvilt.Dto.Export.VideoCreatorExportDto>(), new LibrarySettings()
            );

            new ImportLibraryService().ImportLibrary(dto);
            var libraryId = repository.GetLibraries(new InfinitePagination(), "").Result.Results[0].LibraryId;
            var sequencesList = new PlaylistRepository().GetPlaylistsInLibrary(libraryId, new InfinitePagination(), "").Result.Results;
            var retFiles = mediaFileRepository.GetMediaFiles(new InfinitePagination(), "f1").Result.Results;

            var expectedSequences = new List<PlaylistDto> {
                playlists[0].Details,
                playlists[1].Details
            };
            expectedSequences[0].LibraryId = libraryId;
            expectedSequences[1].LibraryId = libraryId;
            expectedSequences[0].CoverFile = retFiles[0].MediaId;


            CollectionAssert.AreEquivalent(expectedSequences, sequencesList);
        }
    }
}

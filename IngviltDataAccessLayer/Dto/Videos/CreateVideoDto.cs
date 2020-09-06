using Ingvilt.Dto.Characters;
using System;
using System.Collections.Generic;

namespace Ingvilt.Dto.Videos {
    public class CreateVideoDto {
        public string Title {
            get;
            set;
        }

        public int DurationInSeconds {
            get;
        }

        public double ExternalRating {
            get;
            set;
        }

        public double UserRating {
            get;
            set;
        }

        public string Description {
            get;
            set;
        }

        public string Notes {
            get;
        }

        public string SourceURL {
            get;
            set;
        }

        public string SiteURL {
            get;
        }

        public long SeriesId {
            get;
        }

        public VideoWatchStatus WatchStatus {
            get;
        }

        public long PublisherId {
            get;
        }

        public long LibraryId {
            get;
        }

        public Nullable<DateTime> ReleaseDate {
            get;
            set;
        }

        public Nullable<DateTime> TimelineDate {
            get;
            set;
        }

        public long CoverFileId {
            get;
            set;
        }

        public List<ActorForCharacterFullDto> Characters {
            get;
            set;
        }

        public int TimesWatched {
            get;
            set;
        }

        public CreateVideoDto(string title, int durationInSeconds, double externalRating, double userRating, string description, string notes, string sourceURL, string siteURL, long seriesId, VideoWatchStatus watchStatus, long publisherId, long libraryId, Nullable<DateTime> releaseDate, Nullable<DateTime> timelineDate, long coverFileId, List<ActorForCharacterFullDto> characters, int timesWatched = 0) {
            Title = title;
            DurationInSeconds = durationInSeconds;
            ExternalRating = externalRating;
            UserRating = userRating;
            Description = description;
            Notes = notes;
            SourceURL = sourceURL;
            SiteURL = siteURL;
            SeriesId = seriesId;
            WatchStatus = watchStatus;
            PublisherId = publisherId;
            LibraryId = libraryId;
            ReleaseDate = releaseDate;
            TimelineDate = timelineDate;
            CoverFileId = coverFileId;
            Characters = characters;
            TimesWatched = timesWatched;
        }
    }
}

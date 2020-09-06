using Ingvilt.Constants;

namespace Ingvilt.Dto.SeriesNS {
    public class CreateSeriesDto {
        public string Name {
            get;
        }

        public string SiteURL {
            get;
        }

        public long LogoFileId {
            get;
        }

        public string Description {
            get;
        }

        public long PublisherId {
            get;
        }

        public long LibraryId {
            get;
        }

        public long CalendarId {
            get;
        }

        public bool WorthWatching {
            get;
        }

        public CreateSeriesDto(string name, string siteUrl, long logoFileId, string description, long publisherId, long libraryId, long calendarId = DatabaseConstants.DEFAULT_ID, bool worthWatching = false) {
            Name = name;
            SiteURL = siteUrl;
            LogoFileId = logoFileId;
            Description = description;
            PublisherId = publisherId;
            LibraryId = libraryId;
            CalendarId = calendarId;
            WorthWatching = worthWatching;
        }
    }
}

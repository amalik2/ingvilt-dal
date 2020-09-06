using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using Newtonsoft.Json;

namespace Ingvilt.Dto.SeriesNS {
    public class Series : BaseObservableProperty, IAttachedToLibrary {
        private long logoFileId;
        private string name;
        private string siteUrl;
        private string description;
        private long publisherId;
        private long calendarId;
        private bool deleted;

        [JsonIgnore]
        public long SeriesId {
            get;
            set;
        }

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string SiteURL {
            get {
                return siteUrl;
            }
            set {
                siteUrl = value;
                OnPropertyChanged(nameof(SiteURL));
            }
        }

        [JsonIgnore]
        public long LogoFileId {
            get {
                return logoFileId;
            }
            set {
                logoFileId = value;
                OnPropertyChanged(nameof(LogoFileId));
            }
        }

        public string Description {
            get {
                return description;
            }
            set {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [JsonIgnore]
        public long PublisherId {
            get {
                return publisherId;
            }
            set {
                publisherId = value;
                OnPropertyChanged(nameof(PublisherId));
            }
        }

        [JsonIgnore]
        public long LibraryId {
            get;
            set;
        }

        [JsonIgnore]
        public long CalendarId {
            get {
                return calendarId;
            }
            set {
                calendarId = value;
                OnPropertyChanged(nameof(CalendarId));
            }
        }

        public bool Deleted {
            get {
                return deleted;
            }
            set {
                deleted = value;
                OnPropertyChanged(nameof(Deleted));
            }
        }

        public string UniqueId {
            get;
            set;
        }

        public bool WorthWatching {
            get;
            set;
        }

        public Series(long id, string name, string siteURL, long logoFileId, string description, long publisherId, long libraryId, long calendarId, bool deleted = false, string uniqueId = null, bool worthWatching = false) {
            SeriesId = id;
            Name = name;
            SiteURL = siteURL;
            LogoFileId = logoFileId;
            Description = description;
            PublisherId = publisherId;
            LibraryId = libraryId;
            CalendarId = calendarId;
            Deleted = deleted;
            UniqueId = uniqueId;
            WorthWatching = worthWatching;
        }

        public Series(long id, CreateSeriesDto dto) : this(id, dto.Name, dto.SiteURL, dto.LogoFileId, dto.Description, dto.PublisherId, dto.LibraryId, dto.CalendarId, false, null, dto.WorthWatching) {
        }

        private Series() {
        }

        public override bool Equals(object other) {
            var otherSeries = other as Series;
            if (otherSeries == null) {
                return false;
            }

            return otherSeries.SeriesId == SeriesId && otherSeries.CalendarId == CalendarId
                && otherSeries.Deleted == Deleted && otherSeries.Description == Description
                && otherSeries.LibraryId == LibraryId && otherSeries.LogoFileId == LogoFileId
                && otherSeries.Name == Name && otherSeries.PublisherId == PublisherId
                && otherSeries.SeriesId == SeriesId && otherSeries.SiteURL == SiteURL
                && otherSeries.UniqueId == UniqueId && otherSeries.WorthWatching == WorthWatching;
        }

        public override int GetHashCode() {
            return ((int)SeriesId << 2) ^ Name.GetHashCode();
        }

        public void SetLogoFile(MediaFile file) {
            LogoFileId = file.MediaId;
        }

        public void ClearLogoFile() {
            LogoFileId = DatabaseConstants.DEFAULT_ID;
        }

        public CreateSeriesDto GetCreateSeriesDto() {
            return new CreateSeriesDto(Name, SiteURL, LogoFileId, Description, PublisherId, LibraryId, CalendarId, WorthWatching);
        }
    }
}

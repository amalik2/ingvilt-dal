using Ingvilt.Constants;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ingvilt.Dto.Videos {
    public partial class Video : BaseObservableProperty, IAttachedToLibrary {
        
        private long coverFileId;
        
        private bool deleted;
        
        private string title;
        
        private int timesWatched;
        
        private Nullable<DateTime> lastWatchDate;
        
        private Nullable<int> durationInSeconds;
        
        private double externalRating;
        
        private double userRating;
        
        private string description;
        
        private string notes;
        
        private string sourceUrl;
        
        private string siteUrl;
        
        private VideoWatchStatus watchStatus;
        
        private Nullable<int> globalRank;
        
        private long publisherId;
        
        private long seriesId;
        
        private Nullable<DateTime> releaseDate;
        
        private Nullable<DateTime> timelineDate;

        [JsonIgnore]
        public long VideoId {
            get;
            set;
        }

        public string Title {
            get {
                return title;
            }
            set {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public int TimesWatched {
            get {
                return timesWatched;
            }
            set {
                timesWatched = value;
                OnPropertyChanged(nameof(TimesWatched));
            }
        }

        public Nullable<DateTime> LastWatchDate {
            get {
                return lastWatchDate;
            }
            set {
                lastWatchDate = value;
                OnPropertyChanged(nameof(LastWatchDate));
            }
        }

        public Nullable<int> DurationInSeconds {
            get {
                return durationInSeconds;
            }
            set {
                durationInSeconds = value;
                OnPropertyChanged(nameof(DurationInSeconds));
            }
        }

        public double ExternalRating {
            get {
                return externalRating;
            }
            set {
                externalRating = value;
                OnPropertyChanged(nameof(ExternalRating));
            }
        }

        public double UserRating {
            get {
                return userRating;
            }
            set {
                userRating = value;
                OnPropertyChanged(nameof(UserRating));
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

        public string Notes {
            get {
                return notes;
            }
            set {
                notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public string SourceURL {
            get {
                return sourceUrl;
            }
            set {
                sourceUrl = value;
                OnPropertyChanged(nameof(SourceURL));
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
        public long SeriesId {
            get {
                return seriesId;
            }
            set {
                seriesId = value;
                OnPropertyChanged(nameof(SeriesId));
            }
        }

        public VideoWatchStatus WatchStatus {
            get {
                return watchStatus;
            }
            set {
                watchStatus = value;
                OnPropertyChanged(nameof(WatchStatus));
            }
        }

        public Nullable<int> GlobalRank {
            get {
                return globalRank;
            }
            set {
                globalRank = value;
                OnPropertyChanged(nameof(GlobalRank));
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

        public Nullable<DateTime> ReleaseDate {
            get {
                return releaseDate;
            }
            set {
                releaseDate = value;
                OnPropertyChanged(nameof(ReleaseDate));
            }
        }

        public Nullable<DateTime> TimelineDate {
            get {
                return timelineDate;
            }
            set {
                timelineDate = value;
                OnPropertyChanged(nameof(TimelineDate));
            }
        }

        [JsonIgnore]
        public long CoverFileId {
            get {
                return coverFileId;
            }
            set {
                coverFileId = value;
                OnPropertyChanged(nameof(CoverFileId));
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

        public Video(long id, string title, int timesWatched, Nullable<DateTime> lastWatchDate, Nullable<int> durationInSeconds, double externalRating, double userRating, string description, string notes, string sourceURL, string siteURL, long seriesId, VideoWatchStatus watchStatus, Nullable<int> globalRank, long publisherId, long libraryId, Nullable<DateTime> releaseDate, Nullable<DateTime> timelineDate, long coverFileId, bool deleted, string uniqueId = null) {
            VideoId = id;
            Title = title;
            TimesWatched = timesWatched;
            LastWatchDate = lastWatchDate;
            DurationInSeconds = durationInSeconds;
            ExternalRating = externalRating;
            UserRating = userRating;
            Description = description;
            Notes = notes;
            SourceURL = sourceURL;
            SiteURL = siteURL;
            SeriesId = seriesId;
            WatchStatus = watchStatus;
            GlobalRank = globalRank;
            PublisherId = publisherId;
            LibraryId = libraryId;
            ReleaseDate = releaseDate;
            TimelineDate = timelineDate;
            CoverFileId = coverFileId;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        private Video() {
        }

        public override bool Equals(object other) {
            var otherCast = other as Video;
            if (otherCast == null) {
                return false;
            }

            return otherCast.VideoId == VideoId && otherCast.Title == Title;
        }

        public override int GetHashCode() {
            return ((int)VideoId << 2) ^ Title.GetHashCode();
        }

        public void SetCoverFile(MediaFile file) {
            CoverFileId = file.MediaId;
        }

        public void ClearCoverFile() {
            CoverFileId = DatabaseConstants.DEFAULT_ID;
        }

        public CreateVideoDto GetCreateVideoDto() {
            return new CreateVideoDto(Title, DurationInSeconds.GetValueOrDefault(0), ExternalRating, UserRating, Description, Notes, SourceURL, SiteURL, SeriesId, WatchStatus, PublisherId, LibraryId, ReleaseDate, TimelineDate, CoverFileId, new List<ActorForCharacterFullDto>(), TimesWatched);
        }

        public void WatchVideo(GlobalSettings settings) {
            if (settings.TrackTimesWatched && TimesWatched < VideoConstants.MAX_TIMES_WATCHED) {
                ++TimesWatched;
            }

            WatchStatus = VideoWatchStatus.WATCHED;

            if (settings.TrackLastWatchDate) {
                LastWatchDate = DateTime.Now;
            }
        }
    }
}

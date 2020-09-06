using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using Newtonsoft.Json;

namespace Ingvilt.Dto.Publishers {
    public class Publisher : BaseObservableProperty, IAttachedToLibrary {
        private long logoFileId;
        private string name;
        private string siteUrl;
        private string description;
        private bool deleted;

        [JsonIgnore]
        public long PublisherId {
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
        public long LibraryId {
            get;
            set;
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

        public Publisher(long publisherId, string name, string siteUrl, long logoFileId, string description, long libraryId, bool deleted = false, string uniqueId = null) {
            Name = name;
            SiteURL = siteUrl;
            LogoFileId = logoFileId;
            PublisherId = publisherId;
            Description = description;
            LibraryId = libraryId;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        public Publisher(long publisherId, CreatePublisherDto dto) : this(publisherId, dto.Name, dto.SiteURL, dto.LogoFileId, dto.Description, dto.LibraryId) {
        }

        private Publisher() {
        }

        public override bool Equals(object other) {
            var otherPublisher = other as Publisher;
            if (otherPublisher == null) {
                return false;
            }

            return otherPublisher.PublisherId == PublisherId && otherPublisher.Name == Name
                && otherPublisher.SiteURL == SiteURL && otherPublisher.Description == Description
                && otherPublisher.LibraryId == LibraryId && otherPublisher.Deleted == Deleted
                && otherPublisher.UniqueId == UniqueId;
        }

        public override int GetHashCode() {
            return ((int)PublisherId << 2) ^ Name.GetHashCode();
        }

        public void SetLogoFile(MediaFile file) {
            LogoFileId = file.MediaId;
        }

        public void ClearLogoFile() {
            LogoFileId = DatabaseConstants.DEFAULT_ID;
        }

        public PublisherBasicDetails GetBasicDetails() {
            return new PublisherBasicDetails(PublisherId, Name);
        }

        public CreatePublisherDto GetCreatePublisherDto() {
            return new CreatePublisherDto(Name, SiteURL, LogoFileId, Description, LibraryId);
        }
    }
}

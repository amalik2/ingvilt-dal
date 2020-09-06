using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using Newtonsoft.Json;

namespace Ingvilt.Dto.Locations {
    public class Location : BaseObservableProperty, IAttachedToLibrary {
        private long logoFileId;
        private string name;
        private string description;
        private bool deleted;

        [JsonIgnore]
        public long LocationId {
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

        [JsonIgnore]
        public long PublisherId {
            get;
            set;
        }

        [JsonIgnore]
        public long CoverFileId {
            get {
                return logoFileId;
            }
            set {
                logoFileId = value;
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

        public Location() {
        }

        public Location(long locationId, string name, string description, long libraryId, long publisherId, long coverFileId, bool deleted = false, string uniqueId = null) {
            LocationId = locationId;
            Name = name;
            Description = description;
            LibraryId = libraryId;
            PublisherId = publisherId;
            CoverFileId = coverFileId;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        public Location(long locationId, string name, string description, long coverFileId) : this(locationId, name, description, DatabaseConstants.DEFAULT_ID, DatabaseConstants.DEFAULT_ID, coverFileId) {
        }

        public Location(long locationId, CreateLocationDto dto) : this(locationId, dto.Name, dto.Description, dto.LibraryId, dto.PublisherId, dto.CoverFileId) {
        }

        public override bool Equals(object other) {
            var otherLocation = other as Location;
            if (otherLocation == null) {
                return false;
            }

            return otherLocation.LocationId == LocationId;
        }

        public override int GetHashCode() {
            return ((int)LocationId << 2) ^ Name.GetHashCode();
        }

        public void SetLogoFile(MediaFile file) {
            CoverFileId = file.MediaId;
        }

        public void ClearLogoFile() {
            CoverFileId = DatabaseConstants.DEFAULT_ID;
        }

        public CreateLocationDto GetCreateLocationDto() {
            return new CreateLocationDto(Name, Description, LibraryId, PublisherId, CoverFileId);
        }
    }
}

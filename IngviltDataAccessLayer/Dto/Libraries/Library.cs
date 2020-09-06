using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using System;

namespace Ingvilt.Dto {
    public class Library : BaseObservableProperty, IAttachedToLibrary {
        private long backgroundImageId;
        private string name;
        private bool deleted;

        public long LibraryId {
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

        public long BackgroundImageId {
            get {
                return backgroundImageId;
            }
            set {
                backgroundImageId = value;
                OnPropertyChanged(nameof(BackgroundImageId));
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
        }

        public Library(long libraryId, string name, long backgroundImageId, bool deleted = false, string uniqueId = null) {
            LibraryId = libraryId;
            Name = name;
            BackgroundImageId = backgroundImageId;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        public Library(long libraryId, CreateLibraryDto dto) : this(libraryId, dto.Name, dto.BackgroundImageId) {
        }

        public override bool Equals(Object other) {
            if ((other == null) || !this.GetType().Equals(other.GetType())) {
                return false;
            }

            var otherLibrary = other as Library;
            return otherLibrary.LibraryId == LibraryId && otherLibrary.Name == Name
                && otherLibrary.BackgroundImageId == BackgroundImageId && otherLibrary.Deleted == Deleted
                && otherLibrary.UniqueId == UniqueId;
        }

        public override int GetHashCode() {
            return ((int)LibraryId << 2) ^ Name.GetHashCode();
        }

        public void ClearBackgroundImage() {
            BackgroundImageId = DatabaseConstants.DEFAULT_ID;
        }

        public void SetBackgroundImage(MediaFile file) {
            BackgroundImageId = file.MediaId;
        }

        public LibraryBasicDetails GetBasicDetails() {
            return new LibraryBasicDetails(LibraryId, Name);
        }
    }
}

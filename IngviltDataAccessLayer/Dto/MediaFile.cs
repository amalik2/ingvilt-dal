using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Dto.Videos;
using Ingvilt.Models;
using Newtonsoft.Json;
using System;

namespace Ingvilt.Dto {
    public sealed class MediaFileType {
        public int Ordinal {
            get;
        }

        public static readonly MediaFileType IMAGE_TYPE = new MediaFileType(0);
        public static readonly MediaFileType VIDEO_TYPE = new MediaFileType(1);
        public static readonly MediaFileType URL_TYPE = new MediaFileType(2);

        public MediaFileType(int ordinal) {
            Ordinal = ordinal;
        }

        public override string ToString() {
            return Ordinal.ToString();
        }

        public static MediaFileType Parse(string value) {
            var intValue = int.Parse(value);
            if (intValue == 0) {
                return IMAGE_TYPE;
            } else if (intValue == 1) {
                return VIDEO_TYPE;
            } else if (intValue == 2) {
                return URL_TYPE;
            }

            throw new ArgumentOutOfRangeException("An invalid ordinal was specified");
        }

        public override bool Equals(object obj) {
            var otherType = obj as MediaFileType;
            return Ordinal == otherType.Ordinal;
        }

        public override int GetHashCode() {
            return Ordinal;
        }

        public string GetName() {
            if (Equals(IMAGE_TYPE)) {
                return "Image";
            } else if (Equals(VIDEO_TYPE)) {
                return "Video";
            }

            return "Url";
        }
    }

    public class MediaFile : BaseObservableProperty, IAttachedToLibrary {
        private string sourceUrl;
        private MediaFileType fileType;
        private string name;

        [JsonIgnore]
        public long MediaId {
            get;
            set;
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

        public MediaFileType FileType {
            get {
                return fileType;
            }
            set {
                fileType = value;
                OnPropertyChanged(nameof(FileType));
            }
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

        public DateTime CreationDate {
            get;
            set;
        }

        [JsonIgnore]
        public long LibraryId {
            get {
                return DatabaseConstants.DEFAULT_ID;
            }
            set {
                throw new NotImplementedException();
            }
        }

        public string UniqueId {
            get;
            set;
        }

        public MediaFile(long mediaId, string sourceURL, MediaFileType fileType, string name, DateTime createDate, string uniqueId) {
            MediaId = mediaId;
            SourceURL = sourceURL;
            FileType = fileType;
            Name = name;
            CreationDate = createDate;
            UniqueId = uniqueId;
        }

        public MediaFile(string sourceURL, MediaFileType fileType, string name) : this(DatabaseConstants.DEFAULT_ID, sourceURL, fileType, name, DateTime.Now, null) {
        }

        public MediaFile(long fileId, CreateMediaFileDto dto) : this(fileId, dto.SourceURL, dto.FileType, dto.Name, DateTime.Now, null) {
        }

        private MediaFile() {
        }

        public override bool Equals(Object other) {
            if ((other == null) || !this.GetType().Equals(other.GetType())) {
                return false;
            }

            var otherFile = other as MediaFile;
            return otherFile.MediaId == MediaId && otherFile.SourceURL == SourceURL
                && otherFile.FileType.Equals(FileType) && otherFile.Name == Name
                && otherFile.UniqueId == UniqueId;
        }

        public override int GetHashCode() {
            return ((int)MediaId << 2) ^ SourceURL.GetHashCode();
        }

        public CreateMediaFileDto GetCreateMediaFileDto() {
            return new CreateMediaFileDto(SourceURL, FileType, Name);
        }
    }
}

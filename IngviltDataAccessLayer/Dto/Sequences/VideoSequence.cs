using Ingvilt.Constants;
using Ingvilt.Models;
using Newtonsoft.Json;

namespace Ingvilt.Dto.Sequences {
    public class VideoSequence : BaseObservableProperty {
        private long coverFileId;
        private string title;
        private string description;
        private bool deleted;

        [JsonIgnore]
        public long SequenceId {
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
        public long CoverFile {
            get {
                return coverFileId;
            }
            set {
                coverFileId = value;
                OnPropertyChanged(nameof(CoverFile));
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

        public VideoSequence(long sequenceId, string title, string description, long coverFile, bool deleted = false, string uniqueId = null) {
            SequenceId = sequenceId;
            Title = title;
            Description = description;
            CoverFile = coverFile;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        public VideoSequence(long sequenceId, CreateVideoSequenceDto dto) : this(sequenceId, dto.Title, dto.Description, dto.CoverFile) {
        }

        protected VideoSequence() {
        }

        public override bool Equals(object other) {
            var otherCast = other as VideoSequence;
            if (otherCast == null) {
                return false;
            }

            return otherCast.SequenceId == SequenceId && otherCast.Title == Title &&
                otherCast.Description == Description && otherCast.CoverFile == CoverFile
                && otherCast.Deleted == Deleted && otherCast.UniqueId == UniqueId;
        }

        public override int GetHashCode() {
            return ((int)SequenceId << 2) ^ Title.GetHashCode();
        }

        public void SetCoverFile(MediaFile file) {
            CoverFile = file.MediaId;
        }

        public void ClearCoverFile() {
            CoverFile = DatabaseConstants.DEFAULT_ID;
        }
    }
}

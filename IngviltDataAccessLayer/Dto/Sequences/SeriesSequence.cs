using Newtonsoft.Json;

namespace Ingvilt.Dto.Sequences {
    public class SeriesSequence : VideoSequence {
        private bool isSeason;
        private int seasonNumber;

        public bool IsSeason {
            get {
                return isSeason;
            }
            set {
                isSeason = value;
                OnPropertyChanged(nameof(IsSeason));
            }
        }

        public int SeasonNumber {
            get {
                return seasonNumber;
            }
            set {
                seasonNumber = value;
                OnPropertyChanged(nameof(SeasonNumber));
            }
        }

        [JsonIgnore]
        public long SeriesId {
            get;
            set;
        }

        public SeriesSequence(long sequenceId, string title, string description, long coverFile, bool deleted, bool isSeason, int seasonNumber, long seriesId, string uniqueId = null) : base(sequenceId, title, description, coverFile, deleted, uniqueId) {
            IsSeason = isSeason;
            SeasonNumber = seasonNumber;
            SeriesId = seriesId;
        }

        public SeriesSequence(long sequenceId, CreateSeriesSequenceDto dto) : this(sequenceId, dto.Title, dto.Description, dto.CoverFile, false, dto.IsSeason, dto.SeasonNumber, dto.SeriesId) {

        }

        private SeriesSequence() {
        }

        public override bool Equals(object other) {
            var otherCast = other as SeriesSequence;
            if (otherCast == null) {
                return false;
            }

            return otherCast.SequenceId == SequenceId && otherCast.CoverFile == CoverFile
                && otherCast.Deleted == Deleted && otherCast.Description == Description
                && otherCast.IsSeason == IsSeason && otherCast.SeasonNumber == SeasonNumber
                && otherCast.SeriesId == SeriesId && otherCast.Title == Title
                && otherCast.UniqueId == otherCast.UniqueId;
        }

        public override int GetHashCode() {
            return ((int)SequenceId << 2) ^ Title.GetHashCode();
        }

        public CreateSeriesSequenceDto GetCreateSeriesSequenceDto() {
            return new CreateSeriesSequenceDto(SeriesId, Title, Description, CoverFile, IsSeason, SeasonNumber, UniqueId);
        }
    }
}

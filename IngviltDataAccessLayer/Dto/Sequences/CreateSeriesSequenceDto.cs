namespace Ingvilt.Dto.Sequences {
    public class CreateSeriesSequenceDto : CreateVideoSequenceDto {
        public long SeriesId {
            get;
        }

        public bool IsSeason {
            get;
        }

        public int SeasonNumber {
            get;
        }

        public CreateSeriesSequenceDto(long seriesId, string title, string description, long coverFile, bool isSeason, int seasonNumber, string guid = null) : base(title, description, coverFile, guid) {
            SeriesId = seriesId;
            IsSeason = isSeason;
            SeasonNumber = seasonNumber;
        }
    }
}

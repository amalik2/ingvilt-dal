namespace Ingvilt.Dto.SeriesNS {
    public class SeriesRatingDto {
        public double UserRating {
            get;
        }

        public double ExternalRating {
            get;
        }

        public SeriesRatingDto(double userRating, double externalRating) {
            UserRating = userRating;
            ExternalRating = externalRating;
        }
    }
}

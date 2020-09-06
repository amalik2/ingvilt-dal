namespace Ingvilt.Models.DataAccess.Search {
    public class VideoUserRatingGreaterThanQueryGenerator : BaseVideoUserRatingQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoRating>";

        public VideoUserRatingGreaterThanQueryGenerator(string value) : base(value) {
        }

        public override string GetDescription() {
            return "Videos must have a rating greater than the specified value";
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return ">";
        }
    }
}

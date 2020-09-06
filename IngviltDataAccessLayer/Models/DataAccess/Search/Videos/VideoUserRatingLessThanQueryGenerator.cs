namespace Ingvilt.Models.DataAccess.Search {
    public class VideoUserRatingLessThanQueryGenerator : BaseVideoUserRatingQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoRating<";

        public VideoUserRatingLessThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return "<";
        }

        public override string GetDescription() {
            return "Videos must have a rating less than the specified value";
        }
    }
}

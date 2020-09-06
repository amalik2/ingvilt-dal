namespace Ingvilt.Models.DataAccess.Search {
    public class VideoExternalRatingLessThanQueryGenerator : BaseVideoExternalRatingQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoExternalRating<";

        public VideoExternalRatingLessThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return "<";
        }

        public override string GetDescription() {
            return "Videos must have a rating assigned by other viewers that is less than the specified value";
        }
    }
}

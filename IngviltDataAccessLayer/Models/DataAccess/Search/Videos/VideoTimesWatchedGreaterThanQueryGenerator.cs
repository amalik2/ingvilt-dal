namespace Ingvilt.Models.DataAccess.Search {
    public class VideoTimesWatchedGreaterThanQueryGenerator : BaseVideoTimesWatchedQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoTimesWatched>";

        public VideoTimesWatchedGreaterThanQueryGenerator(string value) : base(value) {
        }

        public override string GetDescription() {
            return "Videos must be watched more than the specified number of times";
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return ">";
        }
    }
}

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoTimesWatchedLessThanQueryGenerator : BaseVideoTimesWatchedQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoTimesWatched<";

        public VideoTimesWatchedLessThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return "<";
        }

        public override string GetDescription() {
            return "Videos must be watched less than the specified number of times";
        }
    }
}

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoSeriesQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoSeries";

        private string value;

        public VideoSeriesQueryGenerator(string value) {
            this.value = value;
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT DISTINCT(v.video_id) FROM video v, series s WHERE v.deleted = false AND s.deleted = false AND v.series_id = s.series_id AND s.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }

        public string GetDescription() {
            return "Videos must be part of a series that contains the specified text in its name (case insensitive)";
        }
    }
}

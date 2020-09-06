namespace Ingvilt.Models.DataAccess.Search {
    public class VideoTitleQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoTitle";

        private string value;

        public VideoTitleQueryGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Videos must have a title that contains the specified text (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT v.video_id FROM video v WHERE v.title LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }
    }
}

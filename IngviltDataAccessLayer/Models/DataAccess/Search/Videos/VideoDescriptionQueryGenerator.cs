namespace Ingvilt.Models.DataAccess.Search {
    public class VideoDescriptionQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoDescription";

        private string value;

        public VideoDescriptionQueryGenerator(string value) {
            this.value = value;
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT v.video_id FROM video v WHERE v.description LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }

        public string GetDescription() {
            return "Videos must have a description that contains the specified text (case insensitive)";
        }
    }
}

using Ingvilt.Models.DataAccess.Search.Media;

namespace Ingvilt.Models.DataAccess.Search {
    public class MediaFileWithNameGenerator : IMediaFileSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "fileName";

        private string value;

        public MediaFileWithNameGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Files must have a name that contains the specified text (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT f.media_id FROM media_file f WHERE f.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }
    }
}

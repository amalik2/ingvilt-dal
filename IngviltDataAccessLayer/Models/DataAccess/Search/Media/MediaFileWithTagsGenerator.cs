using Ingvilt.Models.DataAccess.Search.Media;

using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class MediaFileWithTagsGenerator : QueryGeneratorWithArrayValue, IMediaFileSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "fileTags";

        public MediaFileWithTagsGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Files must contain all of the tags specified";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT DISTINCT(tmf.media_file_id) AS media_id FROM tag_on_media_file tmf, tag t WHERE tmf.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}) GROUP BY tmf.media_file_id HAVING COUNT(DISTINCT(t.name)) = {value.Count}";
        }
    }
}

using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoWithTagsQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoTags";

        public VideoWithTagsQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must contain all of the tags specified";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT DISTINCT(tv.video_id) FROM tag_on_video tv, tag t WHERE tv.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}) GROUP BY tv.video_id HAVING COUNT(DISTINCT(t.name)) = {value.Count}";
        }
    }
}

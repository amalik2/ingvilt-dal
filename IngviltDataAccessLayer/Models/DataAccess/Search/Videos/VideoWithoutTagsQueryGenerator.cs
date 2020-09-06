using System;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoWithoutTagsQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoWithoutTags";

        public VideoWithoutTagsQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must not contain any of the specified tags";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT v.video_id FROM video v WHERE v.deleted = false AND v.video_id NOT IN (SELECT tv.video_id FROM tag_on_video tv, tag t WHERE tv.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}))";
        }
    }
}

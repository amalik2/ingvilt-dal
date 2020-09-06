using System.Collections.Generic;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class CharactersWithTagsInVideoQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoHasCharactersWithTags";
        
        public CharactersWithTagsInVideoQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must contain some combination of characters that have all of the specified tags";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT DISTINCT(vc.video_id) FROM character c, actor_for_video_character vc, tag_on_character tc, tag t WHERE c.character_id = vc.character_id AND tc.character_id = vc.character_id AND tc.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}) AND c.deleted = false GROUP BY vc.video_id HAVING COUNT(DISTINCT(t.name)) = {value.Count}";
        }
    }
}

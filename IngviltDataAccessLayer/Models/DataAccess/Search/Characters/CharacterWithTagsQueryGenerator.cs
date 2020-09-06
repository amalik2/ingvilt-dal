using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterWithTagsQueryGenerator : QueryGeneratorWithArrayValue, ICharacterSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterTags";
        
        public CharacterWithTagsQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Characters must contain all of the tags specified";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT DISTINCT(tc.character_id) FROM tag_on_character tc, tag t WHERE tc.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}) GROUP BY tc.character_id HAVING COUNT(DISTINCT(t.name)) = {value.Count}";
        }
    }
}

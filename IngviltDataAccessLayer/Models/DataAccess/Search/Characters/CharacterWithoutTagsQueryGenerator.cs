using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterWithoutTagsQueryGenerator : QueryGeneratorWithArrayValue, ICharacterSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterWithoutTags";
        
        public CharacterWithoutTagsQueryGenerator(string value) : base(value) {
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT c.character_id FROM character c WHERE c.deleted = false AND c.character_id NOT IN (SELECT tc.character_id FROM tag_on_character tc, tag t WHERE tc.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}))";
        }

        public string GetDescription() {
            return "Characters must not contain any of the tags specified";
        }
    }
}

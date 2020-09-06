using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class NoneOfCharactersInQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "noneOfCharacters";

        public NoneOfCharactersInQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must not contain characters who's name contains any of the specified values in the array (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var nameClauses = string.Join(" AND ", value.Select(v => $"c.name NOT LIKE '%{v}%'"));
            return $"SELECT DISTINCT(vc.video_id) FROM actor_for_video_character vc, character c WHERE vc.character_id = c.character_id AND c.deleted = false AND {nameClauses}";
        }
    }
}

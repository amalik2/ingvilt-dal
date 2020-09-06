using System;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class OneOfCharactersInVideoSearchQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "oneOfCharacters";

        public OneOfCharactersInVideoSearchQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must contain a character who's name contains one of the specified values in the array (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var nameClauses = string.Join(" OR ", value.Select(v => $"c.name LIKE '%{v}%'"));
            return $"SELECT DISTINCT(vc.video_id) FROM actor_for_video_character vc, character c WHERE vc.character_id = c.character_id AND c.deleted = false AND ({nameClauses})";
        }
    }
}

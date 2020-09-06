namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterInVideoSearchQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterInVideo";

        private string value;

        public CharacterInVideoSearchQueryGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Videos must contain at least one character containing the specified text in their name (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT DISTINCT(vc.video_id) FROM actor_for_video_character vc, character c WHERE vc.character_id = c.character_id AND c.deleted = false AND c.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value;
        }
    }
}

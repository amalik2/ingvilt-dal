namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterNotInVideoSearchQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterNotInVideo";

        private string value;

        public CharacterNotInVideoSearchQueryGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Videos must not contain any characters with the specified text in their name (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT DISTINCT(v.video_id) FROM video v WHERE v.video_id NOT IN (SELECT vc.video_id FROM actor_for_video_character vc, character c WHERE vc.character_id = c.character_id AND c.deleted = false AND c.name LIKE '%{value}%')";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }
    }
}

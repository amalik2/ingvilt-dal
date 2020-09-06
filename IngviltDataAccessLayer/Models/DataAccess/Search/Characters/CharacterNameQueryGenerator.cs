namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterNameQueryGenerator : ICharacterSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterName";

        private string value;

        public CharacterNameQueryGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Characters must have a name that contains the specified text (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT c.character_id FROM character c WHERE c.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }
    }
}

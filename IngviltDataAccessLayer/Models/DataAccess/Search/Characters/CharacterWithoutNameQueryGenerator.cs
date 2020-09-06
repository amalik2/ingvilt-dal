namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterWithoutNameQueryGenerator : ICharacterSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterWithoutName";

        private string value;

        public CharacterWithoutNameQueryGenerator(string value) {
            this.value = value;
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT c.character_id FROM character c WHERE c.name NOT LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }

        public string GetDescription() {
            return "Characters must have a name that does not contain the specified text (case insensitive)";
        }
    }
}

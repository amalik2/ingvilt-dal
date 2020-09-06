namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterDescriptionQueryGenerator : ICharacterSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterDescription";

        private string value;

        public CharacterDescriptionQueryGenerator(string value) {
            this.value = value;
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT c.character_id FROM character c WHERE c.deleted = false AND c.description LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }

        public string GetDescription() {
            return "Characters must have a description that contains the specified text (case insensitive)";
        }
    }
}

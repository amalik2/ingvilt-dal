namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterRatingGreaterThanQueryGenerator : BaseCharacterRatingQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterRating>";

        public CharacterRatingGreaterThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return ">";
        }

        public override string GetDescription() {
            return "Characters must have a rating greater than the specified value";
        }
    }
}

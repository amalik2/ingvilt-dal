namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterRatingLessThanQueryGenerator : BaseCharacterRatingQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterRating<";

        public CharacterRatingLessThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return "<";
        }

        public override string GetDescription() {
            return "Characters must have a rating less than the specified value";
        }
    }
}

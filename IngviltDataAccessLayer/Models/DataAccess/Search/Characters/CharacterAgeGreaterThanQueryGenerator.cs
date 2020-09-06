namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterAgeGreaterThanQueryGenerator : BaseCharacterAgeQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterAge>";

        public CharacterAgeGreaterThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return ">";
        }

        public override string GetDescription() {
            return "Characters must have an age greater than the specified value";
        }
    }
}

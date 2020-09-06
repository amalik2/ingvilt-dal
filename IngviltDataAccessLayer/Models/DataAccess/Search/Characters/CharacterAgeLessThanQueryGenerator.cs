namespace Ingvilt.Models.DataAccess.Search {
    public class CharacterAgeLessThanQueryGenerator : BaseCharacterAgeQueryGenerator {
        public static readonly string GENERATOR_NAME = "characterAge<";

        public CharacterAgeLessThanQueryGenerator(string value) : base(value) {
        }

        public override string GetName() {
            return GENERATOR_NAME;
        }

        protected override string GetComparisonOperator() {
            return "<";
        }

        public override string GetDescription() {
            return "Characters must have an age lower than the specified value";
        }
    }
}

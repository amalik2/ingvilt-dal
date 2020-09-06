using System;

namespace Ingvilt.Models.DataAccess.Search {
    public abstract class BaseCharacterRatingQueryGenerator : QueryGeneratorWithIntValue, ICharacterSearchQueryGenerator {
        public BaseCharacterRatingQueryGenerator(string value) : base(value) {
        }

        public abstract string GetDescription();
        public abstract string GetName();

        public string GetSearchQuery() {
            return $"SELECT c.character_id FROM character c WHERE c.deleted = false AND c.rating {GetComparisonOperator()} {value}";
        }

        protected abstract string GetComparisonOperator();
    }
}

using System;

namespace Ingvilt.Models.DataAccess.Search {
    public abstract class BaseCharacterAgeQueryGenerator : QueryGeneratorWithIntValue, ICharacterSearchQueryGenerator {
        public BaseCharacterAgeQueryGenerator(string value) : base(value) {
        }

        public string GetSearchQuery() {
            return $"SELECT c.character_id, ((JULIANDAY(date('now')) - JULIANDAY(c.birth_date)) / 365.0) AS age FROM character c WHERE c.deleted = false AND c.calendar_id IS NULL AND c.birth_date IS NOT NULL AND age {GetComparisonOperator()} {value}";
        }

        protected abstract string GetComparisonOperator();

        public abstract string GetName();
        public abstract string GetDescription();
    }
}

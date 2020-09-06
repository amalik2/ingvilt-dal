using System;

namespace Ingvilt.Models.DataAccess.Search {
    public abstract class BaseVideoExternalRatingQueryGenerator : QueryGeneratorWithIntValue, IVideoSearchQueryGenerator {
        public BaseVideoExternalRatingQueryGenerator(string value) : base(value) {
        }

        public abstract string GetDescription();
        public abstract string GetName();

        public string GetSearchQuery() {
            return $"SELECT v.video_id FROM video v WHERE v.external_rating {GetComparisonOperator()} {value}";
        }

        protected abstract string GetComparisonOperator();
    }
}

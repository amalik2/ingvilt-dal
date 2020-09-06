using System;

namespace Ingvilt.Models.DataAccess.Search {
    public abstract class BaseVideoUserRatingQueryGenerator : QueryGeneratorWithIntValue, IVideoSearchQueryGenerator {
        public BaseVideoUserRatingQueryGenerator(string value) : base(value) {
        }

        public abstract string GetName();
        public abstract string GetDescription();

        public string GetSearchQuery() {
            return $"SELECT v.video_id FROM video v WHERE v.user_rating {GetComparisonOperator()} {value}";
        }

        protected abstract string GetComparisonOperator();
    }
}

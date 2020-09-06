using System;

namespace Ingvilt.Models.DataAccess.Search {
    public abstract class BaseVideoTimesWatchedQueryGenerator : QueryGeneratorWithIntValue, IVideoSearchQueryGenerator {
        public BaseVideoTimesWatchedQueryGenerator(string value) : base(value) {
        }

        public string GetSearchQuery() {
            return $"SELECT v.video_id FROM video v WHERE v.times_watched {GetComparisonOperator()} {value}";
        }

        protected abstract string GetComparisonOperator();

        public int GetValue() {
            return value;
        }

        public abstract string GetName();
        public abstract string GetDescription();
    }
}

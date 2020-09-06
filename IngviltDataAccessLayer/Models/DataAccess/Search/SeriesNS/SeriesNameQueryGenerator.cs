using Ingvilt.Models.DataAccess.Search.SeriesNS;

namespace Ingvilt.Models.DataAccess.Search {
    public class SeriesNameQueryGenerator : ISeriesSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "seriesName";

        private string value;

        public SeriesNameQueryGenerator(string value) {
            this.value = value;
        }

        public string GetDescription() {
            return "Series must have a name that contains the specified text (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT s.series_id FROM series s WHERE s.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }
    }
}

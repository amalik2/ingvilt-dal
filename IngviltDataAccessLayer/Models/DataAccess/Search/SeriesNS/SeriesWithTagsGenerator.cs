using Ingvilt.Models.DataAccess.Search.SeriesNS;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class SeriesWithTagsGenerator : QueryGeneratorWithArrayValue, ISeriesSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "seriesTags";

        public SeriesWithTagsGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Series must contain all of the tags specified";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"t.name = '{v}'"));
            return $"SELECT DISTINCT(ts.series_id) FROM tag_on_series ts, tag t WHERE ts.tag_id = t.tag_id AND t.deleted = false AND ({tagClauses}) GROUP BY ts.series_id HAVING COUNT(DISTINCT(t.name)) = {value.Count}";
        }
    }
}

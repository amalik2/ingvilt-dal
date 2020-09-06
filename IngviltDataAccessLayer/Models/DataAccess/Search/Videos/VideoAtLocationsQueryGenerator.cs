using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoAtLocationsQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoLocations";

        public VideoAtLocationsQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must be at all of the specified locations";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" OR ", value.Select(v => $"l.name = '{v}'"));
            return $"SELECT DISTINCT(vl.video_id) FROM video_location vl, location l WHERE vl.location_id = l.location_id AND l.deleted = false AND ({tagClauses}) GROUP BY vl.video_id HAVING COUNT(DISTINCT(l.name)) = {value.Count}";
        }
    }
}

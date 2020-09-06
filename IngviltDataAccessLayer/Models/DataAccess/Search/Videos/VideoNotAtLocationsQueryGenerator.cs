using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoNotAtLocationsQueryGenerator : QueryGeneratorWithArrayValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoNotAtLocations";

        public VideoNotAtLocationsQueryGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Videos must not be at any of the specified locations";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var tagClauses = string.Join(" AND ", value.Select(v => $"l.name != '{v}'"));
            return $"SELECT DISTINCT(vl.video_id) FROM video_location vl, location l WHERE vl.location_id = l.location_id AND l.deleted = false AND ({tagClauses})";
        }
    }
}

using Ingvilt.Models.DataAccess.Search.Media;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class MediaFileWithLocationsGenerator : QueryGeneratorWithArrayValue, IMediaFileSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "fileLocations";

        public MediaFileWithLocationsGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Files must contain a combination of locations who contain all of the specified values in their names (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var nameClauses = string.Join(" OR ", value.Select(v => $"l.name LIKE '%{v}%'"));
            var mediaCase = "CASE WHEN lmf.media_id IS NULL THEN l.cover_file_id ELSE lmf.media_id END";
            return $"SELECT DISTINCT({mediaCase}) AS media_id FROM location l LEFT OUTER JOIN location_media_file lmf ON l.location_id = lmf.location_id WHERE (lmf.media_id IS NOT NULL OR l.cover_file_id IS NOT NULL) AND l.deleted = false AND ({nameClauses}) GROUP BY {mediaCase} HAVING COUNT(DISTINCT(l.name)) = {value.Count}";
        }
    }
}

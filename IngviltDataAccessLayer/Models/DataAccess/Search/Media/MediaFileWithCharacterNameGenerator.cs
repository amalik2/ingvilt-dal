using Ingvilt.Models.DataAccess.Search.Media;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class MediaFileWithCharacterNameGenerator : QueryGeneratorWithArrayValue, IMediaFileSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "fileCharacters";

        public MediaFileWithCharacterNameGenerator(string value) : base(value) {
        }

        public string GetDescription() {
            return "Files must contain a combination of characters who contain all of the specified values in their names (case insensitive)";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var nameClauses = string.Join(" OR ", value.Select(v => $"c.name LIKE '%{v}%'"));
            var mediaCase = "CASE WHEN cmf.media_id IS NULL THEN c.cover_file_id ELSE cmf.media_id END";
            return $"SELECT DISTINCT({mediaCase}) AS media_id FROM character c LEFT OUTER JOIN character_media_file cmf ON c.character_id = cmf.character_id WHERE (cmf.media_id IS NOT NULL OR c.cover_file_id IS NOT NULL) AND c.deleted = false AND ({nameClauses}) GROUP BY {mediaCase} HAVING COUNT(DISTINCT(c.name)) = {value.Count}";
        }
    }
}

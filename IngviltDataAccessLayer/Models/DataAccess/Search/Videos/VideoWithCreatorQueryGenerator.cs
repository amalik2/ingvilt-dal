using System;

namespace Ingvilt.Models.DataAccess.Search {
    public class VideoWithCreatorQueryGenerator : QueryGeneratorWithArrayValue, IGeneratorWithSpecificExampleValue, IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "videoWithCreator";

        public VideoWithCreatorQueryGenerator(string value) : base(value) {
            if (this.value.Count != 2) {
                throw new ArgumentException("There must be exactly two values");
            }
        }

        public string GetDescription() {
            return "Videos must contain at least one creator with the specified role and text in their name (case insensitive)";
        }

        public string GetExampleValue() {
            return "[\"role\", \"creator name\"]";
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            var role = value[0];
            var name = value[1];
            return $"SELECT DISTINCT(vc.video_id) FROM video_creator vc, character c WHERE vc.creator_id = c.character_id AND c.deleted = false AND c.name LIKE '%{name}%' AND vc.role LIKE '%{role}%'";
        }
    }
}

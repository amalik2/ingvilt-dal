namespace Ingvilt.Models.DataAccess.Search {
    public class CreatorInVideoQueryGenerator : IVideoSearchQueryGenerator {
        public static readonly string GENERATOR_NAME = "actorInVideo";

        private string value;

        public CreatorInVideoQueryGenerator(string value) {
            this.value = value;
        }

        public string GetName() {
            return GENERATOR_NAME;
        }

        public string GetSearchQuery() {
            return $"SELECT DISTINCT(vc.video_id) FROM actor_for_video_character vc, character c WHERE vc.creator_id = c.character_id AND c.deleted = false AND c.name LIKE '%{value}%'";
        }

        public string GetValueAsString() {
            return value.Replace("'", "\\'");
        }

        public string GetDescription() {
            return "Videos must feature an actor who's name contains the specified text (case insensitive)";
        }
    }
}

namespace Ingvilt.Dto.Tags {
    public class VideoTag : Tag {
        public VideoTag(long tagId, string name, string type, string uniqueId = null) : base(tagId, name, type, uniqueId) {
        }

        public VideoTag(long tagId, CreateVideoTagDto dto) : base(tagId, dto) {
        }
    }
}

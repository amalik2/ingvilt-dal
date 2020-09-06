namespace Ingvilt.Dto.Tags {
    public class CharacterTag : Tag {
        public CharacterTag(long tagId, string name, string type, string uniqueId = null) : base(tagId, name, type, uniqueId) {
        }

        public CharacterTag(long tagId, CreateCharacterTagDto dto) : base(tagId, dto) {
        }
    }
}

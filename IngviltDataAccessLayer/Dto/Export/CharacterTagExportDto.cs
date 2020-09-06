namespace Ingvilt.Dto.Export {
    public class CharacterTagExportDto {
        public string CharacterId;
        public string TagId;

        public CharacterTagExportDto(string characterId, string tagId) {
            CharacterId = characterId;
            TagId = tagId;
        }
    }
}

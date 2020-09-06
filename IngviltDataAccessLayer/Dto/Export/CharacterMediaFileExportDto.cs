namespace Ingvilt.Dto.Export {
    public class CharacterMediaFilesExportDto {
        public string CharacterId;
        public string MediaFileId;

        public CharacterMediaFilesExportDto(string characterId, string mediaFileId) {
            CharacterId = characterId;
            MediaFileId = mediaFileId;
        }
    }
}

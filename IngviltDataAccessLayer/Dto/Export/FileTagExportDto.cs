namespace Ingvilt.Dto.Export {
    public class FileTagExportDto {
        public string FileId;
        public string TagId;

        public FileTagExportDto(string fileId, string tagId) {
            FileId = fileId;
            TagId = tagId;
        }
    }
}

namespace Ingvilt.Dto.Export {
    public class FileLocationExportDto {
        public string FileId;
        public string LocationId;

        public FileLocationExportDto(string fileId, string locationId) {
            FileId = fileId;
            LocationId = locationId;
        }
    }
}

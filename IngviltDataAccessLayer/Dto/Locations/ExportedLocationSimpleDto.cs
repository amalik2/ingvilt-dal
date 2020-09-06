namespace Ingvilt.Dto.Locations {
    public class ExportedLocationSimpleDto {
        public Location Details;
        public string FileId;
        public string PublisherId;

        public ExportedLocationSimpleDto(Location details, string fileId, string publisherId) {
            Details = details;
            FileId = fileId;
            PublisherId = publisherId;
        }
    }
}

namespace Ingvilt.Dto.Publishers {
    public class ExportedPublisherSimpleDto {
        public Publisher Details;
        public string FileId;

        public ExportedPublisherSimpleDto(Publisher details, string fileId) {
            Details = details;
            FileId = fileId;
        }
    }
}

namespace Ingvilt.Dto.Videos {
    public class ExportedVideoSimpleDto {
        public Video Details;
        public string FileId;
        public string PublisherId;
        public string SeriesId;

        public ExportedVideoSimpleDto(Video details, string fileId, string publisherId, string seriesId) {
            Details = details;
            FileId = fileId;
            PublisherId = publisherId;
            SeriesId = seriesId;
        }
    }
}

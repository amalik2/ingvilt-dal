namespace Ingvilt.Dto.Export {
    public class VideoLocationExportDto {
        public string VideoId;
        public string LocationId;

        public VideoLocationExportDto(string videoId, string locationId) {
            VideoId = videoId;
            LocationId = locationId;
        }
    }
}

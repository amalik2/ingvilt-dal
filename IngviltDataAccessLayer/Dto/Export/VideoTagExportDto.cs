namespace Ingvilt.Dto.Export {
    public class VideoTagExportDto {
        public string VideoId;
        public string TagId;

        public VideoTagExportDto(string videoId, string tagId) {
            VideoId = videoId;
            TagId = tagId;
        }
    }
}

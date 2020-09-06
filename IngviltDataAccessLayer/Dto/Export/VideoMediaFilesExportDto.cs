namespace Ingvilt.Dto.Export {
    public class VideoMediaFilesExportDto {
        public string VideoId;
        public string MediaFileId;

        public VideoMediaFilesExportDto(string videoId, string mediaFileId) {
            VideoId = videoId;
            MediaFileId = mediaFileId;
        }
    }
}

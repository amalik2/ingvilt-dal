namespace Ingvilt.Dto.Export {
    public class VideoCreatorExportDto {
        public string VideoId;
        public string CreatorId;
        public string Role;

        public VideoCreatorExportDto(string videoId, string creatorId, string role) {
            VideoId = videoId;
            CreatorId = creatorId;
            Role = role;
        }
    }
}

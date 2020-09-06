namespace Ingvilt.Dto.Export {
    public class VideosInSequencesExportDto {
        public string VideoId;
        public string SequenceId;
        public int Order;

        public VideosInSequencesExportDto(string videoId, string sequenceId, int order) {
            VideoId = videoId;
            SequenceId = sequenceId;
            Order = order;
        }
    }
}

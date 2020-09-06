namespace Ingvilt.Dto.Sequences {
    public class VideoInSequenceOrder {
        public long VideoId {
            get;
        }

        public int Order {
            get;
        }

        public VideoInSequenceOrder(long videoId, int order) {
            VideoId = videoId;
            Order = order;
        }
    }
}

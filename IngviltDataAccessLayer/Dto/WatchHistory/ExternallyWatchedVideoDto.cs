using System;

namespace Ingvilt.Dto.WatchHistory {
    public class ExternallyWatchedVideoDto {
        public string VideoUrl {
            get;
            set;
        }

        public DateTime Time {
            get;
            set;
        }

        public ExternallyWatchedVideoDto(string videoUrl, DateTime time) {
            VideoUrl = videoUrl;
            Time = time;
        }
    }
}

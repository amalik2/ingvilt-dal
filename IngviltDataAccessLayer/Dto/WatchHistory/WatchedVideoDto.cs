using System;

namespace Ingvilt.Dto.WatchHistory {
    public class WatchedVideoDto {
        public long HistoryId {
            get;
        }

        public long VideoId {
            get;
        }

        public string VideoTitle {
            get;
        }

        public DateTime WatchDate {
            get;
        }

        public WatchedVideoDto(long historyId, long videoId, string videoTitle, DateTime watchDate) {
            HistoryId = historyId;
            VideoId = videoId;
            VideoTitle = videoTitle;
            WatchDate = watchDate;
        }
    }
}

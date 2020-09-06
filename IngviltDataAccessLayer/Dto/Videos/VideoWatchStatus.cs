using System;
using System.Collections.Generic;

namespace Ingvilt.Dto {
    public enum VideoWatchStatus {
        WONT_WATCH,
        UNDECIDED,
        NEED_TO_WATCH,
        WATCHED
    }

    public class VideoWatchStatusWrapper {
        public VideoWatchStatus WatchStatus {
            get;
            set;
        }

        public string StatusDisplay {
            get {
                if (WatchStatus == VideoWatchStatus.NEED_TO_WATCH) {
                    return "Need to watch";
                } else if (WatchStatus == VideoWatchStatus.UNDECIDED) {
                    return "Undecided";
                } else if (WatchStatus == VideoWatchStatus.WONT_WATCH) {
                    return "Not worth watching";
                } else if (WatchStatus == VideoWatchStatus.WATCHED) {
                    return "Watched";
                }

                throw new InvalidOperationException("Invalid watch status");
            }
        }

        public VideoWatchStatusWrapper(VideoWatchStatus watchStatus) {
            WatchStatus = watchStatus;
        }

        public static List<VideoWatchStatusWrapper> AllWatchStatuses() {
            var list = new List<VideoWatchStatusWrapper>();
            list.Add(new VideoWatchStatusWrapper(VideoWatchStatus.UNDECIDED));
            list.Add(new VideoWatchStatusWrapper(VideoWatchStatus.NEED_TO_WATCH));
            list.Add(new VideoWatchStatusWrapper(VideoWatchStatus.WONT_WATCH));
            list.Add(new VideoWatchStatusWrapper(VideoWatchStatus.WATCHED));
            return list;
        }

        public override bool Equals(object other) {
            var otherCast = other as VideoWatchStatusWrapper;
            if (otherCast == null) {
                return false;
            }

            return otherCast.WatchStatus == WatchStatus;
        }

        public override int GetHashCode() {
            return StatusDisplay.GetHashCode();
        }
    }
}

using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public abstract class VideoSortCriteria : BaseSortCriteria {
        public VideoSortCriteria(bool ascending) : base(ascending) {
        }
    }

    public class VideoCreationDateSortCriteria : VideoSortCriteria {

        public VideoCreationDateSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.video_id" };
        }
    }

    public class VideoTitleSortCriteria : VideoSortCriteria {

        public VideoTitleSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.title" };
        }
    }

    public class VideoReleaseDateSortCriteria : VideoSortCriteria {

        public VideoReleaseDateSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.release_date" };
        }
    }

    public class VideoTimelineDateSortCriteria : VideoSortCriteria {

        public VideoTimelineDateSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.timeline_date" };
        }
    }

    public class VideoExternalRatingSortCriteria : VideoSortCriteria {

        public VideoExternalRatingSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.external_rating" };
        }
    }

    public class VideoUserRatingSortCriteria : VideoSortCriteria {

        public VideoUserRatingSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.user_rating" };
        }
    }

    public class VideoDurationSortCriteria : VideoSortCriteria {

        public VideoDurationSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "v.duration_in_seconds" };
        }
    }
}

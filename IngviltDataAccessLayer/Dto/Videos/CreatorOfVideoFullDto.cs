using Ingvilt.Constants;
using Ingvilt.Models;

namespace Ingvilt.Dto.Videos {
    public class CreatorOfVideoFullDto : BaseObservableProperty {
        private string actorName;
        private long actorId;

        public Video Video {
            get;
            set;
        }

        public long ActorId {
            get {
                return actorId;
            }
            set {
                actorId = value;
                OnPropertyChanged(nameof(ActorId));
            }
        }

        public string ActorName {
            get {
                return actorName;
            }
            set {
                actorName = value;
                OnPropertyChanged(nameof(ActorName));
            }
        }

        public string Role {
            get;
            set;
        }

        public CreatorOfVideoFullDto(Video video, long creatorId, string creatorName, string role) {
            Video = video;
            ActorId = creatorId;
            ActorName = creatorName;
            Role = role;
        }

        public CreatorOfVideoFullDto(Video video) {
            Video = video;
            Clear();
        }

        public override bool Equals(object other) {
            var otherVideo = other as CreatorOfVideoFullDto;
            if (otherVideo == null) {
                return false;
            }

            return otherVideo.Video.VideoId == Video.VideoId;
        }

        public override int GetHashCode() {
            return ((int)Video.VideoId << 2) ^ Video.Title.GetHashCode();
        }

        public void Clear() {
            ActorId = DatabaseConstants.DEFAULT_ID;
            ActorName = "";
            Role = "";
        }
    }
}

namespace Ingvilt.Dto.Publishers {
    public class PublisherBasicDetails {
        public long PublisherId {
            get;
        }

        public string Name {
            get;
        }

        public PublisherBasicDetails(long publisherId, string name) {
            PublisherId = publisherId;
            Name = name;
        }

        public override bool Equals(object other) {
            var otherCast = other as PublisherBasicDetails;
            if (otherCast == null) {
                return false;
            }

            return otherCast.PublisherId == PublisherId;
        }

        public override int GetHashCode() {
            return ((int)PublisherId << 2) ^ Name.GetHashCode();
        }
    }
}

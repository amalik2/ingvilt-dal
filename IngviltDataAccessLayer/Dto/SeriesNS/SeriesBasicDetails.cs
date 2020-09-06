using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto.SeriesNS {
    public class SeriesBasicDetails {
        public long PublisherId {
            get;
        }

        public string Name {
            get;
        }

        public long SeriesId {
            get;
        }

        public SeriesBasicDetails(long seriesId, string name, long publisherId) {
            PublisherId = publisherId;
            Name = name;
            SeriesId = seriesId;
        }

        public override bool Equals(object other) {
            var otherSeries = other as SeriesBasicDetails;
            if (otherSeries == null) {
                return false;
            }

            return otherSeries.SeriesId == SeriesId;
        }

        public override int GetHashCode() {
            return ((int)SeriesId << 2) ^ Name.GetHashCode();
        }
    }
}

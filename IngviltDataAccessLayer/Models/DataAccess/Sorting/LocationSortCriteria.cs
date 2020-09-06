using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class LocationSortCriteria : BaseSortCriteria {
        public LocationSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "l.location_id" };
        }
    }
}

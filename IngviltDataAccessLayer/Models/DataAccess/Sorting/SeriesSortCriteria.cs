using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class SeriesSortCriteria : BaseSortCriteria {
        public SeriesSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "s.series_id" };
        }
    }
}

using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class LibrarySortCriteria : BaseSortCriteria {
        public LibrarySortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>(){ "library_id" };
        }
    }
}

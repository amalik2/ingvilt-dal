using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class SavedSearchResultsSortCriteria : BaseSortCriteria {
        public SavedSearchResultsSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "name" };
        }
    }
}

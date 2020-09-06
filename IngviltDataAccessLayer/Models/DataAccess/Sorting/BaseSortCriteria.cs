using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public abstract class BaseSortCriteria {
        private bool Ascending;

        private string GetAscendingString() {
            return Ascending ? "" : "DESC";
        }

        public BaseSortCriteria(bool ascending) {
            Ascending = ascending;
        }

        protected abstract List<string> GetSortQueryColumnNames();

        public virtual string GetSortQueryText() {
            return $"ORDER BY {string.Join(",", GetSortQueryColumnNames())} {GetAscendingString()}";
        }
    }
}

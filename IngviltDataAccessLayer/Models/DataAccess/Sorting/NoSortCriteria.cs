using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class NoSortCriteria : BaseSortCriteria {
        public NoSortCriteria() : base(true) {
        }

        public override string GetSortQueryText() {
            return "";
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>();
        }
    }
}

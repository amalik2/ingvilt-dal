using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class PublisherSortCriteria : BaseSortCriteria {
        public PublisherSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "publisher_id" };
        }
    }
}

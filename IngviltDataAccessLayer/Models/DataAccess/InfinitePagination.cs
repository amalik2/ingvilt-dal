using System;

namespace Ingvilt.Models.DataAccess {
    public class InfinitePagination : Pagination {
        public InfinitePagination() : base(null, Int32.MaxValue, 0) {
        }

        public override string GetQueryString() {
            return "";
        }
    }
}

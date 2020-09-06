using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class WatchHistoryWatchDateSortCriteria : BaseSortCriteria {
        public WatchHistoryWatchDateSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "watch_date" };
        }
    }

    public class WatchHistoryTitleSortCriteria : BaseSortCriteria {
        public WatchHistoryTitleSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "title" };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class CalendarSortCriteria : BaseSortCriteria {
        public CalendarSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "calendar_id" };
        }
    }
}

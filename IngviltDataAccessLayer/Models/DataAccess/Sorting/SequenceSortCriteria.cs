using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class SequenceSortCriteria : BaseSortCriteria {
        public SequenceSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "vs.sequence_id" };
        }
    }

    public class SequenceTitleSortCriteria : BaseSortCriteria {
        public SequenceTitleSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "vs.title" };
        }
    }

    public class SequenceSeasonChronologySortCriteria : BaseSortCriteria {
        public SequenceSeasonChronologySortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "CASE WHEN is_season IS true THEN 0 ELSE 99999 END", "season_number" };
        }
    }
}

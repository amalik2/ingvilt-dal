using Ingvilt.Dto;
using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public abstract class MediaFileSortCriteria : BaseSortCriteria {

        public MediaFileSortCriteria(bool ascending) : base(ascending) {
        }
    }

    public class MediaFileNameSortCriteria : MediaFileSortCriteria {

        public MediaFileNameSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "name" };
        }
    }

    public class MediaFileTypeSortCriteria : MediaFileSortCriteria {

        public MediaFileTypeSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            var imageOrdinal = MediaFileType.IMAGE_TYPE.Ordinal;
            var urlOrdinal = MediaFileType.URL_TYPE.Ordinal;
            return new List<string>() { $"CASE WHEN file_type IS {imageOrdinal} THEN 0 WHEN file_type IS {urlOrdinal} THEN 1 ELSE 2 END" };
        }
    }

    public class MediaFileDateSortCriteria : MediaFileSortCriteria {

        public MediaFileDateSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "create_date" };
        }
    }
}

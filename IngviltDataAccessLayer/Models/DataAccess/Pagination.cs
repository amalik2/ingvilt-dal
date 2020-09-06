using Ingvilt.Models.DataAccess.Sorting;

using System;

namespace Ingvilt.Models.DataAccess {
    public class Pagination {
        private int Offset {
            get;
            set;
        }

        public int PageNumber {
            get {
                return (int)Math.Floor((double)Offset / (double)CountPerPage) + 1;
            }
        }

        public int CountPerPage {
            get;
        }

        public BaseSortCriteria SortCriteria {
            get;
        }

        public Pagination(BaseSortCriteria sortCriteria, int countPerPage, int offset = 0) {
            SortCriteria = sortCriteria;
            CountPerPage = countPerPage;
            Offset = offset;
        }

        public Pagination GetNextPage() {
            return new Pagination(SortCriteria, CountPerPage, Offset + CountPerPage);
        }

        public Pagination GetPreviousPage() {
            if (PageNumber == 1) {
                throw new InvalidOperationException("There is no previous page");
            }

            return new Pagination(SortCriteria, CountPerPage, Offset - CountPerPage);
        }

        public static Pagination FirstPage(int countPerPage, BaseSortCriteria sortCriteria) {
            return GetForPage(1, countPerPage, sortCriteria);
        }

        public static Pagination FirstPageWithDefaultCount(BaseSortCriteria sortCriteria) {
            return FirstPage(30, sortCriteria);
        }

        public static Pagination GetForPage(int page, int countPerPage, BaseSortCriteria sortCriteria) {
            return new Pagination(sortCriteria, countPerPage, (page - 1) * countPerPage);
        }

        public virtual string GetQueryString() {
            return $"{SortCriteria.GetSortQueryText()} LIMIT {Offset}, {CountPerPage}";
        }

        public void DecrementOffset() {
            --Offset;
        }

        public void IncrementOffset() {
            ++Offset;
        }
    }
}

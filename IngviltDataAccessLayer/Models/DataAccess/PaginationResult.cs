using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess {
    public class PaginationResult<T> {
        public List<T> Results {
            get;
        }

        public Pagination NextPage {
            get;
        }

        public Pagination PreviousPage {
            get;
        }

        public bool HasNextPage => NextPage != null;

        public PaginationResult(List<T> results, Pagination nextPage, Pagination previousPage) {
            Results = results;
            NextPage = nextPage;
            PreviousPage = previousPage;
        }

        public static PaginationResult<T> CreateResultFromCurrentPage(List<T> results, Pagination currentPage) {
            return new PaginationResult<T>(results, results.Count == 0 ? null : currentPage.GetNextPage(), currentPage);
        }
    }
}

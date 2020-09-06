using Ingvilt.Dto.Libraries;
using Ingvilt.Models.DataAccess.Search;

using System.Collections.Generic;

namespace Ingvilt.Dto.Search {
    public class SearchRequestDto : IAttachedToLibrary {
        public long QueryId {
            get;
        }

        public long LibraryId {
            get;
            set;
        }

        public List<ISearchQueryGenerator> Generators {
            get;
        }
        public SearchRequestDto(long queryId, long libraryId, List<ISearchQueryGenerator> generators) {
            QueryId = queryId;
            LibraryId = libraryId;
            Generators = generators;
        }
    }
}

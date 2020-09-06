using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto.Libraries {
    public interface IAttachedToLibrary {
        long LibraryId {
            get;
            set;
        }
    }
}

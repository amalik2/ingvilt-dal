using Ingvilt.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto {
    public class CreateLibraryDto {
        public string Name {
            get;
        }

        public long BackgroundImageId {
            get;
        }

        public CreateLibraryDto(string name, long backgroundImageId) {
            Name = name;
            BackgroundImageId = backgroundImageId;
        }

        public CreateLibraryDto(string name) : this(name, DatabaseConstants.DEFAULT_ID) {
        }
    }
}

using Ingvilt.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto.Sequences {
    public class CreateVideoSequenceDto {
        public string Title {
            get;
            set;
        }

        public string Description {
            get;
            set;
        }

        public long CoverFile {
            get;
            set;
        }

        public string UniqueId {
            get;
            set;
        }

        public CreateVideoSequenceDto(string title, string description, long coverFile, string uniqueId = null) {
            Title = title;
            Description = description;
            CoverFile = coverFile;

            if (uniqueId == null) {
                uniqueId = UniqueIdUtil.GenerateUniqueId();
            }
            UniqueId = uniqueId;
        }
    }
}

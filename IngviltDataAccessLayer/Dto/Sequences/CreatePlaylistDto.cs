using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto.Sequences {
    public class CreatePlaylistDto : CreateVideoSequenceDto {
        public long LibraryId {
            get;
        }

        public CreatePlaylistDto(long libraryId, string title, string description, long coverFile, string guid = null) : base(title, description, coverFile, guid) {
            LibraryId = libraryId;
        }
    }
}

using Ingvilt.Dto.Libraries;
using Newtonsoft.Json;
using System;

namespace Ingvilt.Dto.Sequences {
    public class PlaylistDto : VideoSequence, IAttachedToLibrary {
        private DateTime creationDate;

        [JsonIgnore]
        public long LibraryId {
            get;
            set;
        }

        public DateTime CreationDate {
            get {
                return creationDate;
            }
            set {
                creationDate = value;
                OnPropertyChanged(nameof(CreationDate));
            }
        }

        public PlaylistDto(long sequenceId, string title, string description, long coverFile, bool deleted, long libraryId, DateTime creationDate, string uniqueId = null) : base(sequenceId, title, description, coverFile, deleted, uniqueId) {
            LibraryId = libraryId;
            CreationDate = creationDate;
        }

        public PlaylistDto(long sequenceId, CreatePlaylistDto dto) : this(sequenceId, dto.Title, dto.Description, dto.CoverFile, false, dto.LibraryId, DateTime.Now) {
        }

        private PlaylistDto() {
        }

        public CreatePlaylistDto GetCreatePlaylistDto() {
            return new CreatePlaylistDto(LibraryId, Title, Description, CoverFile, UniqueId);
        }
    }
}

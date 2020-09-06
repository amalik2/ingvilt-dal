namespace Ingvilt.Dto.Locations {
    public class CreateLocationDto {
        public string Name {
            get;
        }

        public string Description {
            get;
        }

        public long LibraryId {
            get;
        }

        public long PublisherId {
            get;
        }

        public long CoverFileId {
            get;
            set;
        }

        public CreateLocationDto(string name, string description, long libraryId, long publisherId, long coverFileId) {
            Name = name;
            Description = description;
            LibraryId = libraryId;
            PublisherId = publisherId;
            CoverFileId = coverFileId;
        }
    }
}

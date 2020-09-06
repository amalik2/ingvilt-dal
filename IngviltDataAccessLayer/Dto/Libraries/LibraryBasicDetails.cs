namespace Ingvilt.Dto {
    public class LibraryBasicDetails {
        public long LibraryId {
            get;
        }

        public string Name {
            get;
        }

        public LibraryBasicDetails(long libraryId, string name) {
            LibraryId = libraryId;
            Name = name;
        }
    }
}

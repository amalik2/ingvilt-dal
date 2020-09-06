namespace Ingvilt.Dto.Publishers {
    public class CreatePublisherDto {
        public string Name {
            get;
        }

        public string SiteURL {
            get;
        }

        public long LogoFileId {
            get;
        }

        public string Description {
            get;
        }

        public long LibraryId {
            get;
        }

        public CreatePublisherDto(string name, string siteUrl, long logoFileId, string description, long libraryId) {
            Name = name;
            SiteURL = siteUrl;
            LogoFileId = logoFileId;
            Description = description;
            LibraryId = libraryId;
        }
    }
}

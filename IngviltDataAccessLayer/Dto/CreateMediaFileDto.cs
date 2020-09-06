using System.Collections.Generic;

namespace Ingvilt.Dto {
    public class CreateMediaFileDto {
        public string SourceURL {
            get;
        }

        public MediaFileType FileType {
            get;
        }

        public string Name {
            get;
        }

        public List<long> TagIds {
            get;
        }

        public CreateMediaFileDto(string sourceURL, MediaFileType fileType, string name, List<long> tagIds) {
            SourceURL = sourceURL;
            FileType = fileType;
            Name = name;
            TagIds = tagIds;
        }

        public CreateMediaFileDto(string sourceURL, MediaFileType fileType, string name) : this(sourceURL, fileType, name, new List<long>()) { 
        }
    }
}

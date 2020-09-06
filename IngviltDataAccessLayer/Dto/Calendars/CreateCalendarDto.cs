namespace Ingvilt.Dto.Calendars {
    public class CreateCalendarDto {
        public string DateFormat {
            get;
        }

        public string Name {
            get;
        }

        public string Description {
            get;
        }

        public long LibraryId {
            get;
        }

        public CreateCalendarDto(string dateFormat, string name, string description, long libraryId) {
            DateFormat = dateFormat;
            Name = name;
            Description = description;
            LibraryId = libraryId;
        }
    }
}

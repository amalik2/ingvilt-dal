namespace Ingvilt.Dto.Characters {
    public class ExportedCharacterSimpleDto {
        public Character Details;
        public string FileId;
        public string CalendarId;

        public ExportedCharacterSimpleDto(Character details, string fileId, string calendarId) {
            Details = details;
            FileId = fileId;
            CalendarId = calendarId;
        }
    }
}

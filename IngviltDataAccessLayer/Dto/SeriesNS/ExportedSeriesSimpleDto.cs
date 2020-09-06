namespace Ingvilt.Dto.SeriesNS {
    public class ExportedSeriesSimpleDto {
        public Series Details;
        public string FileId;
        public string PublisherId;
        public string CalendarId;

        public ExportedSeriesSimpleDto(Series details, string fileId, string publisherId, string calendarId) {
            Details = details;
            FileId = fileId;
            PublisherId = publisherId;
            CalendarId = calendarId;
        }
    }
}

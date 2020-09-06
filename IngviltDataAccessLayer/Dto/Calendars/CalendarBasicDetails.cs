using Ingvilt.Constants;

namespace Ingvilt.Dto.Calendars {
    public class CalendarBasicDetails {
        public static readonly CalendarBasicDetails REAL_WORLD_CALENDAR = new CalendarBasicDetails(DatabaseConstants.DEFAULT_ID, "Real world calendar", "MMMM DD, YYYY");

        public long CalendarId {
            get;
        }

        public string Name {
            get;
        }

        public string DateFormat {
            get;
        }

        public CalendarBasicDetails(long calendarId, string name, string dateFormat) {
            CalendarId = calendarId;
            Name = name;
            DateFormat = dateFormat;
        }

        public override bool Equals(object other) {
            var otherCast = other as CalendarBasicDetails;
            if (otherCast == null) {
                return false;
            }

            return otherCast.CalendarId == CalendarId;
        }

        public override int GetHashCode() {
            return ((int)CalendarId << 2) ^ Name.GetHashCode();
        }
    }
}

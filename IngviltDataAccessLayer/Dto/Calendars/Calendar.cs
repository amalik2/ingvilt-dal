using Ingvilt.Constants;
using Ingvilt.Dto.Libraries;
using Ingvilt.Models;
using Newtonsoft.Json;

namespace Ingvilt.Dto.Calendars {
    public class Calendar : BaseObservableProperty, IAttachedToLibrary {
        private bool deleted;
        private string dateFormat;
        private string name;
        private string description;

        [JsonIgnore]
        public long CalendarId {
            get;
            set;
        }

        public string DateFormat {
            get {
                return dateFormat;
            }
            set {
                dateFormat = value;
                OnPropertyChanged(nameof(BaseObservableProperty));
            }
        }

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Description {
            get {
                return description;
            }
            set {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public bool Deleted {
            get {
                return deleted;
            }
            set {
                deleted = value;
                OnPropertyChanged(nameof(Deleted));
            }
        }

        [JsonIgnore]
        public long LibraryId {
            get;
            set;
        }

        public string UniqueId {
            get;
            set;
        }

        private Calendar() {
        }

        public Calendar(long calendarId, string dateFormat, string name, string description, long libraryId, bool deleted = false, string uniqueId = null) {
            CalendarId = calendarId;
            DateFormat = dateFormat;
            Name = name;
            Description = description;
            LibraryId = libraryId;
            Deleted = deleted;
            UniqueId = uniqueId;
        }

        public Calendar(long calendarId, CreateCalendarDto dto) : this(calendarId, dto.DateFormat, dto.Name, dto.Description, dto.LibraryId) {
        }

        public override bool Equals(object other) {
            var otherCast = other as Calendar;
            if (otherCast == null) {
                return false;
            }

            return otherCast.CalendarId == CalendarId;
        }

        public override int GetHashCode() {
            return ((int)CalendarId << 2) ^ Name.GetHashCode();
        }

        public CalendarBasicDetails GetBasicDetails() {
            return new CalendarBasicDetails(CalendarId, Name, DateFormat);
        }

        public bool IsReadWorldCalendar() {
            return CalendarId == CalendarBasicDetails.REAL_WORLD_CALENDAR.CalendarId;
        }

        public static Calendar GetRealWorldCalendar() {
            return new Calendar(
                CalendarBasicDetails.REAL_WORLD_CALENDAR.CalendarId,
                CalendarBasicDetails.REAL_WORLD_CALENDAR.DateFormat,
                CalendarBasicDetails.REAL_WORLD_CALENDAR.Name,
                "",
                DatabaseConstants.DEFAULT_ID
            );
        }

        public CreateCalendarDto GetCreateCalendarDto() {
            return new CreateCalendarDto(DateFormat, Name, Description, LibraryId);
        }
    }
}

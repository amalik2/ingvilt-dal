using System;

namespace Ingvilt.Dto.Characters {
    public class CreateCharacterDto {
        public string Name {
            get;
            set;
        }

        public string Description {
            get;
            set;
        }

        public Nullable<DateTime> BirthDate {
            get;
            set;
        }

        public Nullable<DateTime> CareerStartDate {
            get;
            set;
        }

        public Nullable<DateTime> CareerEndDate {
            get;
            set;
        }

        public double Rating {
            get;
            set;
        }

        public long LibraryId {
            get;
            set;
        }

        public long CoverMediaId {
            get;
            set;
        }

        public long CalendarId {
            get;
            set;
        }

        public bool IsCreator {
            get;
            set;
        }

        public CreateCharacterDto(string name, string description, Nullable<DateTime> birthDate, Nullable<DateTime> careerStartDate, Nullable<DateTime> careerEndDate, double rating, long libraryId, long coverMediaId, long calendarId, bool isCreator = false) {
            Name = name;
            Description = description;
            BirthDate = birthDate;
            CareerStartDate = careerStartDate;
            CareerEndDate = careerEndDate;
            Rating = rating;
            LibraryId = libraryId;
            CoverMediaId = coverMediaId;
            CalendarId = calendarId;
            IsCreator = isCreator;
        }
    }
}

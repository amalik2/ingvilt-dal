using Ingvilt.Constants;
using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.Libraries;
using Ingvilt.Dto.Videos;
using Ingvilt.Models;
using Ingvilt.Util;
using Newtonsoft.Json;
using System;

namespace Ingvilt.Dto.Characters {
    public class Character : BaseObservableProperty, INamedEntity, IAttachedToLibrary {
        private long coverMediaId;
        private bool deleted;
        private string name;
        private string description;
        private Nullable<DateTime> birthDate;
        private Nullable<DateTime> careerStartDate;
        private Nullable<DateTime> careerEndDate;
        private double rating;
        private long calendarId;

        [JsonIgnore]
        public long CharacterId {
            get;
            set;
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

        public Nullable<DateTime> BirthDate {
            get {
                return birthDate;
            }
            set {
                birthDate = value;
                OnPropertyChanged(nameof(BirthDate));
            }
        }

        public Nullable<DateTime> CareerStartDate {
            get {
                return careerStartDate;
            }
            set {
                careerStartDate = value;
                OnPropertyChanged(nameof(CareerStartDate));
            }
        }

        public Nullable<DateTime> CareerEndDate {
            get {
                return careerEndDate;
            }
            set {
                careerEndDate = value;
                OnPropertyChanged(nameof(CareerEndDate));
            }
        }

        public double Rating {
            get {
                return rating;
            }
            set {
                rating = value;
                OnPropertyChanged(nameof(Rating));
            }
        }

        [JsonIgnore]
        public long LibraryId {
            get;
            set;
        }

        [JsonIgnore]
        public long CoverMediaId {
            get {
                return coverMediaId;
            }
            set {
                coverMediaId = value;
                OnPropertyChanged(nameof(CoverMediaId));
            }
        }

        [JsonIgnore]
        public long CalendarId {
            get {
                return calendarId;
            }
            set {
                calendarId = value;
                OnPropertyChanged(nameof(CalendarId));
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

        public bool Creator {
            get;
            set;
        }

        public string UniqueId {
            get;
            set;
        }

        private Character() {
        }

        public Character(long characterId, string name, string description, Nullable<DateTime> birthDate, Nullable<DateTime> careerStartDate, Nullable<DateTime> careerEndDate, double rating, long libraryId, long coverMediaId, long calendarId, bool deleted = false, bool creator = false, string uniqueId = null) {
            CharacterId = characterId;
            Name = name;
            Description = description;
            BirthDate = birthDate;
            CareerStartDate = careerStartDate;
            CareerEndDate = careerEndDate;
            Rating = rating;
            LibraryId = libraryId;
            CoverMediaId = coverMediaId;
            CalendarId = calendarId;
            Deleted = deleted;
            Creator = creator;
            UniqueId = uniqueId;
        }

        public Character(long characterId, CreateCharacterDto dto) : this(characterId, dto.Name, dto.Description, dto.BirthDate, dto.CareerStartDate, dto.CareerEndDate, dto.Rating, dto.LibraryId, dto.CoverMediaId, dto.CalendarId, false, dto.IsCreator) {
        }

        public override bool Equals(object other) {
            var otherCharacter = other as Character;
            if (otherCharacter == null) {
                return false;
            }

            return otherCharacter.CharacterId == CharacterId && otherCharacter.BirthDate == BirthDate
                && otherCharacter.CalendarId == CalendarId && otherCharacter.CareerEndDate == CareerEndDate
                && otherCharacter.CareerStartDate == CareerStartDate && otherCharacter.CoverMediaId == CoverMediaId
                && otherCharacter.Creator == Creator && otherCharacter.Deleted == Deleted && otherCharacter.Description == Description
                && otherCharacter.LibraryId == LibraryId && otherCharacter.Name == Name && otherCharacter.Rating == Rating
                && otherCharacter.UniqueId == UniqueId;
        }

        public override int GetHashCode() {
            return ((int)CharacterId << 2) ^ Name.GetHashCode();
        }

        public void SetLogoFile(MediaFile file) {
            CoverMediaId = file.MediaId;
        }

        public void ClearLogoFile() {
            CoverMediaId = DatabaseConstants.DEFAULT_ID;
        }

        public string GetName() {
            return Name;
        }

        public Nullable<int> GetAge(Calendar calendar) {
            if (calendar.CalendarId != CalendarId) {
                throw new ArgumentException("The calendar id does not match this character's calendar");
            }


            if (BirthDate == null || !calendar.IsReadWorldCalendar()) {
                return null;
            }

            return DateUtil.GetAge(BirthDate.Value, DateTime.Now);
        }

        public Nullable<int> GetAgeInVideo(Video video) {
            if (!video.TimelineDate.HasValue || !BirthDate.HasValue) {
                return null;
            }

            return DateUtil.GetAge(BirthDate.Value, video.TimelineDate.Value);
        }

        public CreateCharacterDto GetCreateCharacterDto() {
            return new CreateCharacterDto(Name, Description, BirthDate, CareerStartDate, CareerEndDate, Rating, LibraryId, CoverMediaId, CalendarId, Creator);
        }
    }
}

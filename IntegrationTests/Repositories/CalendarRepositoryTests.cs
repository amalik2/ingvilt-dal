using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTestingRedo.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class CalendarRepositoryTests : BaseTest {
        private CalendarRepository repository = new CalendarRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();

        private Library testLibrary = null;

        public CalendarRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        private Calendar CreateAndRetrieveCalendar(CreateCalendarDto dto) {
            var id = repository.CreateCalendar(dto);
            return repository.GetCalendar(id);
        }

        private Calendar CreateAndRetrieveCalendar() {
            return CreateAndRetrieveCalendar(new CreateCalendarDto("MM-DD-YYYY", "", "", testLibrary.LibraryId));
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new CalendarSortCriteria(true));
        }

        [TestMethod]
        public void CreateCalendar() {
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", testLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);

            var calendarRetrieved = repository.GetCalendar(calendarId);

            Assert.AreEqual(calendarId, calendarRetrieved.CalendarId);
            Assert.AreEqual(calendarToCreate.Name, calendarRetrieved.Name);
            Assert.AreEqual(calendarToCreate.Description, calendarRetrieved.Description);
            Assert.AreEqual(calendarToCreate.LibraryId, calendarRetrieved.LibraryId);
            Assert.AreEqual(calendarToCreate.DateFormat, calendarRetrieved.DateFormat);
            Assert.IsNotNull(calendarRetrieved.UniqueId);
        }

        [TestMethod]
        public void TestUpdateCalendar() {
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", testLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);

            var calendarRetrieved = repository.GetCalendar(calendarId);
            calendarRetrieved.Description += "1";
            calendarRetrieved.Name += "2";
            calendarRetrieved.DateFormat += "[TEST]";
            repository.UpdateCalendar(calendarRetrieved);

            var updatedCalendarRetrieved = repository.GetCalendar(calendarId);

            Assert.AreEqual(calendarRetrieved.CalendarId, updatedCalendarRetrieved.CalendarId);
            Assert.AreEqual(calendarRetrieved.Name, updatedCalendarRetrieved.Name);
            Assert.AreEqual(calendarRetrieved.Description, updatedCalendarRetrieved.Description);
            Assert.AreEqual(calendarRetrieved.LibraryId, updatedCalendarRetrieved.LibraryId);
            Assert.AreEqual(calendarRetrieved.DateFormat, updatedCalendarRetrieved.DateFormat);
            CollectionAssert.AreEquivalent(new List<Calendar>() { updatedCalendarRetrieved }, repository.GetCalendars(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void TestUpdateCalendar_ShouldNotUpdateDifferentCalendar() {
            var calendarDto = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", testLibrary.LibraryId);

            var calendarToUpdateId = repository.CreateCalendar(calendarDto);
            var calendarNotUpdatedId = repository.CreateCalendar(calendarDto);

            var calendarToUpdate = repository.GetCalendar(calendarToUpdateId);
            calendarToUpdate.Description += "1";
            repository.UpdateCalendar(calendarToUpdate);

            var calendarToNotUpdate = repository.GetCalendar(calendarNotUpdatedId);

            Assert.AreNotEqual(calendarToUpdate.Description, calendarToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetCalendarsWithNoneCreated() {
            var calendars = repository.GetCalendars(GetFirstPage()).Result.Results;
            Assert.AreEqual(0, calendars.Count);
        }

        [TestMethod]
        public void TestGetCalendarsWithOneCreated() {
            var calendar = CreateAndRetrieveCalendar();
            var calendars = repository.GetCalendars(GetFirstPage()).Result.Results;

            var expectedCalendars = new List<Calendar>();
            expectedCalendars.Add(calendar);

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void TestGetCalendarsWithMultipleCreated() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 5; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                expectedCalendars.Add(calendar);
            }

            var calendars = repository.GetCalendars(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void GetCalendars_ShouldntReturnDeletedCalendars() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 5; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                expectedCalendars.Add(calendar);
            }
            var deletedCalendar = CreateAndRetrieveCalendar();
            repository.DeleteCalendar(deletedCalendar.CalendarId);

            var calendars = repository.GetCalendars(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void GetDeletedCalendars_ShouldOnlyReturnDeletedCalendars() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 5; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                repository.DeleteCalendar(calendar.CalendarId);
                expectedCalendars.Add(calendar);
            }
            CreateAndRetrieveCalendar();

            var calendars = repository.GetDeletedCalendars(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void ShouldNotDeleteCalendar_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", otherLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);
            var calendarRetrieved = repository.GetCalendar(calendarId);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Calendar>(), repository.GetDeletedCalendars(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Calendar>() { calendarRetrieved }, repository.GetCalendars(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteCalendar_WhenLibraryRestored() {
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", testLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var calendarRetrieved = repository.GetCalendar(calendarId);

            CollectionAssert.AreEquivalent(new List<Calendar>(), repository.GetDeletedCalendars(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Calendar>() { calendarRetrieved }, repository.GetCalendars(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteCalendar_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", otherLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);

            repository.DeleteCalendar(calendarId);
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var calendarRetrieved = repository.GetCalendar(calendarId);

            CollectionAssert.AreEquivalent(new List<Calendar>() { calendarRetrieved }, repository.GetDeletedCalendars(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Calendar>(), repository.GetCalendars(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreCalendarDeletedNormally_WhenLibraryRestored() {
            var calendarToCreate = new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", testLibrary.LibraryId);
            var calendarId = repository.CreateCalendar(calendarToCreate);
            repository.DeleteCalendar(calendarId);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var calendarRetrieved = repository.GetCalendar(calendarId);

            CollectionAssert.AreEquivalent(new List<Calendar>() { calendarRetrieved }, repository.GetDeletedCalendars(GetFirstPage()).Result.Results);
            CollectionAssert.AreEquivalent(new List<Calendar>(), repository.GetCalendars(GetFirstPage()).Result.Results);
        }

        [TestMethod]
        public void GetCalendarsInLibrary_ShouldntReturnDeletedCalendars() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 3; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                expectedCalendars.Add(calendar);
            }
            var deletedCalendar = CreateAndRetrieveCalendar();
            repository.DeleteCalendar(deletedCalendar.CalendarId);

            var calendars = repository.GetCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void GetCalendarsInLibrary_ShouldntReturnCalendarsInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            repository.CreateCalendar(new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", otherLibrary.LibraryId));

            var calendars = repository.GetCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Calendar>(), calendars);
        }

        [TestMethod]
        public void GetDeletedCalendarsInLibrary_ShouldOnlyReturnDeletedCalendars() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 3; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                repository.DeleteCalendar(calendar.CalendarId);
                expectedCalendars.Add(calendar);
            }
            CreateAndRetrieveCalendar();

            var calendars = repository.GetDeletedCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void GetDeletedCalendarsInLibrary_ShouldntReturnCalendarsInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var calendarId = repository.CreateCalendar(new CreateCalendarDto("MM-DD-YYYY", "Test library", "test desc", otherLibrary.LibraryId));
            repository.GetCalendar(calendarId);
            repository.DeleteCalendar(calendarId);

            var calendars = repository.GetDeletedCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Calendar>(), calendars);
        }

        [TestMethod]
        public void RestoreDeleteCalendar_ShouldUndeleteCalendar() {
            var expectedCalendars = new List<Calendar>();
            for (int i = 0; i < 2; ++i) {
                var calendar = CreateAndRetrieveCalendar();
                expectedCalendars.Add(calendar);
            }
            var deletedCalendar = CreateAndRetrieveCalendar();
            expectedCalendars.Add(deletedCalendar);
            repository.DeleteCalendar(deletedCalendar.CalendarId);
            repository.RestoreDeletedCalendar(deletedCalendar.CalendarId);

            var calendars = repository.GetCalendars(GetFirstPage()).Result.Results;

            CollectionAssert.AreEquivalent(expectedCalendars, calendars);
        }

        [TestMethod]
        public void DeleteCalendar_ShouldRemoveFromSeries() {
            var seriesRepository = new SeriesRepository();
            var deletedCalendar = CreateAndRetrieveCalendar();
            var notDeletedCalendar = CreateAndRetrieveCalendar();

            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("", "", -1, "", -1, testLibrary.LibraryId, deletedCalendar.CalendarId));
            var series2Id = seriesRepository.CreateSeries(new CreateSeriesDto("", "", -1, "", -1, testLibrary.LibraryId, notDeletedCalendar.CalendarId));

            repository.DeleteCalendar(deletedCalendar.CalendarId);

            var series = seriesRepository.GetSeries(seriesId).Result;
            var series2 = seriesRepository.GetSeries(series2Id).Result;

            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, series.CalendarId);
            Assert.AreEqual(notDeletedCalendar.CalendarId, series2.CalendarId);
        }

        [TestMethod]
        public void DeleteCalendar_ShouldRemoveFromCharacter() {
            var characterRepository = new CharacterRepository();
            var deletedCalendar = CreateAndRetrieveCalendar();
            var notDeletedCalendar = CreateAndRetrieveCalendar();

            var dto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            dto.CalendarId = deletedCalendar.CalendarId;
            var characterId = characterRepository.CreateCharacter(dto);
            dto.CalendarId = notDeletedCalendar.CalendarId;
            var character2Id = characterRepository.CreateCharacter(dto);

            repository.DeleteCalendar(deletedCalendar.CalendarId);

            var character = characterRepository.GetCharacter(characterId);
            var character2 = characterRepository.GetCharacter(character2Id);

            Assert.AreEqual(DatabaseConstants.DEFAULT_ID, character.CalendarId);
            Assert.AreEqual(notDeletedCalendar.CalendarId, character2.CalendarId);
        }

        [TestMethod]
        public void UpsertCalendars_ShouldInsertNewItems() {
            var calendars = new List<Calendar>();

            for (int i = 0; i < 3; ++i) {
                var calendar = new Calendar(-1, "DD-MM-YYYY", "test: " + i, "test desc", testLibrary.LibraryId, false, UniqueIdUtil.GenerateUniqueId());
                calendars.Add(calendar);
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertCalendars(calendars, ids);
            var retCalendars = repository.GetCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var calendar in retCalendars) {
                expectedIds[calendar.UniqueId] = calendar.CalendarId;
            }

            CollectionAssert.AreEquivalent(calendars, retCalendars);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertCalendars_ShouldUpdateExistingItems() {
            var calendars = new List<Calendar>();

            for (int i = 0; i < 3; ++i) {
                var calendar = new Calendar(-1, "DD-MM-YYYY", "test: " + i, "test desc", testLibrary.LibraryId, false, UniqueIdUtil.GenerateUniqueId());
                calendars.Add(calendar);
            }

            repository.UpsertCalendars(calendars, new Dictionary<string, long>());
            calendars[0].Name = "new 0";
            calendars[2].Name = "new 2";
            repository.UpsertCalendars(calendars, new Dictionary<string, long>());
            var retCalendars = repository.GetCalendarsInLibrary(testLibrary.LibraryId, GetFirstPage(), "").Result.Results;

            CollectionAssert.AreEquivalent(calendars, retCalendars);
        }
    }
}

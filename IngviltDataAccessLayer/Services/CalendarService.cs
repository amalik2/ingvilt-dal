using Ingvilt.Constants;
using Ingvilt.Core;
using Ingvilt.Dto.Calendars;
using Ingvilt.Models.DataAccess;
using Ingvilt.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class CalendarService {
        private CalendarRepository calendarRepository;

        public CalendarService() {
            calendarRepository = DependencyInjectionContainer.Container.Resolve<CalendarRepository>();
        }

        public async Task<PaginationResult<Calendar>> GetCalendars(Pagination pagination) {
            return await calendarRepository.GetCalendars(pagination);
        }

        public async Task<PaginationResult<Calendar>> GetDeletedCalendars(Pagination pagination) {
            return await calendarRepository.GetDeletedCalendars(pagination);
        }

        public async Task<PaginationResult<Calendar>> GetCalendarsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await calendarRepository.GetCalendarsInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<PaginationResult<Calendar>> GetDeletedCalendarsInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            return await calendarRepository.GetDeletedCalendarsInLibrary(libraryId, pagination, nameFilter);
        }

        public async Task<Calendar> GetCalendar(long calendarId) {
            if (calendarId == DatabaseConstants.DEFAULT_ID) {
                return Calendar.GetRealWorldCalendar();
            }

            return calendarRepository.GetCalendar(calendarId);
        }

        public long CreateCalendar(CreateCalendarDto dto) {
            return calendarRepository.CreateCalendar(dto);
        }

        public Calendar CreateAndRetrieveCalendar(CreateCalendarDto dto) {
            long libraryId = CreateCalendar(dto);
            return new Calendar(libraryId, dto);
        }

        public async Task DeleteCalendar(Calendar calendar) {
            calendarRepository.DeleteCalendar(calendar.CalendarId);
        }

        public void UpdateCalendar(Calendar calendar) {
            calendarRepository.UpdateCalendar(calendar);
        }

        public async Task RestoreCalendar(Calendar calendar) {
            calendarRepository.RestoreDeletedCalendar(calendar.CalendarId);
        }

        public async Task PermanentlyRemoveCalendar(Calendar calendar) {
            calendarRepository.PermanentlyRemoveCalendar(calendar.CalendarId);
        }

        public async Task<List<CalendarBasicDetails>> GetAllActiveCalendarsInLibrary(long libraryId) {
            var calendars = calendarRepository.GetAllActiveCalendarsInLibrary(libraryId);
            calendars.Insert(0, CalendarBasicDetails.REAL_WORLD_CALENDAR);
            return calendars;
        }
    }
}

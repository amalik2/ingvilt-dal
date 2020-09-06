using Ingvilt.Constants;
using Ingvilt.Dto.Calendars;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class CalendarRepository {
        private static readonly string SELECT_BASE = "SELECT calendar_id, date_format, name, description, library_id, deleted, unique_id FROM calendar";

        private Calendar ParseCalendar(SqliteDataReader query) {
            return new Calendar(query.GetInt64(0), query.GetString(1), query.GetString(2), query.GetString(3), QueryUtil.GetNullableId(query, 4), query.GetBoolean(5), query.GetString(6));
        }

        private void UpdateDeletedStatus(long calendarId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE calendar SET deleted = {deleted}, deletion_due_to_cascade = false WHERE calendar_id = {calendarId}", db);
                command.ExecuteNonQuery();
            }
        }

        private async Task<PaginationResult<Calendar>> GetCalendars(Pagination pagination, bool deleted, string nameFilter = "", long libraryId = DatabaseConstants.DEFAULT_ID) {
            var libraryClause = libraryId == DatabaseConstants.DEFAULT_ID ? "" : $"library_id = {libraryId} AND";
            var query = $"{SELECT_BASE} WHERE {libraryClause} deleted = {deleted} AND name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseCalendar);
        }

        private SqliteCommand GetCreateCalendarCommand(SqliteConnection db, CreateCalendarDto dto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"INSERT {ignoreClause} INTO calendar(name, date_format, description, library_id, deleted, deletion_due_to_cascade, unique_id) VALUES(@Name, @DateFormat, @Description, @LibraryId, false, false, @UniqueId)";
            command.Parameters.AddWithValue("@Name", dto.Name);
            command.Parameters.AddWithValue("@DateFormat", dto.DateFormat);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@LibraryId", dto.LibraryId);
            command.Parameters.AddWithValue("@UniqueId", guid);
            return command;
        }

        public long CreateCalendar(CreateCalendarDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetCreateCalendarCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateCalendarCommand(SqliteConnection db, Calendar calendar, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE calendar SET name = @Name, date_format = @DateFormat, description = @Description, library_id = @LibraryId WHERE {idColumn} = @CalendarId";
            command.Parameters.AddWithValue("@Name", calendar.Name);
            command.Parameters.AddWithValue("@DateFormat", calendar.DateFormat);
            command.Parameters.AddWithValue("@Description", calendar.Description);
            command.Parameters.AddWithValue("@LibraryId", calendar.LibraryId);
            command.Parameters.AddWithValue("@CalendarId", idValue);
            return command;
        }

        public void UpdateCalendar(Calendar calendar) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdateCalendarCommand(db, calendar, "calendar_id", calendar.CalendarId);
                command.ExecuteNonQuery();
            }
        }

        private Calendar GetCalendar(object calendarId, string idColumnName, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"{SELECT_BASE} WHERE {idColumnName} = @CalendarId", db, txn);
            command.Parameters.AddWithValue("@CalendarId", calendarId);
            var query = command.ExecuteReader();
            query.Read();
            return ParseCalendar(query);
        }

        public Calendar GetCalendar(long calendarId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                return GetCalendar(calendarId, "calendar_id", db, null);
            }
        }

        public async Task<PaginationResult<Calendar>> GetCalendars(Pagination pagination) {
            return await GetCalendars(pagination, false);
        }

        public async Task<PaginationResult<Calendar>> GetDeletedCalendars(Pagination pagination) {
            return await GetCalendars(pagination, true);
        }

        public async Task<PaginationResult<Calendar>> GetCalendarsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetCalendars(pagination, false, nameFilter, libraryId);
        }

        public async Task<PaginationResult<Calendar>> GetDeletedCalendarsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetCalendars(pagination, true, nameFilter, libraryId);
        }

        public void DeleteCalendar(long calendarId) {
            UpdateDeletedStatus(calendarId, true);
        }

        public void PermanentlyRemoveCalendar(long calendarId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM calendar WHERE calendar_id = {calendarId}", db);
                command.ExecuteNonQuery();
            }
        }

        public void RestoreDeletedCalendar(long calendarId) {
            UpdateDeletedStatus(calendarId, false);
        }

        public List<CalendarBasicDetails> GetAllActiveCalendarsInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT calendar_id, name, date_format FROM calendar WHERE library_id = {libraryId} AND deleted = false", db);
                var query = command.ExecuteReader();
                var items = new List<CalendarBasicDetails>();

                while (query.Read()) {
                    items.Add(new CalendarBasicDetails(query.GetInt64(0), query.GetString(1), query.GetString(2)));
                }

                return items;
            }
        }

        public int GetNumberOfCalendarsInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM calendar WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task UpsertCalendars(List<Calendar> calendars, Dictionary<string, long> calendarIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var calendar in calendars) {
                        var command = GetUpdateCalendarCommand(db, calendar, "unique_id", calendar.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = calendar.GetCreateCalendarDto();
                        command = GetCreateCalendarCommand(db, createDto, calendar.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var retreivedCalendar = GetCalendar(calendar.UniqueId, "unique_id", db, txn);
                        calendar.CalendarId = retreivedCalendar.CalendarId;
                        calendarIds[calendar.UniqueId] = calendar.CalendarId;
                    }

                    txn.Commit();
                }
            }
        }
    }
}

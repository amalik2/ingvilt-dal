using Ingvilt.Constants;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search.SeriesNS;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class SeriesRepository {
        private static readonly string SELECT_BASE_COLUMNS_STRING = "s.series_id, s.name, s.site_url, s.logo_image, s.description, s.publisher_id, s.library_id, s.calendar_id, s.deleted, s.unique_id, s.worth_watching";
        private static readonly string SELECT_BASE = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM series s";

        private Series ParseSeries(SqliteDataReader query) {
            return new Series(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetNullableId(query, 3), query.GetString(4), QueryUtil.GetNullableId(query, 5), query.GetInt64(6), QueryUtil.GetNullableId(query, 7), query.GetBoolean(8), query.GetString(9), query.GetBoolean(10));
        }

        private void UpdateDeletedStatus(long seriesId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE series SET deleted = {deleted}, deletion_due_to_cascade = false WHERE series_id = {seriesId}", db);
                command.ExecuteNonQuery();
            }
        }

        private async Task<PaginationResult<Series>> GetAllSeries(Pagination pagination, bool deleted) {
            var command = $"{SELECT_BASE} WHERE deleted = {deleted}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseSeries);
        }

        private async Task<PaginationResult<Series>> GetSeriesInLibrary(Pagination pagination, bool deleted, long libraryId, string nameFilter) {
            var query = $"{SELECT_BASE} WHERE library_id = {libraryId} AND deleted = {deleted} AND name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseSeries);
        }

        private async Task<PaginationResult<Series>> GetSeriesByPublisher(Pagination pagination, bool deleted, long publisherId) {
            var command = $"{SELECT_BASE} WHERE publisher_id = {publisherId} AND deleted = {deleted}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseSeries);
        }

        private SqliteCommand GetCreateSeriesCommand(SqliteConnection db, CreateSeriesDto dto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"INSERT {ignoreClause} INTO series(name, site_url, logo_image, description, publisher_id, library_id, deleted, deletion_due_to_cascade, calendar_id, unique_id, worth_watching) VALUES(@Name, @Url, @LogoId, @Description, @PublisherId, @LibraryId, false, false, @CalendarId, @UniqueId, @WorthWatching)";
            command.Parameters.AddWithValue("@Name", dto.Name);
            command.Parameters.AddWithValue("@Url", dto.SiteURL);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(dto.PublisherId));
            command.Parameters.AddWithValue("@LibraryId", dto.LibraryId);
            command.Parameters.AddWithValue("@LogoId", QueryUtil.GetNullableIdForStorage(dto.LogoFileId));
            command.Parameters.AddWithValue("@CalendarId", QueryUtil.GetNullableIdForStorage(dto.CalendarId));
            command.Parameters.AddWithValue("@UniqueId", guid);
            command.Parameters.AddWithValue("@WorthWatching", dto.WorthWatching);
            return command;
        }

        public long CreateSeries(CreateSeriesDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetCreateSeriesCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateSeriesCommand(SqliteConnection db, Series series, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE series SET name = @Name, site_url = @Url, logo_image = @LogoId, description = @Description, publisher_id = @PublisherId, library_id = @LibraryId, calendar_id = @CalendarId, worth_watching = @WorthWatching WHERE {idColumn} = @SeriesId";
            command.Parameters.AddWithValue("@Name", series.Name);
            command.Parameters.AddWithValue("@Url", series.SiteURL);
            command.Parameters.AddWithValue("@Description", series.Description);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(series.PublisherId));
            command.Parameters.AddWithValue("@LibraryId", series.LibraryId);
            command.Parameters.AddWithValue("@LogoId", QueryUtil.GetNullableIdForStorage(series.LogoFileId));
            command.Parameters.AddWithValue("@SeriesId", idValue);
            command.Parameters.AddWithValue("@CalendarId", QueryUtil.GetNullableIdForStorage(series.CalendarId));
            command.Parameters.AddWithValue("@WorthWatching", series.WorthWatching);
            return command;
        }

        public void UpdateSeries(Series series) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdateSeriesCommand(db, series, "series_id", series.SeriesId);
                command.ExecuteNonQuery();
            }
        }

        public async Task<Series> GetSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = $"{SELECT_BASE} WHERE series_id = {seriesId}";
                var query = command.ExecuteReader();
                query.Read();
                return ParseSeries(query);
            }
        }

        public async Task<PaginationResult<Series>> GetSeries(Pagination pagination) {
            return await GetAllSeries(pagination, false);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeries(Pagination pagination) {
            return await GetAllSeries(pagination, true);
        }

        public async Task<PaginationResult<Series>> GetSeriesInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetSeriesInLibrary(pagination, false, libraryId, nameFilter);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeriesInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetSeriesInLibrary(pagination, true, libraryId, nameFilter);
        }

        public async Task<PaginationResult<Series>> GetSeriesByPublisher(long publisherId, Pagination pagination) {
            return await GetSeriesByPublisher(pagination, false, publisherId);
        }

        public async Task<PaginationResult<Series>> GetDeletedSeriesByPublisher(long publisherId, Pagination pagination) {
            return await GetSeriesByPublisher(pagination, true, publisherId);
        }

        public async Task DeleteSeries(long seriesId) {
            UpdateDeletedStatus(seriesId, true);
        }

        public async Task PermanentlyRemoveSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM series WHERE series_id = {seriesId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task RestoreDeletedSeries(long seriesId) {
            UpdateDeletedStatus(seriesId, false);
        }

        public async Task<List<SeriesBasicDetails>> GetAllActiveSeriesInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT series_id, name, publisher_id FROM series WHERE library_id = {libraryId} AND deleted = false", db);
                var query = command.ExecuteReader();
                var items = new List<SeriesBasicDetails>();

                while (query.Read()) {
                    items.Add(new SeriesBasicDetails(query.GetInt64(0), query.GetString(1), QueryUtil.GetNullableId(query, 2)));
                }

                return items;
            }
        }

        public async Task<Series> GetVideoSeries(Video video) {
            if (video.SeriesId == DatabaseConstants.DEFAULT_ID) {
                return null;
            }

            return await GetSeries(video.SeriesId);
        }

        public async Task<SeriesBasicDetails> GetSeriesBasicDetails(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT series_id, name, publisher_id FROM series WHERE series_id = {seriesId} AND deleted = false", db);
                var query = command.ExecuteReader();

                query.Read();
                return new SeriesBasicDetails(query.GetInt64(0), query.GetString(1), QueryUtil.GetNullableId(query, 2));
            }
        }

        public async Task<int> GetNumberOfSeriesInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM series WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<int> GetPercentageOfVideosWatchedInSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT SUM(CASE WHEN v.times_watched > 0 THEN 1 ELSE 0 END), COUNT(*) FROM video v WHERE v.series_id = {seriesId} AND v.deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                if (reader.IsDBNull(0)) {
                    return 0;
                }

                var watchedVideos = reader.GetInt32(0);
                var totalVideos = reader.GetInt32(1);
                if (totalVideos == 0) {
                    return 0;
                }

                var percentWatched = (100 * watchedVideos) / ((double)totalVideos);
                return Convert.ToInt32(Math.Floor(percentWatched));
            }
        }

        public async Task<int> GetNumberOfFinishedSeriesInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var finishedSeriesSubquery = $"SELECT series_id, SUM(CASE WHEN times_watched > 0 THEN 1 ELSE 0 END) AS watched_count, COUNT(*) AS total_count FROM video WHERE library_id = {libraryId} AND deleted = false GROUP BY series_id HAVING watched_count = total_count";
                
                var command = new SqliteCommand($"SELECT COUNT(*) FROM ({finishedSeriesSubquery})", db);
                var reader = command.ExecuteReader();
                reader.Read();
                var finishedSeriesWithVideos = reader.GetInt32(0);

                // TODO: is this worth including?
                /*command = new SqliteCommand($"SELECT COUNT(*) FROM series s WHERE library_id = {libraryId} AND deleted = false AND NOT EXISTS (SELECT v.series_id FROM video v WHERE v.series_id = s.series_id AND v.deleted = false)", db);
                reader = command.ExecuteReader();
                reader.Read();
                var seriesWithoutVideos = reader.GetInt32(0);*/
                var seriesWithoutVideos = 0;

                return finishedSeriesWithVideos + seriesWithoutVideos;
            }
        }

        private async Task<PaginationResult<Series>> GetSeriesCharacterIsIn(long characterId, string characterIdColumnName, Pagination pagination) {
            var columns = SELECT_BASE_COLUMNS_STRING.Replace("s.series_id", "DISTINCT(s.series_id)");
            var query = $"SELECT {columns} FROM series s, actor_for_video_character a, video v WHERE s.deleted = false AND v.deleted = false AND a.video_id = v.video_id AND v.series_id = s.series_id AND a.{characterIdColumnName} = {characterId}";
            var secondQuery = "";
            if (characterIdColumnName == "creator_id") {
                secondQuery = $"UNION SELECT {columns} FROM series s, video_creator vc, video v WHERE s.deleted = false AND v.deleted = false AND vc.video_id = v.video_id AND v.series_id = s.series_id AND vc.creator_id = {characterId}";
            }

            return await DataAccessUtil.GetPaginatedResult(pagination, $"{query} {secondQuery}", ParseSeries);
        }

        public async Task<List<Series>> GetSeriesCalendarIsUsedIn(long calendarId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{SELECT_BASE} WHERE s.calendar_id = {calendarId} AND s.deleted = false", db);
                var reader = command.ExecuteReader();
                var items = new List<Series>();

                while (reader.Read()) {
                    items.Add(ParseSeries(reader));
                }

                return items;
            }
        }

        public async Task<PaginationResult<Series>> GetSeriesCharacterIsIn(long characterId, Pagination pagination) {
            return await GetSeriesCharacterIsIn(characterId, "character_id", pagination);
        }

        public async Task<PaginationResult<Series>> GetSeriesFeaturingCreator(long creatorId, Pagination pagination) {
            return await GetSeriesCharacterIsIn(creatorId, "creator_id", pagination);
        }

        public async Task<int> GetNumberOfSeriesByPublisher(long publisherId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM series WHERE publisher_id = {publisherId} AND (deleted = false OR deletion_due_to_cascade = true)", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<List<Series>> GetRecentlyWatchedSeriesInLibrary(long libraryId, int count) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var columns = SELECT_BASE_COLUMNS_STRING.Replace("s.series_id", "DISTINCT(s.series_id)");
                var command = new SqliteCommand($"SELECT {columns} FROM series s, video v, watch_history w WHERE v.video_id = w.video_id AND v.series_id = s.series_id AND v.library_id = {libraryId} AND v.deleted = false AND s.library_id = {libraryId} AND s.deleted = false ORDER BY w.history_id DESC LIMIT {count}", db);
                var reader = command.ExecuteReader();
                var list = new List<Series>();

                while (reader.Read()) {
                    list.Add(ParseSeries(reader));
                }

                return list;
            }
        }

        public async Task<SeriesRatingDto> GetSeriesRating(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT AVG(CASE WHEN v.user_rating = 0 THEN NULL ELSE v.user_rating END), AVG(CASE WHEN v.external_rating = 0 THEN NULL ELSE v.external_rating END) FROM video v WHERE v.series_id = {seriesId} AND (v.deleted = false OR v.deletion_due_to_cascade = true)", db);
                var reader = command.ExecuteReader();

                if (!reader.Read()) {
                    return new SeriesRatingDto(0, 0);
                }

                var userRating = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                var externalRating = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                return new SeriesRatingDto(userRating, externalRating);
            }
        }

        public async Task<PaginationResult<Series>> SearchForSeries(Pagination pagination, long libraryId, List<ISeriesSearchQueryGenerator> subqueryGenerators) {
            if (subqueryGenerators.Count == 0) {
                return PaginationResult<Series>.CreateResultFromCurrentPage(new List<Series>(), pagination);
            }

            var subqueries = subqueryGenerators.Select((s) => {
                return s.GetSearchQuery();
            });
            var tables = string.Join(" INTERSECT ", subqueries);

            var command = $"{SELECT_BASE}, ({tables}) AS si WHERE s.series_id = si.series_id AND s.deleted = false AND s.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseSeries);
        }

        private Series GetSeries(string seriesId, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.Transaction = txn;
            command.CommandText = SELECT_BASE + " WHERE unique_id = @SeriesId";
            command.Parameters.AddWithValue("@SeriesId", seriesId);
            var query = command.ExecuteReader();
            query.Read();
            return ParseSeries(query);
        }

        public async Task UpsertSeries(List<ExportedSeriesSimpleDto> seriesList, Dictionary<string, long> seriesIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var s in seriesList) {
                        var series = s.Details;
                        var command = GetUpdateSeriesCommand(db, series, "unique_id", series.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = series.GetCreateSeriesDto();
                        command = GetCreateSeriesCommand(db, createDto, series.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var retrievedSeries = GetSeries(series.UniqueId, db, txn);
                        seriesIds[series.UniqueId] = retrievedSeries.SeriesId;
                        series.SeriesId = retrievedSeries.SeriesId;
                    }

                    txn.Commit();
                }
            }
        }

        public async Task<PaginationResult<Series>> GetSeriesToWatchInLibrary(Pagination pagination, long libraryId, string nameFilter) {
            var finishedSeriesNestedSubquery = $"SELECT series_id, SUM(CASE WHEN times_watched > 0 THEN 1 ELSE 0 END) AS watched_count, COUNT(*) AS total_count FROM video WHERE library_id = {libraryId} AND deleted = false GROUP BY series_id HAVING watched_count = total_count";
            var finishedSeriesSubquery = $"SELECT series_id FROM ({finishedSeriesNestedSubquery})";

            var query = $"{SELECT_BASE} WHERE library_id = {libraryId} AND deleted = false AND name LIKE @NameFilter AND worth_watching = true AND series_id NOT IN ({finishedSeriesSubquery})";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseSeries);
        }
    }
}

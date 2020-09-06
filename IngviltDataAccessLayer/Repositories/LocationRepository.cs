using Ingvilt.Dto.Export;
using Ingvilt.Dto.Locations;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class LocationRepository {
        private static readonly string SELECT_BASE = "SELECT l.location_id, l.name, l.description, l.library_id, l.publisher_id, l.cover_file_id, l.deleted, l.unique_id FROM location l";

        private Location ParseLocation(SqliteDataReader reader) {
            return new Location(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetInt64(3), QueryUtil.GetNullableId(reader, 4), QueryUtil.GetNullableId(reader, 5), reader.GetBoolean(6), reader.GetString(7));
        }

        private SqliteCommand GetCreateLocationCommand(SqliteConnection db, CreateLocationDto location, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand($"INSERT {ignoreClause} INTO location(name, description, library_id, deleted, deletion_due_to_cascade, publisher_id, cover_file_id, unique_id) VALUES (@Name, @Description, @LibraryId, false, false, @PublisherId, @CoverId, @UniqueId)", db);
            command.Parameters.AddWithValue("@Name", location.Name);
            command.Parameters.AddWithValue("@Description", location.Description);
            command.Parameters.AddWithValue("@LibraryId", location.LibraryId);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(location.PublisherId));
            command.Parameters.AddWithValue("@CoverId", QueryUtil.GetNullableIdForStorage(location.CoverFileId));
            command.Parameters.AddWithValue("@UniqueId", guid);
            return command;
        }

        public long CreateLocation(CreateLocationDto location) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetCreateLocationCommand(db, location, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateLocationCommand(SqliteConnection db, Location location, string idColumn, object idValue) {
            var command = new SqliteCommand($"UPDATE location SET name = @Name, description = @Description, library_id = @LibraryId, publisher_id = @PublisherId, cover_file_id = @CoverId WHERE {idColumn} = @LocationId", db);
            command.Parameters.AddWithValue("@Name", location.Name);
            command.Parameters.AddWithValue("@Description", location.Description);
            command.Parameters.AddWithValue("@LibraryId", location.LibraryId);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(location.PublisherId));
            command.Parameters.AddWithValue("@CoverId", QueryUtil.GetNullableIdForStorage(location.CoverFileId));
            command.Parameters.AddWithValue("@LocationId", idValue);
            return command;
        }

        public long UpdateLocation(Location location) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetUpdateLocationCommand(db, location, "location_id", location.LocationId);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private async Task UpdateDeletedStatus(long locationId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var tagCommand = new SqliteCommand($"UPDATE location SET deleted = {deleted}, deletion_due_to_cascade = false WHERE location_id = {locationId}", db);
                tagCommand.ExecuteNonQuery();
            }
        }

        public async Task DeleteLocation(Location location) {
            UpdateDeletedStatus(location.LocationId, true);
        }

        public async Task RestoreLocation(Location location) {
            UpdateDeletedStatus(location.LocationId, false);
        }

        public async Task DeleteLocation(long locationId) {
            UpdateDeletedStatus(locationId, true);
        }

        public async Task RestoreLocation(long locationId) {
            UpdateDeletedStatus(locationId, false);
        }

        public async Task PermanentlyRemoveLocation(Location location) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var tagCommand = new SqliteCommand($"DELETE FROM location WHERE location_id = @LocationId", db);
                tagCommand.Parameters.AddWithValue("@LocationId", location.LocationId);
                tagCommand.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<Location>> GetLocations(Pagination pagination, bool deleted = false) {
            var command = $"{SELECT_BASE} WHERE deleted = {deleted}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseLocation);
        }

        public async Task<PaginationResult<Location>> GetDeletedLocations(Pagination pagination) {
            return await GetLocations(pagination, true);
        }

        public Location GetLocation(long locationId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{SELECT_BASE} WHERE location_id = @LocationId", db);
                command.Parameters.AddWithValue("@LocationId", locationId);
                var reader = command.ExecuteReader();

                if (!reader.Read()) {
                    throw new ArgumentException($"There was no location with the id: {locationId}");
                }

                return ParseLocation(reader);
            }
        }

        private async Task<PaginationResult<Location>> GetLocations(Pagination pagination, bool deleted, long libraryId, string nameFilter) {
            var query = $"{SELECT_BASE} WHERE library_id = {libraryId} AND deleted = {deleted} AND name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseLocation);
        }

        public async Task<PaginationResult<Location>> GetLocationsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetLocations(pagination, false, libraryId, nameFilter);
        }

        public async Task<PaginationResult<Location>> GetDeletedLocationsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetLocations(pagination, true, libraryId, nameFilter);
        }

        public async Task<int> GetNumberOfLocationsInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM location WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<PaginationResult<Location>> GetLocationsVideoIsAt(long videoId, Pagination pagination) {
            var command = $"{SELECT_BASE}, video_location vl WHERE l.location_id = vl.location_id AND l.deleted = false AND vl.video_id = {videoId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseLocation);
        }

        public async Task<PaginationResult<Location>> GetLocationsForSelection(string locationTableName, string parentIdColumnName, long parentId, Pagination pagination) {
            var command = $"{SELECT_BASE} WHERE l.deleted = false AND l.location_id NOT IN (SELECT vl.location_id FROM {locationTableName} vl WHERE vl.{parentIdColumnName} = {parentId})";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseLocation);
        }

        public async Task AddLocationsToVideo(List<long> locationIds, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var baseCommand = "INSERT INTO video_location(location_id, video_id) VALUES";
                using (var txn = db.BeginTransaction()) {
                    foreach (var locationId in locationIds) {
                        var command = new SqliteCommand($"{baseCommand} ({locationId}, {videoId})", db, txn);
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public async Task RemoveLocationFromVideo(long locationId, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video_location WHERE video_id = {videoId} AND location_id = {locationId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<Location>> GetLocationsWithFile(Pagination pagination, long fileId) {
            var command = $"{SELECT_BASE}, location_media_file vl WHERE vl.location_id = l.location_id AND vl.media_id = {fileId} AND l.deleted = false UNION {SELECT_BASE} WHERE l.cover_file_id = {fileId} AND l.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseLocation);
        }

        private FileLocationExportDto ParseFileLocationExport(SqliteDataReader reader) {
            return new FileLocationExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<FileLocationExportDto>> GetAllFilesOnLocations(Pagination pagination, long libraryId) {
            var command = $"SELECT mf.unique_id, l.unique_id FROM location l, location_media_file lmf, media_file mf WHERE lmf.location_id = l.location_id AND lmf.media_id = mf.media_id AND l.deleted = false AND l.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseFileLocationExport);
        }

        private VideoLocationExportDto ParseVideoLocationExport(SqliteDataReader reader) {
            return new VideoLocationExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<VideoLocationExportDto>> GetAllVideosAtLocations(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, l.unique_id FROM location l, video_location vl, video v WHERE vl.location_id = l.location_id AND l.deleted = false AND l.library_id = {libraryId} AND v.video_id = vl.video_id AND v.deleted = false and v.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoLocationExport);
        }

        private Location GetLocation(string uniqueId, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"{SELECT_BASE} WHERE unique_id = @LocationId", db, txn);
            command.Parameters.AddWithValue("@LocationId", uniqueId);
            var reader = command.ExecuteReader();

            if (!reader.Read()) {
                throw new ArgumentException($"There was no location with the GUID: {uniqueId}");
            }

            return ParseLocation(reader);
        }

        public async Task UpsertLocations(List<ExportedLocationSimpleDto> locations, Dictionary<string, long> idsMap) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var loc in locations) {
                        var location = loc.Details;
                        var command = GetUpdateLocationCommand(db, location, "unique_id", location.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = location.GetCreateLocationDto();
                        command = GetCreateLocationCommand(db, createDto, location.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var retrievedCalendar = GetLocation(location.UniqueId, db, txn);
                        location.LocationId = retrievedCalendar.LocationId;
                        idsMap[location.UniqueId] = location.LocationId;
                    }

                    txn.Commit();
                }
            }
        }

        public async Task UpsertVideosAtLocations(List<VideoLocationExportDto> locations, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var l in locations) {
                        if (l.LocationId == null || l.VideoId == null) {
                            continue;
                        }

                        var videoId = ids[l.VideoId];
                        var locationId = ids[l.LocationId];

                        var command = new SqliteCommand($"INSERT OR IGNORE INTO video_location(location_id, video_id) VALUES ({locationId}, {videoId})", db, txn);
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }
    }
}

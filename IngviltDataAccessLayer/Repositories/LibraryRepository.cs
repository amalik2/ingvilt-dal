using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class LibraryRepository {
        private static readonly string SELECT_BASE = "SELECT library_id, name, background_image_file_id, deleted, unique_id FROM library";

        private void SetLibraryDeletedStatus(long libraryId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var deleteTableCommand = new SqliteCommand($"UPDATE library SET deleted = {deleted} WHERE library_id = {libraryId}", db);
                deleteTableCommand.ExecuteNonQuery();
            }
        }

        private Library ParseLibrary(SqliteDataReader query) {
            return new Library(query.GetInt64(0), query.GetString(1), QueryUtil.GetNullableId(query, 2), query.GetBoolean(3), query.GetString(4));
        }

        public async Task<PaginationResult<Library>> GetLibraries(Pagination pagination, string nameFilter, bool deleted = false) {
            var query = $"{SELECT_BASE} WHERE deleted = {deleted} AND name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseLibrary);
        }

        public async Task<PaginationResult<Library>> GetDeletedLibraries(Pagination pagination, string nameFilter) {
            return await GetLibraries(pagination, nameFilter, true);
        }

        public long CreateLibrary(CreateLibraryDto library, string guid = null) {
            if (guid == null) {
                guid = UniqueIdUtil.GenerateUniqueId();
            }

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                long createdLibraryId = -1;

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand();
                    command.Connection = db;
                    command.CommandText = "INSERT INTO library VALUES(NULL, @Name, @BackgroundId, false, @UniqueId)";
                    command.Parameters.AddWithValue("@Name", library.Name);
                    command.Parameters.AddWithValue("@BackgroundId", QueryUtil.GetNullableIdForStorage(library.BackgroundImageId));
                    command.Parameters.AddWithValue("@UniqueId", guid);
                    command.Transaction = txn;
                    command.ExecuteNonQuery();
                    createdLibraryId = QueryUtil.GetLastInsertedPrimaryKey(db, txn);

                    foreach (var setting in LibrarySettings.DEFAULT_SETTINGS) {
                        var createSettingsCommand = new SqliteCommand($"INSERT INTO library_setting VALUES({createdLibraryId}, '{setting.Key}', '{setting.Value}')", db, txn);
                        createSettingsCommand.ExecuteNonQuery();
                    }

                    txn.Commit();
                }

                return createdLibraryId;
            }
        }

        public Library GetLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = SELECT_BASE + " WHERE library_id = @LibraryId";
                command.Parameters.AddWithValue("@LibraryId", libraryId);
                var query = command.ExecuteReader();
                query.Read();

                return ParseLibrary(query);
            }
        }

        public LibrarySettings GetLibrarySettings(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = "SELECT name, value FROM library_setting WHERE library_id = @LibraryId";
                command.Parameters.AddWithValue("@LibraryId", libraryId);
                var query = command.ExecuteReader();

                Dictionary<string, string> settingPairs = new Dictionary<string, string>();
                while (query.Read()) {
                    var name = query.GetString(0);
                    var value = query.GetString(1);
                    settingPairs.Add(name, value);
                }

                return new LibrarySettings(settingPairs);
            }
        }

        public void UpdateLibrarySettings(long libraryId, LibrarySettings settings) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand($"UPDATE library_setting SET value = @Value WHERE library_id = {libraryId} AND name = @Key", db, txn);

                    command.Parameters.AddWithValue("@Key", nameof(settings.PublisherLabel));
                    command.Parameters.AddWithValue("@Value", settings.PublisherLabel);
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.SeriesLabel);
                    command.Parameters["@Value"].Value = settings.SeriesLabel;
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.CharacterLabel);
                    command.Parameters["@Value"].Value = settings.CharacterLabel;
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.VideoLabel);
                    command.Parameters["@Value"].Value = settings.VideoLabel;
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.VideoPreviewDateFormat);
                    command.Parameters["@Value"].Value = settings.VideoPreviewDateFormat;
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowCharacters);
                    command.Parameters["@Value"].Value = settings.ShowCharacters.ToString();
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowCreators);
                    command.Parameters["@Value"].Value = settings.ShowCreators.ToString();
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowLocations);
                    command.Parameters["@Value"].Value = settings.ShowLocations.ToString();
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowPublishers);
                    command.Parameters["@Value"].Value = settings.ShowPublishers.ToString();
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowSeries);
                    command.Parameters["@Value"].Value = settings.ShowSeries.ToString();
                    command.ExecuteNonQuery();

                    command.Parameters["@Key"].Value = nameof(settings.ShowVideos);
                    command.Parameters["@Value"].Value = settings.ShowVideos.ToString();
                    command.ExecuteNonQuery();

                    txn.Commit();
                }
            }
        }

        public void PermanentlyDeleteLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var deleteTableCommand = new SqliteCommand($"DELETE FROM library WHERE library_id = {libraryId}", db);
                deleteTableCommand.ExecuteNonQuery();
            }
        }

        public void DeleteLibrary(long libraryId) {
            SetLibraryDeletedStatus(libraryId, true);
        }

        public void RestoreDeletedLibrary(long libraryId) {
            SetLibraryDeletedStatus(libraryId, false);
        }

        private SqliteCommand GetUpdateLibraryCommand(SqliteConnection db, Library library) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE library SET name = @Name, background_image_file_id = @BackgroundId WHERE library_id = {library.LibraryId}";
            command.Parameters.AddWithValue("@Name", library.Name);
            command.Parameters.AddWithValue("@BackgroundId", QueryUtil.GetNullableIdForStorage(library.BackgroundImageId));
            return command;
        }

        public void UpdateLibrary(Library library) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetUpdateLibraryCommand(db, library);
                command.ExecuteNonQuery();
            }
        }

        public List<LibraryBasicDetails> GetAllActiveLibraries() {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT library_id, name FROM library WHERE deleted = false", db);
                var query = command.ExecuteReader();
                var items = new List<LibraryBasicDetails>();

                while (query.Read()) {
                    items.Add(new LibraryBasicDetails(query.GetInt64(0), query.GetString(1)));
                }

                return items;
            }
        }

        private long GetLibraryIdFromGUID(string uniqueId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT library_id FROM library WHERE unique_id = @UniqueId", db);
                command.Parameters.AddWithValue("@UniqueId", uniqueId);
                var query = command.ExecuteReader();

                if (!query.Read()) {
                    return DatabaseConstants.DEFAULT_ID;
                }

                return query.GetInt64(0);
            }
        }

        public long UpsertLibrary(CreateLibraryDto dto, string uniqueId, LibrarySettings settings) {
            var libraryId = GetLibraryIdFromGUID(uniqueId);
            if (libraryId == DatabaseConstants.DEFAULT_ID) {
                libraryId = CreateLibrary(dto, uniqueId);
            } else {
                var library = new Library(libraryId, dto.Name, dto.BackgroundImageId, false, uniqueId);
                UpdateLibrary(library);
            }

            if (settings != null) {
                UpdateLibrarySettings(libraryId, settings);
            }

            return libraryId;
        }
    }
}

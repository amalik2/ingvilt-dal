using Ingvilt.Constants;
using Ingvilt.Dto.Publishers;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class PublisherRepository {
        private static readonly string SELECT_BASE = "SELECT publisher_id, name, site_url, logo_image, description, library_id, deleted, unique_id FROM publisher";

        private Publisher ParsePublisher(SqliteDataReader query) {
            return new Publisher(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetNullableId(query, 3), query.GetString(4), query.GetInt64(5), query.GetBoolean(6), query.GetString(7));
        }

        private void UpdateDeletedStatus(long publisherId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE publisher SET deleted = {deleted}, deletion_due_to_cascade = false WHERE publisher_id = {publisherId}", db);
                command.ExecuteNonQuery();
            }
        }

        private async Task<PaginationResult<Publisher>> GetPublishers(Pagination pagination, bool deleted, long libraryId = DatabaseConstants.DEFAULT_ID, string nameFilter = "") {
            var libraryClause = libraryId == DatabaseConstants.DEFAULT_ID ? "" : $"library_id = {libraryId} AND";
            var query = $"{SELECT_BASE} WHERE {libraryClause} deleted = {deleted} AND name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParsePublisher);
        }

        private SqliteCommand GetCreatePublisherCommand(SqliteConnection db, CreatePublisherDto dto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"INSERT {ignoreClause} INTO publisher(name, site_url, logo_image, description, library_id, deleted, deletion_due_to_cascade, unique_id) VALUES(@Name, @Url, @LogoId, @Description, @LibraryId, false, false, @UniqueId)";
            command.Parameters.AddWithValue("@Name", dto.Name);
            command.Parameters.AddWithValue("@Url", dto.SiteURL);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@LibraryId", dto.LibraryId);
            command.Parameters.AddWithValue("@LogoId", QueryUtil.GetNullableIdForStorage(dto.LogoFileId));
            command.Parameters.AddWithValue("@UniqueId", guid);
            return command;
        }

        public long CreatePublisher(CreatePublisherDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetCreatePublisherCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdatePublisherCommand(SqliteConnection db, Publisher publisher, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE publisher SET name = @Name, site_url = @Url, logo_image = @LogoId, description = @Description, library_id = @LibraryId WHERE {idColumn} = @PublisherId";
            command.Parameters.AddWithValue("@Name", publisher.Name);
            command.Parameters.AddWithValue("@Url", publisher.SiteURL);
            command.Parameters.AddWithValue("@Description", publisher.Description);
            command.Parameters.AddWithValue("@LibraryId", publisher.LibraryId);
            command.Parameters.AddWithValue("@LogoId", QueryUtil.GetNullableIdForStorage(publisher.LogoFileId));
            command.Parameters.AddWithValue("@PublisherId", idValue);
            return command;
        }

        public void UpdatePublisher(Publisher publisher) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdatePublisherCommand(db, publisher, "publisher_id", publisher.PublisherId);
                command.ExecuteNonQuery();
            }
        }

        public async Task<Publisher> GetPublisher(long publisherId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = SELECT_BASE + " WHERE publisher_id = @PublisherId";
                command.Parameters.AddWithValue("@PublisherId", publisherId);
                var query = command.ExecuteReader();
                query.Read();
                return ParsePublisher(query);
            }
        }

        private Publisher GetPublisher(string uniqueId, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"{SELECT_BASE} WHERE unique_id = @UniqueId", db, txn);
            command.Parameters.AddWithValue("@UniqueId", uniqueId);
            var reader = command.ExecuteReader();

            if (!reader.Read()) {
                return null;
            }

            return ParsePublisher(reader);
        }

        public async Task<PaginationResult<Publisher>> GetPublishers(Pagination pagination) {
            return await GetPublishers(pagination, false);
        }

        public async Task<PaginationResult<Publisher>> GetDeletedPublishers(Pagination pagination) {
            return await GetPublishers(pagination, true);
        }

        public async Task<PaginationResult<Publisher>> GetPublishersInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetPublishers(pagination, false, libraryId, nameFilter);
        }

        public async Task<PaginationResult<Publisher>> GetDeletedPublishersInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetPublishers(pagination, true, libraryId, nameFilter);
        }

        public async Task DeletePublisher(long publisherId) {
           UpdateDeletedStatus(publisherId, true);
        }

        public async Task PermanentlyRemovePublisher(long publisherId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM publisher WHERE publisher_id = {publisherId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task RestoreDeletedPublisher(long publisherId) {
            UpdateDeletedStatus(publisherId, false);
        }

        public async Task<List<PublisherBasicDetails>> GetAllActivePublishersInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT publisher_id, name FROM publisher WHERE library_id = {libraryId} AND deleted = false", db);
                var query = command.ExecuteReader();
                var items = new List<PublisherBasicDetails>();

                while (query.Read()) {
                    items.Add(new PublisherBasicDetails(query.GetInt64(0), query.GetString(1)));
                }

                return items;
            }
        }

        public async Task<PublisherBasicDetails> GetPublisherBasicDetails(long publisherId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT publisher_id, name FROM publisher WHERE publisher_id = {publisherId} AND deleted = false", db);
                var query = command.ExecuteReader();

                query.Read();
                return new PublisherBasicDetails(query.GetInt64(0), query.GetString(1));
            }
        }

        public async Task<int> GetNumberOfPublishersInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM publisher WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task UpsertPublishers(List<ExportedPublisherSimpleDto> publishers, Dictionary<string, long> publisherIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var exportedPublisher in publishers) {
                        var publisher = exportedPublisher.Details;
                        var command = GetUpdatePublisherCommand(db, publisher, "unique_id", publisher.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = publisher.GetCreatePublisherDto();
                        command = GetCreatePublisherCommand(db, createDto, publisher.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var retrievedPublisher = GetPublisher(publisher.UniqueId, db, txn);
                        publisher.PublisherId = retrievedPublisher.PublisherId;
                        publisherIds[publisher.UniqueId] = publisher.PublisherId;
                    }

                    txn.Commit();
                }
            }
        }
    }
}

using Microsoft.Data.Sqlite;

using Ingvilt.Constants;
using Ingvilt.Repositories;
using Ingvilt.Core;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util.DataInit;

using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Ingvilt.Util {
    public class DataAccessUtil {
        private static PersistentSqliteConnection PERSISTENT_CONNECTION = null;

        private static GlobalSettingsRepository globalSettingsRepository;

        public static string DatabasePath {
            get;
            set;
        }

        public static bool UsePersistentConnection {
            get;
            set;
        }

        private static void InitGlobalSettings(SqliteConnection db) {
            globalSettingsRepository.CreateDefaultSettings(db);
        }

        static DataAccessUtil() {
            globalSettingsRepository = DependencyInjectionContainer.Container.Resolve<GlobalSettingsRepository>();
            UsePersistentConnection = false;
        }

        public static void InitDatabase() {
            if (DatabasePath != DatabaseConstants.IN_MEMORY_DB_PATH) {
                CommonFileUtil.EnsureFileExists(DatabasePath);
            }

            using (var db = CreateSqlConnection()) {
                db.Open();

                // Enable write-ahead logging
                var walCommand = db.CreateCommand();
                walCommand.CommandText = @"PRAGMA journal_mode = 'wal'";
                walCommand.ExecuteNonQuery();

                using (var txn = db.BeginTransaction()) {
                    DatabaseTablesUtil.InitTables(db);
                    TriggersUtil.InitTriggers(db);
                    IndexUtil.InitIndices(db);
                    InitGlobalSettings(db);

                    txn.Commit();
                }
            }
        }

        public static void ClearDatabase() {
            using (SqliteConnection db = CreateSqlConnection()) {
                db.Open();

                string[] tablesToDelete = {
                    "library",
                    "media_file",
                    "calendar",
                    "location",
                    "tag",
                    "playlist",
                    "video",
                    "series",
                    "global_setting",
                    "publisher",
                    "character"
                };

                using (SqliteTransaction txn = db.BeginTransaction()) {
                    foreach (var table in tablesToDelete) {
                        new SqliteCommand($"DELETE FROM {table}", db, txn).ExecuteReader();
                    }

                    txn.Commit();
                }
            }
        }

        public static SqliteConnection CreateSqlConnection() {
            var connString = $"Filename={DatabasePath}";
            if (UsePersistentConnection) {
                if (PERSISTENT_CONNECTION == null) {
                    PERSISTENT_CONNECTION = new PersistentSqliteConnection(connString);
                }

                return PERSISTENT_CONNECTION;
            }

            return new SqliteConnection(connString);
        }

        public static void ClosePersistentConnection() {
            if (PERSISTENT_CONNECTION != null) {
                PERSISTENT_CONNECTION.ForceClose();
                PERSISTENT_CONNECTION = null;
            }
        }

        public static async Task<PaginationResult<T>> GetPaginatedResult<T>(Pagination pagination, string baseQuery, Action<SqliteCommand> parameterizeQuery, Func<SqliteDataReader, T> parseRecord) {
            using (var db = CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{baseQuery} {pagination.GetQueryString()}", db);
                parameterizeQuery(command);

                var query = command.ExecuteReader();
                var records = new List<T>();
                while (query.Read()) {
                    records.Add(parseRecord(query));
                }
                return PaginationResult<T>.CreateResultFromCurrentPage(records, pagination);
            }
        }

        public static async Task<PaginationResult<T>> GetPaginatedResult<T>(Pagination pagination, string baseQuery, Func<SqliteDataReader, T> parseRecord) {
            return await GetPaginatedResult(pagination, baseQuery, (SqliteCommand command) => { }, parseRecord);
        }
    }
}

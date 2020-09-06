using Ingvilt.Constants;

using Microsoft.Data.Sqlite;

using System;

namespace Ingvilt.Util {
    public class QueryUtil {
        public static long GetLastInsertedPrimaryKey(SqliteConnection db) {
            var command = new SqliteCommand("select last_insert_rowid()", db);
            return (long)command.ExecuteScalar();
        }

        public static long GetLastInsertedPrimaryKey(SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand("select last_insert_rowid()", db, txn);
            return (long)command.ExecuteScalar();
        }

        public static long GetNullableId(SqliteDataReader reader, int columnIndex) {
            if (reader.IsDBNull(columnIndex)) {
                return DatabaseConstants.DEFAULT_ID;
            }

            return reader.GetInt64(columnIndex);
        }

        public static object GetNullableIdForStorage(long id) {
            if (id == DatabaseConstants.DEFAULT_ID) {
                return DBNull.Value;
            }

            return id;
        }

        public static object GetNullableValueForStorage(object value) {
            return value == null ? DBNull.Value : value;
        }

        public static Nullable<DateTime> GetDateTime(SqliteDataReader reader, int columnIndex) {
            if (reader.IsDBNull(columnIndex)) {
                return null;
            }

            return reader.GetDateTime(columnIndex).ToUniversalTime();
        }

        public static Nullable<int> GetInt32(SqliteDataReader reader, int columnIndex) {
            if (reader.IsDBNull(columnIndex)) {
                return null;
            }

            return reader.GetInt32(columnIndex);
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ingvilt.Util.DataInit {
    public class IndexUtil {
        private static int indexNumber = 1;

        private static void CreateIndex(SqliteConnection db, string tableAndColumns) {
            var commandText = $"CREATE INDEX IF NOT EXISTS Index{indexNumber} ON {tableAndColumns}";
            var command = db.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();

            indexNumber += 1;
        }

        public static void InitIndices(SqliteConnection db) {
            CreateIndex(db, "video(library_id)");
            CreateIndex(db, "character(library_id)");
            CreateIndex(db, "series(library_id)");
            CreateIndex(db, "publisher(library_id)");
            CreateIndex(db, "location(library_id)");
            CreateIndex(db, "calendar(library_id)");
            CreateIndex(db, "playlist(library_id)");

            CreateIndex(db, "series(publisher_id)");
            CreateIndex(db, "location(publisher_id)");
            CreateIndex(db, "video(publisher_id)");
            CreateIndex(db, "video(series_id)");
            CreateIndex(db, "series_sequence(series_id)");

            CreateIndex(db, "character(calendar_id)");
            CreateIndex(db, "series(calendar_id)");
        }
    }
}

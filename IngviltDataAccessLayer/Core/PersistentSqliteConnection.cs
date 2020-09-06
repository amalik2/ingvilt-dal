using Microsoft.Data.Sqlite;

namespace Ingvilt.Core {
    public class PersistentSqliteConnection : SqliteConnection {
        private bool isOpen;

        public PersistentSqliteConnection(string connection) : base(connection) {
            isOpen = false;
        }

        public override void Open() {
            if (isOpen) {
                return;
            }

            isOpen = true;
            base.Open();
        }

        public override void Close() {
        }

        public void ForceClose() {
            isOpen = false;
            base.Close();
        }
    }
}

using Microsoft.Data.Sqlite;

namespace Ingvilt.Util.DataInit {
    public class TriggersUtil {
        private static int triggerNumber = 1;

        private static string GetDeleteCascadeTrigger(string tableToUpdate, string tableToWatch, string parentId) {
            return $@"CREATE TRIGGER IF NOT EXISTS library_delete_cascade_{triggerNumber} AFTER UPDATE OF deleted ON {tableToWatch}
FOR EACH ROW
    BEGIN UPDATE {tableToUpdate}
        SET deleted = NEW.deleted, deletion_due_to_cascade = true
        WHERE {parentId} = NEW.{parentId}
        AND (deleted != NEW.deleted)
        AND (deletion_due_to_cascade = true OR NEW.deleted = true);
END;";
        }

        private static string GetVideoSeriesUpdateTrigger() {
            return $@"CREATE TRIGGER IF NOT EXISTS video_series_update_trigger AFTER UPDATE OF series_id ON video
FOR EACH ROW
    WHEN NEW.series_id IS NOT OLD.series_id
    BEGIN DELETE FROM video_in_sequence
        WHERE video_id = NEW.video_id
        AND sequence_id IN (
            SELECT sequence_id FROM series_sequence
            WHERE series_id = OLD.series_id
        );
END;";
        }

        // TODO: optimize this
        private static string GetVideoInSequenceDeletionTrigger() {
            return $@"CREATE TRIGGER IF NOT EXISTS video_sequence_deletion_trigger AFTER DELETE ON video_in_sequence
FOR EACH ROW
    BEGIN UPDATE video_in_sequence
        SET order_in_list = order_in_list - 1
        WHERE sequence_id = OLD.sequence_id
        AND order_in_list > OLD.order_in_list;
END;";
        }

        private static string GetSeriesPublisherUpdateTrigger() {
            return $@"CREATE TRIGGER IF NOT EXISTS series_publisher_update_trigger AFTER UPDATE OF publisher_id ON series
FOR EACH ROW
    WHEN NEW.publisher_id IS NOT OLD.publisher_id
    BEGIN UPDATE video
        SET publisher_id = NEW.publisher_id
        WHERE series_id = NEW.series_id;
END;";
        }

        private static string GetCalendarDeletion_UpdateSeriesTrigger() {
            return $@"CREATE TRIGGER IF NOT EXISTS calendar_deletion_update_series_trigger AFTER UPDATE OF deleted ON calendar
FOR EACH ROW
    WHEN NEW.deleted != OLD.deleted
    BEGIN UPDATE series
        SET calendar_id = NULL
        WHERE calendar_id = NEW.calendar_id;
END;";
        }

        private static string GetCalendarDeletion_UpdateCharacterTrigger() {
            return $@"CREATE TRIGGER IF NOT EXISTS calendar_deletion_update_character_trigger AFTER UPDATE OF deleted ON calendar
FOR EACH ROW
    BEGIN UPDATE character
        SET calendar_id = NULL
        WHERE calendar_id = NEW.calendar_id;
END;";
        }

        private static void CreateTrigger(SqliteConnection db, string triggerCommand) {
            var command = db.CreateCommand();
            command.CommandText = triggerCommand;
            command.ExecuteNonQuery();

            triggerNumber += 1;
        }

        public static void InitTriggers(SqliteConnection db) {
            CreateTrigger(db, GetDeleteCascadeTrigger("series", "publisher", "publisher_id"));
            CreateTrigger(db, GetDeleteCascadeTrigger("location", "publisher", "publisher_id"));
            CreateTrigger(db, GetDeleteCascadeTrigger("video", "publisher", "publisher_id"));
            CreateTrigger(db, GetDeleteCascadeTrigger("video", "series", "series_id"));

            CreateTrigger(db, GetVideoSeriesUpdateTrigger());
            CreateTrigger(db, GetVideoInSequenceDeletionTrigger());
            CreateTrigger(db, GetSeriesPublisherUpdateTrigger());

            CreateTrigger(db, GetCalendarDeletion_UpdateSeriesTrigger());
            CreateTrigger(db, GetCalendarDeletion_UpdateCharacterTrigger());
        }
    }
}

using Ingvilt.Constants;

using Microsoft.Data.Sqlite;

namespace Ingvilt.Util.DataInit {
    public class DatabaseTablesUtil {
        private static readonly string URL_TYPE = "NVARCHAR(2048)";
        private static readonly string ID_TYPE = "INTEGER";
        private static readonly string PRIMARY_KEY = $"{ID_TYPE} PRIMARY KEY AUTOINCREMENT";
        private static readonly string UUID_COLUMN = "unique_id CHARACTER(36) UNIQUE NOT NULL";

        private static string GetMediaFileTableDetails() {
            return $"media_file (media_id {PRIMARY_KEY}, source_url {URL_TYPE}, name NVARCHAR({MediaFileConstants.MAX_NAME_LENGTH}), file_type INTEGER, create_date DATETIME, {UUID_COLUMN})";
        }

        private static string GetLibraryTableDetails() {
            return $"library (library_id {PRIMARY_KEY}, name NVARCHAR({LibraryConstants.MAX_NAME_LENGTH}), background_image_file_id references media_file(media_id) ON DELETE SET NULL, deleted BOOLEAN, {UUID_COLUMN})";
        }

        private static string GetPublisherTableDetails() {
            return $"publisher (publisher_id {PRIMARY_KEY}, name NVARCHAR({PublisherConstants.MAX_NAME_LENGTH}), site_url {URL_TYPE} NULL, logo_image references media_file(media_id) ON DELETE SET NULL, description NVARCHAR({PublisherConstants.MAX_DESCRIPTION_LENGTH}), library_id references library(library_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, {UUID_COLUMN})";
        }

        private static string GetSeriesTableDetails() {
            return $"series (series_id {PRIMARY_KEY}, name NVARCHAR({SeriesConstants.MAX_NAME_LENGTH}), site_url {URL_TYPE} NULL, logo_image references media_file(media_id) ON DELETE SET NULL, description NVARCHAR({SeriesConstants.MAX_DESCRIPTION_LENGTH}), publisher_id {ID_TYPE} references publisher(publisher_id) ON DELETE CASCADE, library_id references library(library_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, calendar_id references calendar(calendar_id) ON DELETE SET NULL, {UUID_COLUMN}, worth_watching BOOLEAN)";
        }

        private static string GetTagsOnSeriesTableDetails() {
            return $"tag_on_series (tag_id references video_tag(tag_id) ON DELETE CASCADE, series_id references series(series_id) ON DELETE CASCADE, PRIMARY KEY(tag_id, series_id))";
        }

        private static string GetVideoSequenceTableDetails() {
            return $"video_sequence (sequence_id {PRIMARY_KEY}, title NVARCHAR({VideoSequenceConstants.MAX_NAME_LENGTH}), description NVARCHAR({VideoSequenceConstants.MAX_DESCRIPTION_LENGTH}), cover_file references media_file(media_id) ON DELETE SET NULL)";
        }

        private static string GetVideoTableDetails() {
            return $"video (video_id {PRIMARY_KEY}, title NVARCHAR({VideoConstants.MAX_NAME_LENGTH}), times_watched INTEGER NOT NULL, last_watch_date DATETIME, duration_in_seconds INTEGER, external_rating REAL, user_rating REAL, description NVARCHAR({VideoConstants.MAX_DESCRIPTION_LENGTH}), notes NVARCHAR({VideoConstants.MAX_NOTES_LENGTH}), source_url {URL_TYPE}, site_url {URL_TYPE}, series_id references series(series_id) ON DELETE CASCADE, watch_status INTEGER NOT NULL, global_rank INTEGER, publisher_id references publisher(publisher_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, library_id references library(library_id) ON DELETE CASCADE, release_date DATETIME, timeline_date DATETIME, cover_image references media_file(media_id) ON DELETE SET NULL, {UUID_COLUMN})";
        }

        private static string GetMediaFileForVideoTableDetails() {
            return $"video_media_file (video_id references video(video_id) ON DELETE CASCADE, media_id references media_file(media_id) ON DELETE CASCADE, PRIMARY KEY(video_id, media_id))";
        }

        private static string GetVideoInSequenceTableDetails() {
            return $"video_in_sequence (sequence_id references video_sequence(sequence_id) ON DELETE CASCADE, video_id references video(video_id) ON DELETE CASCADE, order_in_list INTEGER NOT NULL, PRIMARY KEY(sequence_id, video_id))";
        }

        private static string GetBaseTagTableDetails() {
            return $"tag (tag_id {PRIMARY_KEY}, type NVARCHAR({TagConstants.MAX_TYPE_LENGTH}), name NVARCHAR({TagConstants.MAX_NAME_LENGTH}), deleted BOOLEAN, unique_id {UUID_COLUMN})";
        }

        private static string GetVideoTagTableDetails() {
            return $"video_tag (tag_id references tag(tag_id) ON DELETE CASCADE, PRIMARY KEY(tag_id))";
        }

        private static string GetTagsOnVideosTableDetails() {
            return $"tag_on_video (tag_id references video_tag(tag_id) ON DELETE CASCADE, video_id references video(video_id) ON DELETE CASCADE, PRIMARY KEY(tag_id, video_id))";
        }

        private static string GetTagsOnMediaFilesTableDetails() {
            return $"tag_on_media_file (tag_id references video_tag(tag_id) ON DELETE CASCADE, media_file_id references media_file(media_id) ON DELETE CASCADE, PRIMARY KEY(tag_id, media_file_id))";
        }

        private static string GetLocationTableDetails() {
            return $"location (location_id {PRIMARY_KEY}, name NVARCHAR({LocationConstants.MAX_NAME_LENGTH}), description NVARCHAR({LocationConstants.MAX_DESCRIPTION_LENGTH}), library_id references library(library_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, publisher_id references publisher(publisher_id) ON DELETE CASCADE, cover_file_id references media_file(media_id) ON DELETE SET NULL, {UUID_COLUMN})";
        }

        private static string GetLocationMediaFilesTableDetails() {
            return $"location_media_file (location_id references location(location_id) ON DELETE CASCADE, media_id references media_file(media_id) ON DELETE CASCADE, PRIMARY KEY(location_id, media_id))";
        }

        private static string GetVideoLocationTableDetails() {
            return $"video_location (location_id references location(location_id) ON DELETE CASCADE, video_id references video(video_id) ON DELETE CASCADE, PRIMARY KEY(location_id, video_id))";
        }

        private static string GetCharacterTableDetails() {
            return $"character (character_id {PRIMARY_KEY}, name NVARCHAR({CharacterConstants.MAX_NAME_LENGTH}), description NVARCHAR({CharacterConstants.MAX_DESCRIPTION_LENGTH}), birth_date DATETIME, career_start_date DATETIME, career_end_date DATETIME, rating REAL, deleted BOOLEAN, library_id references library(library_id) ON DELETE CASCADE, deletion_due_to_cascade BOOLEAN, cover_file_id references media_file(media_id) ON DELETE SET NULL, calendar_id references calendar(calendar_id) ON DELETE SET NULL, creator BOOLEAN, {UUID_COLUMN})";
        }

        private static string GetCharacterTagTableDetails() {
            return $"character_tag (tag_id references tag(tag_id) ON DELETE CASCADE, PRIMARY KEY(tag_id))";
        }

        private static string GetTagsOnCharactersTableDetails() {
            return $"tag_on_character (tag_id references character_tag(tag_id) ON DELETE CASCADE, character_id references character(character_id) ON DELETE CASCADE, PRIMARY KEY(tag_id, character_id))";
        }

        private static string GetMediaFileForCharacterTableDetails() {
            return $"character_media_file (character_id references character(character_id) ON DELETE CASCADE, media_id references media_file(media_id) ON DELETE CASCADE, PRIMARY KEY(character_id, media_id))";
        }

        private static string GetPlaylistTableDetails() {
            return $"playlist (library_id references library(library_id) ON DELETE CASCADE, sequence_id references video_sequence(sequence_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, creation_date DATETIME, {UUID_COLUMN}, PRIMARY KEY(sequence_id))";
        }

        private static string GetSeriesSequenceTableDetails() {
            return $"series_sequence (series_id references series(series_id) ON DELETE CASCADE, sequence_id references video_sequence(sequence_id) ON DELETE CASCADE, deleted BOOLEAN, deletion_due_to_cascade BOOLEAN, is_season BOOLEAN, season_number INTEGER, {UUID_COLUMN}, PRIMARY KEY(sequence_id))";
        }

        private static string GetActorForCharacterInVideoTableDetails() {
            return $"actor_for_video_character (creator_id references character(character_id) ON DELETE SET NULL, video_id references video(video_id) ON DELETE CASCADE, character_id references character(character_id) ON DELETE CASCADE, PRIMARY KEY(video_id, character_id))";
        }

        private static string GetVideoCreatorTableDetails() {
            return $"video_creator (creator_id references character(character_id) ON DELETE CASCADE, video_id references video(video_id) ON DELETE CASCADE, role NVARCHAR({VideoCreatorConstants.MAX_ROLE_LENGTH}), PRIMARY KEY(creator_id, video_id))";
        }

        private static string GetLibrarySettingsTableDetails() {
            return $"library_setting (library_id references library(library_id) ON DELETE CASCADE, name NVARCHAR({LibraryConstants.MAX_NAME_LENGTH}), value NVARCHAR({LibraryConstants.MAX_SETTINGS_VALUE_LENGTH}), PRIMARY KEY(library_id, name))";
        }

        private static string GetGlobalSettingsTableDetails() {
            return $"global_setting (name NVARCHAR({GlobalSettingsConstants.MAX_NAME_LENGTH}) PRIMARY KEY, value NVARCHAR({GlobalSettingsConstants.MAX_SETTINGS_VALUE_LENGTH}))";
        }

        private static string GetCalendarTableDetails() {
            return $"calendar (calendar_id {PRIMARY_KEY}, name NVARCHAR({CalendarConstants.MAX_NAME_LENGTH}), description NVARCHAR({CalendarConstants.MAX_DESCRIPTION_LENGTH}), date_format NVARCHAR({CalendarConstants.MAX_DATE_FORMAT_LENGTH}), deleted BOOLEAN, library_id references library(library_id) ON DELETE CASCADE, deletion_due_to_cascade BOOLEAN, {UUID_COLUMN})";
        }

        private static string GetWatchHistoryTableDetails() {
            return $"watch_history (history_id {PRIMARY_KEY}, video_id references video(video_id) ON DELETE CASCADE, watch_date DATETIME)";
        }

        public static void InitTables(SqliteConnection db) {
            string[] tables = {
                GetMediaFileTableDetails(),
                GetLibraryTableDetails(),
                GetPublisherTableDetails(),
                GetSeriesTableDetails(),
                GetTagsOnSeriesTableDetails(),
                GetVideoSequenceTableDetails(),
                GetVideoTableDetails(),
                GetMediaFileForVideoTableDetails(),
                GetVideoInSequenceTableDetails(),
                GetBaseTagTableDetails(),
                GetVideoTagTableDetails(),
                GetTagsOnVideosTableDetails(),
                GetTagsOnMediaFilesTableDetails(),
                GetLocationTableDetails(),
                GetLocationMediaFilesTableDetails(),
                GetVideoLocationTableDetails(),
                GetCharacterTableDetails(),
                GetCharacterTagTableDetails(),
                GetTagsOnCharactersTableDetails(),
                GetMediaFileForCharacterTableDetails(),
                GetPlaylistTableDetails(),
                GetSeriesSequenceTableDetails(),
                GetActorForCharacterInVideoTableDetails(),
                GetVideoCreatorTableDetails(),
                GetLibrarySettingsTableDetails(),
                GetGlobalSettingsTableDetails(),
                GetCalendarTableDetails(),
                GetWatchHistoryTableDetails()
            };
            string baseCreateString = "CREATE TABLE IF NOT EXISTS ";

            foreach (var tableDetails in tables) {
                var command = db.CreateCommand();
                command.CommandText = baseCreateString + tableDetails;
                command.ExecuteNonQuery();
            }
        }
    }
}

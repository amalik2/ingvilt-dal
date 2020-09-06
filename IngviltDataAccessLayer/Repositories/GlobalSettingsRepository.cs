using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Util;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class GlobalSettingsRepository {
        private void SetSettingValue(SqliteConnection db, string key, string value) {
            var command = db.CreateCommand();
            command.CommandText = "REPLACE INTO global_setting (name, value) VALUES (@NameParam, @ValueParam)";
            command.Parameters.AddWithValue("@NameParam", key);
            command.Parameters.AddWithValue("@ValueParam", value);
            command.ExecuteNonQuery();
        }

        public void UpdateSettings(SqliteConnection db, GlobalSettings settings) {
            SetSettingValue(db, nameof(settings.TrackLastWatchDate), settings.TrackLastWatchDate.ToString());
            SetSettingValue(db, nameof(settings.TrackTimesWatched), settings.TrackTimesWatched.ToString());
            SetSettingValue(db, nameof(settings.VideoLoadType), settings.VideoLoadType.ToString());
            SetSettingValue(db, nameof(settings.CollapseTagCategories), settings.CollapseTagCategories.ToString());

            foreach (var settingName in GlobalSettingsConstants.ALL_PREVIEW_TYPE_NAMES) {
                SetSettingValue(db, settingName, settings.PreviewTypes[settingName].ToString());
            }

            SetSettingValue(db, nameof(settings.VideoVolume), settings.VideoVolume.ToString());
        }

        public void UpdateSettings(GlobalSettings settings) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    UpdateSettings(db, settings);
                    txn.Commit();
                }
            }
        }

        public void CreateDefaultSettings(SqliteConnection db) {
            var command = db.CreateCommand();
            command.CommandText = "SELECT count(*) FROM global_setting";
            var reader = command.ExecuteReader();
            if (!reader.Read()) {
                throw new InvalidDataException("Could not check if settings exist");
            }

            var count = reader.GetInt32(0);
            if (count == 0) {
                UpdateSettings(db, new GlobalSettings());
            }
        }

        public void CreateDefaultSettings() {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    CreateDefaultSettings(db);
                    txn.Commit();
                }
            }
        }

        public GlobalSettings LoadSettings() {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand("SELECT * FROM global_setting", db);
                var reader = command.ExecuteReader();

                var settingsMap = new Dictionary<string, string>();
                while (reader.Read()) {
                    settingsMap[reader.GetString(0)] = reader.GetString(1);
                }

                return new GlobalSettings(settingsMap);
            }
        }
    }
}

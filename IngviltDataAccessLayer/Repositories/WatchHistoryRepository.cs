using Ingvilt.Dto.WatchHistory;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class WatchHistoryRepository {
        private WatchedVideoDto ParseWatchedVideo(SqliteDataReader query) {
            return new WatchedVideoDto(query.GetInt64(0), query.GetInt64(1), query.GetString(2), query.GetDateTime(3));
        }

        public async Task<PaginationResult<WatchedVideoDto>> GetWatchedVideosHistory(Pagination pagination, long libraryId) {
            var command = $"SELECT h.history_id, h.video_id, v.title, h.watch_date from watch_history h, video v WHERE v.video_id = h.video_id AND v.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseWatchedVideo);
        }

        public async Task<long> AddWatchedVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"INSERT INTO watch_history(video_id, watch_date) VALUES({videoId}, {"CURRENT_TIMESTAMP"})", db);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        public void SyncWatchedVideos(List<ExternallyWatchedVideoDto> videos) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var updateWatchHistoryCommand = new SqliteCommand($"INSERT INTO watch_history(video_id, watch_date) SELECT video_id, @WatchDate as watch_date FROM video WHERE source_url = @VideoUrl", db, txn);
                    updateWatchHistoryCommand.Parameters.AddWithValue("@VideoUrl", "");
                    updateWatchHistoryCommand.Parameters.AddWithValue("@WatchDate", DateTime.Now);

                    // note: The case clause is used instead of MAX(last_watch_date, @WatchDate) because the MAX approach doesn't seem to work with parameterized queries
                    var updateTimesWatchedCommand = new SqliteCommand($"UPDATE video SET last_watch_date = (CASE WHEN last_watch_date > @WatchDate THEN last_watch_date ELSE @WatchDate END), times_watched = times_watched + 1 WHERE source_url = @VideoUrl", db, txn);
                    updateTimesWatchedCommand.Parameters.AddWithValue("@VideoUrl", "");
                    updateTimesWatchedCommand.Parameters.AddWithValue("@WatchDate", DateTime.Now);

                    foreach (var v in videos) {
                        if (v.VideoUrl == null) {
                            continue;
                        }

                        updateWatchHistoryCommand.Parameters["@VideoUrl"].Value = v.VideoUrl;
                        updateWatchHistoryCommand.Parameters["@WatchDate"].Value = v.Time;
                        updateWatchHistoryCommand.ExecuteNonQuery();

                        updateTimesWatchedCommand.Parameters["@VideoUrl"].Value = v.VideoUrl;
                        updateTimesWatchedCommand.Parameters["@WatchDate"].Value = v.Time;
                        updateTimesWatchedCommand.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }
    }
}

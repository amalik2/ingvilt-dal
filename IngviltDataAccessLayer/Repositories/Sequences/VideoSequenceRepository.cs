using Ingvilt.Constants;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Sequences;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ingvilt.Repositories.Sequences {
    public abstract class VideoSequenceRepository {
        private static readonly string SELECT_BASE = $"SELECT vs.sequence_id, vs.title, vs.description, vs.cover_file FROM video_sequence vs";
        
        private VideoSequence ParseVideoSequence(SqliteDataReader query) {
            return new VideoSequence(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetNullableId(query, 3));
        }

        private void UpdateDeletedStatus(long sequenceId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE {GetChildTableName()} SET deleted = {deleted}, deletion_due_to_cascade = false WHERE sequence_id = {sequenceId}", db);
                command.ExecuteNonQuery();
            }
        }

        protected SqliteCommand GetCreateVideoSequenceCommand(SqliteConnection db, CreateVideoSequenceDto dto) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = "INSERT INTO video_sequence(title, description, cover_file) VALUES(@Name, @Description, @CoverId)";
            command.Parameters.AddWithValue("@Name", dto.Title);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@CoverId", QueryUtil.GetNullableIdForStorage(dto.CoverFile));
            return command;
        }

        protected long CreateVideoSequence(CreateVideoSequenceDto dto, SqliteConnection db, SqliteTransaction txn) {
            var sequenceId = CreateVideoSequence(db, txn, dto);

            var columnNames = new List<string>() { "sequence_id", GetParentColumnName() };
            columnNames.AddRange(GetChildTableAdditionalColumns());

            var columnValues = new List<string>() { sequenceId.ToString(), "@ParentColumnValue" };
            columnValues.AddRange(GetChildTableAdditionalValues(dto));

            var command = new SqliteCommand($"INSERT INTO {GetChildTableName()}({string.Join(",", columnNames)}) VALUES ({string.Join(",", columnValues)})", db, txn);
            command.Parameters.AddWithValue("@ParentColumnValue", QueryUtil.GetNullableIdForStorage(GetParentColumnValue(dto)));
            command.ExecuteNonQuery();

            return sequenceId;
        }

        public long CreateVideoSequence(CreateVideoSequenceDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var sequenceId = CreateVideoSequence(dto, db, txn);
                    txn.Commit();
                    return sequenceId;
                }
            }
        }

        private long CreateVideoSequence(SqliteConnection db, SqliteTransaction txn, CreateVideoSequenceDto dto) {
            var command = GetCreateVideoSequenceCommand(db, dto);
            command.Transaction = txn;
            command.ExecuteNonQuery();

            return QueryUtil.GetLastInsertedPrimaryKey(db, txn);
        }

        protected SqliteCommand GetUpdateVideoSequenceCommand(SqliteConnection db, VideoSequence sequence, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE video_sequence SET title = @Name, description = @Description, cover_file = @CoverFile WHERE {idColumn} = @VideoSequenceId";
            command.Parameters.AddWithValue("@Name", sequence.Title);
            command.Parameters.AddWithValue("@Description", sequence.Description);
            command.Parameters.AddWithValue("@CoverFile", QueryUtil.GetNullableIdForStorage(sequence.CoverFile));
            command.Parameters.AddWithValue("@VideoSequenceId", idValue);
            return command;
        }

        public virtual void UpdateVideoSequence(VideoSequence sequence) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdateVideoSequenceCommand(db, sequence, "sequence_id", sequence.SequenceId);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteVideoSequence(long sequenceId) {
            UpdateDeletedStatus(sequenceId, true);
        }

        public void PermanentlyRemoveVideoSequence(long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video_sequence WHERE sequence_id = {sequenceId}", db);
                command.ExecuteNonQuery();
            }
        }

        public void RestoreDeletedVideoSequence(long sequenceId) {
            UpdateDeletedStatus(sequenceId, false);
        }

        protected abstract string GetChildTableName();

        protected abstract string GetParentColumnName();

        protected abstract long GetParentColumnValue(CreateVideoSequenceDto dto);

        protected virtual List<string> GetChildTableAdditionalColumns() {
            return new List<string>();
        }

        protected virtual List<string> GetChildTableAdditionalValues(CreateVideoSequenceDto dto) {
            return new List<string>();
        }

        protected async Task<PaginationResult<VideoSequence>> GetVideoSequences(Pagination pagination, bool deleted, long parentId = DatabaseConstants.DEFAULT_ID) {
            var parentClause = parentId == DatabaseConstants.DEFAULT_ID ? "" : $"c.{GetParentColumnName()} = {parentId} AND";
            var command = $"{SELECT_BASE}, {GetChildTableName()} c WHERE {parentClause} vs.sequence_id = c.sequence_id AND deleted = {deleted}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoSequence);
        }

        public async Task<PaginationResult<VideoSequence>> GetVideoSequences(Pagination pagination) {
            return await GetVideoSequences(pagination, false);
        }

        public async Task<PaginationResult<VideoSequence>> GetDeletedVideoSequences(Pagination pagination) {
            return await GetVideoSequences(pagination, true);
        }

        private int GetOrderNumberForNewVideoInSequence(long sequenceId, SqliteConnection db, SqliteTransaction txn = null) {
            var orderCommand = new SqliteCommand($"SELECT order_in_list FROM video_in_sequence WHERE sequence_id = {sequenceId} ORDER BY order_in_list DESC LIMIT 1", db, txn);
            var orderReader = orderCommand.ExecuteReader();
            if (orderReader.Read()) {
                return orderReader.GetInt32(0) + 1;
            }

            return 1;
        }

        public void AddVideoToSequence(long videoId, long sequenceId) {
            AddVideosToSequence(new List<long>() { videoId }, sequenceId);
        }

        public void RemoveVideoFromSequence(long videoId, long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video_in_sequence WHERE sequence_id = {sequenceId} AND video_id = {videoId}", db);
                command.ExecuteNonQuery();
            }
        }

        public void AddVideosToSequence(List<long> videoIds, long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var order = GetOrderNumberForNewVideoInSequence(sequenceId, db);

                var baseCommand = "INSERT INTO video_in_sequence(sequence_id, video_id, order_in_list) VALUES";
                var tuples = videoIds.Select((id, index) => {
                    return $"({sequenceId}, {id}, {order + index})";
                });

                var command = new SqliteCommand($"{baseCommand} {string.Join(",", tuples)}", db);
                command.ExecuteNonQuery();
            }
        }

        public void ReorderVideosInSequence(List<VideoInSequenceOrder> videos, long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var video in videos) {
                        var updateQuery = $"UPDATE video_in_sequence SET order_in_list = {video.Order} WHERE video_id = {video.VideoId} AND sequence_id = {sequenceId}";
                        var command = new SqliteCommand(updateQuery, db, txn);
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        protected bool DoesSequenceWithGUIDExist(string guid, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"SELECT * FROM {GetChildTableName()} WHERE unique_id = @UniqueId", db, txn);
            command.Parameters.AddWithValue("@UniqueId", guid);
            var reader = command.ExecuteReader();
            return reader.Read();
        }

        protected long GetSequenceIdFromGUID(string guid, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"SELECT sequence_id FROM {GetChildTableName()} WHERE unique_id = @UniqueId", db, txn);
            command.Parameters.AddWithValue("@UniqueId", guid);
            var reader = command.ExecuteReader();
            reader.Read();

            return reader.GetInt64(0);
        }

        public void UpsertVideosInSequences(List<VideosInSequencesExportDto> videos, Dictionary<string, long> ids) {
            videos.Sort((a, b) => a.Order.CompareTo(b.Order));
            
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO video_in_sequence(sequence_id, video_id, order_in_list) VALUES(@SequenceId, @VideoId, @Order)", db, txn);
                    command.Parameters.AddWithValue("@SequenceId", -1);
                    command.Parameters.AddWithValue("@VideoId", -1);
                    command.Parameters.AddWithValue("@Order", -1);

                    foreach (var v in videos) {
                        if (v.VideoId == null || v.SequenceId == null) {
                            continue;
                        }

                        var sequenceId = ids[v.SequenceId];
                        var videoId = ids[v.VideoId];
                        var order = GetOrderNumberForNewVideoInSequence(sequenceId, db, txn);

                        command.Parameters["@SequenceId"].Value = sequenceId;
                        command.Parameters["@VideoId"].Value = videoId;
                        command.Parameters["@Order"].Value = order;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public int GetPercentageOfVideosWatchedInSequence(long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT SUM(CASE WHEN v.times_watched > 0 THEN 1 ELSE 0 END), COUNT(*) FROM video v, video_in_sequence vis WHERE v.video_id = vis.video_id AND vis.sequence_id = {sequenceId} AND v.deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                if (reader.IsDBNull(0)) {
                    return 0;
                }

                var watchedVideos = reader.GetInt32(0);
                var totalVideos = reader.GetInt32(1);
                if (totalVideos == 0) {
                    return 0;
                }

                var percentWatched = (100 * watchedVideos) / ((double)totalVideos);
                return Convert.ToInt32(Math.Floor(percentWatched));
            }
        }
    }
}

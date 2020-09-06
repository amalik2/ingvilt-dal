using Ingvilt.Constants;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories.Sequences {
    public class SeriesSequenceRepository : VideoSequenceRepository {
        private SeriesSequence ParseSeriesSequence(SqliteDataReader query) {
            return new SeriesSequence(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetNullableId(query, 3), query.GetBoolean(4), query.GetBoolean(5), query.GetInt32(6), query.GetInt64(7), query.GetString(8));
        }

        protected override string GetParentColumnName() {
            return "series_id";
        }

        protected override long GetParentColumnValue(CreateVideoSequenceDto dto) {
            var castDto = dto as CreateSeriesSequenceDto;
            return castDto.SeriesId;
        }

        protected override string GetChildTableName() {
            return "series_sequence";
        }

        protected override List<string> GetChildTableAdditionalColumns() {
            return new List<string>() { "is_season", "season_number", "deleted", "deletion_due_to_cascade", "unique_id" };
        }

        protected override List<string> GetChildTableAdditionalValues(CreateVideoSequenceDto dto) {
            var ss = dto as CreateSeriesSequenceDto;
            return new List<string>() { ss.IsSeason.ToString(), ss.SeasonNumber.ToString(), "false", "false", $"'{dto.UniqueId}'" };
        }

        private async Task<PaginationResult<SeriesSequence>> GetVideoSequencesInSeries(long seriesId, Pagination pagination, bool deleted) {
            var command = $"SELECT vs.sequence_id, vs.title, vs.description, vs.cover_file, c.deleted, c.is_season, c.season_number, c.series_id, c.unique_id FROM video_sequence vs, {GetChildTableName()} c WHERE c.series_id = {seriesId} AND vs.sequence_id = c.sequence_id AND c.deleted = {deleted}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseSeriesSequence);
        }

        public async Task<PaginationResult<SeriesSequence>> GetVideoSequencesInSeries(long seriesId, Pagination pagination) {
            return await GetVideoSequencesInSeries(seriesId, pagination, false);
        }

        public async Task<PaginationResult<SeriesSequence>> GetDeletedVideoSequencesInSeries(long seriesId, Pagination pagination) {
            return await GetVideoSequencesInSeries(seriesId, pagination, true);
        }

        public VideoSeriesChronologyDetails GetVideoSeriesChronologyDetails(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT ss.season_number, vs.order_in_list FROM series_sequence ss, video_in_sequence vs WHERE ss.sequence_id = vs.sequence_id AND vs.video_id = {videoId} AND ss.deleted = false AND ss.is_season = true", db);
                var query = command.ExecuteReader();

                if (query.Read()) {
                    return new VideoSeriesChronologyDetails(query.GetInt32(0), query.GetInt32(1));
                }

                return null;
            }
        }

        public SeriesSequence GetSeriesSequence(long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT vs.sequence_id, vs.title, vs.description, vs.cover_file, ss.deleted, ss.is_season, ss.season_number, ss.series_id, ss.unique_id FROM series_sequence ss, video_sequence vs WHERE ss.sequence_id = {sequenceId} AND ss.sequence_id = vs.sequence_id", db);
                var query = command.ExecuteReader();
                query.Read();
                return ParseSeriesSequence(query);
            }
        }

        public List<int> GetSeasonNumbersInSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT season_number FROM series_sequence WHERE series_id = {seriesId} AND is_season = true AND deleted = false", db);
                var query = command.ExecuteReader();
                var list = new List<int>();

                while (query.Read()) {
                    list.Add(query.GetInt32(0));
                }
                return list;
            }
        }

        protected SqliteCommand GetUpdateSeriesSequenceCommand(SqliteConnection db, SeriesSequence sequence, string idColumn, object idValue) {
            var command = new SqliteCommand($"UPDATE series_sequence SET is_season = {sequence.IsSeason}, season_number = {sequence.SeasonNumber} WHERE {idColumn} = @UniqueId", db);
            command.Parameters.AddWithValue("@UniqueId", idValue);
            return command;
        }

        public override void UpdateVideoSequence(VideoSequence sequence) {
            base.UpdateVideoSequence(sequence);
            var seriesSequence = sequence as SeriesSequence;

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdateSeriesSequenceCommand(db, seriesSequence, "sequence_id", seriesSequence.SequenceId);
                command.ExecuteNonQuery();
            }
        }

        private VideosInSequencesExportDto ParseVideosInSequencesExport(SqliteDataReader query) {
            return new VideosInSequencesExportDto(query.GetString(0), query.GetString(1), query.GetInt32(2));
        }

        public async Task<PaginationResult<VideosInSequencesExportDto>> GetAllVideosInSeriesSequences(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, c.unique_id, vis.order_in_list FROM video v, series s, video_in_sequence vis, {GetChildTableName()} c WHERE v.video_id = vis.video_id AND vis.sequence_id = c.sequence_id AND s.series_id = c.series_id AND v.deleted = false AND s.deleted = false AND v.library_id = {libraryId} AND s.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideosInSequencesExport);
        }

        public async Task<PaginationResult<SeriesSequence>> GetAllSequencesInAllSeries(Pagination pagination, long libraryId) {
            var command = $"SELECT vs.sequence_id, vs.title, vs.description, vs.cover_file, c.deleted, c.is_season, c.season_number, c.series_id, c.unique_id FROM video_sequence vs, {GetChildTableName()} c, series s WHERE s.library_id = {libraryId} AND s.deleted = false AND c.series_id = s.series_id AND vs.sequence_id = c.sequence_id AND c.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseSeriesSequence);
        }

        public async Task UpsertSeriesSequences(List<ExportedSeriesSequenceSimpleDto> sequences, Dictionary<string, long> sequenceIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var s in sequences) {
                        var sequence = s.Details;
                        var sequenceId = DatabaseConstants.DEFAULT_ID;

                        if (DoesSequenceWithGUIDExist(sequence.UniqueId, db, txn)) {
                            sequenceId = GetSequenceIdFromGUID(sequence.UniqueId, db, txn);

                            var command = GetUpdateVideoSequenceCommand(db, sequence, "sequence_id", sequenceId);
                            command.Transaction = txn;
                            command.ExecuteNonQuery();

                            command = GetUpdateSeriesSequenceCommand(db, sequence, "sequence_id", sequenceId);
                            command.Transaction = txn;
                            command.ExecuteNonQuery();
                        } else {
                            var createDto = sequence.GetCreateSeriesSequenceDto();
                            sequenceId = CreateVideoSequence(createDto, db, txn);
                        }

                        sequenceIds[sequence.UniqueId] = sequenceId;
                        sequence.SequenceId = sequenceId;
                    }

                    txn.Commit();
                }
            }
        }
    }
}

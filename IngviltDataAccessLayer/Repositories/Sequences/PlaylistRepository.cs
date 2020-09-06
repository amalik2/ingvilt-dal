using Ingvilt.Constants;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Sequences;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories.Sequences {
    public class PlaylistRepository : VideoSequenceRepository {
        private static readonly string SELECT_BASE = "SELECT vs.sequence_id, vs.title, vs.description, vs.cover_file, p.library_id, p.creation_date, p.deleted, p.unique_id FROM playlist p, video_sequence vs";

        private async Task<PaginationResult<PlaylistDto>> GetPlaylists(Pagination pagination, bool deleted, long parentId = DatabaseConstants.DEFAULT_ID, string nameFilter = "") {
            var parentClause = parentId == DatabaseConstants.DEFAULT_ID ? $"p.{GetParentColumnName()} IS NULL AND" : $"p.{GetParentColumnName()} = {parentId} AND";
            var query = $"{SELECT_BASE} WHERE {parentClause} vs.sequence_id = p.sequence_id AND deleted = {deleted} AND vs.title LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParsePlaylist);
        }

        private PlaylistDto ParsePlaylist(SqliteDataReader query) {
            return new PlaylistDto(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetNullableId(query, 3), query.GetBoolean(6), QueryUtil.GetNullableId(query, 4), query.GetDateTime(5), query.GetString(7));
        }

        protected override string GetParentColumnName() {
            return "library_id";
        }

        protected override long GetParentColumnValue(CreateVideoSequenceDto dto) {
            var castDto = dto as CreatePlaylistDto;
            return castDto.LibraryId;
        }

        protected override string GetChildTableName() {
            return "playlist";
        }

        protected override List<string> GetChildTableAdditionalColumns() {
            return new List<string>() { "creation_date", "deleted", "deletion_due_to_cascade", "unique_id" };
        }

        protected override List<string> GetChildTableAdditionalValues(CreateVideoSequenceDto dto) {
            return new List<string>() { "CURRENT_TIMESTAMP", "false", "false", $"'{dto.UniqueId}'" };
        }

        public async Task<PaginationResult<PlaylistDto>> GetPlaylistsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetPlaylists(pagination, false, libraryId, nameFilter);
        }

        public async Task<PaginationResult<PlaylistDto>> GetDeletedPlaylistsInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetPlaylists(pagination, true, libraryId, nameFilter);
        }

        public PlaylistDto GetPlaylist(long playlistId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{SELECT_BASE} WHERE p.sequence_id = vs.sequence_id AND p.sequence_id = {playlistId}", db);
                var query = command.ExecuteReader();
                query.Read();
                return ParsePlaylist(query);
            }
        }

        public List<PlaylistDto> GetAllPlaylistsContainingVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{SELECT_BASE}, video_in_sequence vis WHERE p.sequence_id = vs.sequence_id AND p.deleted = false AND vs.sequence_id = vis.sequence_id AND vis.video_id = {videoId}", db);
                var query = command.ExecuteReader();

                var list = new List<PlaylistDto>();
                while (query.Read()) {
                    var dto = ParsePlaylist(query);
                    list.Add(dto);
                }
                return list;
            }
        }

        public int GetNumberOfPlaylistsInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM playlist WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<PaginationResult<PlaylistDto>> GetPlaylists(Pagination pagination, string nameFilter) {
            return await GetPlaylists(pagination, false, DatabaseConstants.DEFAULT_ID, nameFilter);
        }

        public async Task<PaginationResult<PlaylistDto>> GetDeletedPlaylists(Pagination pagination, string nameFilter) {
            return await GetPlaylists(pagination, true, DatabaseConstants.DEFAULT_ID, nameFilter);
        }

        private VideosInSequencesExportDto ParseVideosInSequencesExport(SqliteDataReader query) {
            return new VideosInSequencesExportDto(query.GetString(0), query.GetString(1), query.GetInt32(2));
        }

        public async Task<PaginationResult<VideosInSequencesExportDto>> GetAllVideosInPlaylists(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, c.unique_id, vis.order_in_list FROM video v, video_in_sequence vis, {GetChildTableName()} c WHERE v.video_id = vis.video_id AND vis.sequence_id = c.sequence_id AND v.deleted = false AND c.deleted = false AND v.library_id = {libraryId} AND c.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideosInSequencesExport);
        }

        public async Task UpsertPlaylists(List<ExportedPlaylistSimpleDto> sequences, Dictionary<string, long> sequenceIds) {
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
                        } else {
                            var createDto = sequence.GetCreatePlaylistDto();
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

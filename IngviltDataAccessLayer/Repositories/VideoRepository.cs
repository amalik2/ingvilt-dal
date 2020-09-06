using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Sequences;
using Ingvilt.Dto.Videos;
using Ingvilt.Dto.WatchHistory;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class VideoRepository {
        private static readonly string SELECT_BASE_COLUMNS_STRING = "v.video_id, v.title, v.times_watched, v.last_watch_date, v.duration_in_seconds, v.external_rating, v.user_rating, v.description, v.notes, v.source_url, v.site_url, v.series_id, v.watch_status, v.global_rank, v.publisher_id, v.library_id, v.release_date, v.timeline_date, v.cover_image, v.deleted, v.unique_id";
        private static readonly string SELECT_BASE = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM video v";

        private Video ParseVideo(SqliteDataReader query) {
            return new Video(
                query.GetInt64(0), query.GetString(1), query.GetInt32(2), QueryUtil.GetDateTime(query, 3), QueryUtil.GetInt32(query, 4),
                query.GetDouble(5), query.GetDouble(6), query.GetString(7), query.GetString(8), query.GetString(9),
                query.GetString(10), QueryUtil.GetNullableId(query, 11), (VideoWatchStatus)query.GetInt16(12), QueryUtil.GetInt32(query, 13),
                QueryUtil.GetNullableId(query, 14), query.GetInt64(15), QueryUtil.GetDateTime(query, 16), QueryUtil.GetDateTime(query, 17),
                QueryUtil.GetNullableId(query, 18), query.GetBoolean(19), query.GetString(20)
            );
        }

        private void UpdateDeletedStatus(long videoId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE video SET deleted = {deleted}, deletion_due_to_cascade = false WHERE video_id = {videoId}", db);
                command.ExecuteNonQuery();
            }
        }

        private async Task<PaginationResult<Video>> GetVideos(Pagination pagination, bool deleted, string nameFilter = "", long libraryId = DatabaseConstants.DEFAULT_ID) {
            var libraryClause = libraryId == DatabaseConstants.DEFAULT_ID ? "" : $"library_id = {libraryId} AND";
            var query = $"{SELECT_BASE} WHERE {libraryClause} deleted = {deleted} AND v.title LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseVideo);
        }

        private async Task<PaginationResult<Video>> GetVideosByPublisher(Pagination pagination, bool deleted, long publisherId) {
            var command = $"{SELECT_BASE} WHERE publisher_id = {publisherId} AND (deleted = {deleted} OR deletion_due_to_cascade = true)";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideo);
        }

        private async Task<PaginationResult<Video>> GetVideosInSeries(Pagination pagination, bool deleted, long seriesId) {
            var command = $"{SELECT_BASE} WHERE series_id = {seriesId} AND (deleted = {deleted} OR deletion_due_to_cascade = true)";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideo);
        }

        private SqliteCommand GetCreateVideoCommand(SqliteConnection db, CreateVideoDto dto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand();
            command.Connection = db;

            command.CommandText = $"INSERT {ignoreClause} INTO video(title, duration_in_seconds, external_rating, user_rating, description, notes, source_url, site_url, series_id, watch_status, publisher_id, library_id, deleted, deletion_due_to_cascade, times_watched, release_date, timeline_date, cover_image, unique_id) VALUES(@Title, @Duration, @ExternalRating, @UserRating, @Description, @Notes, @SourceURL, @SiteURL, @SeriesId, @WatchStatus, @PublisherId, @LibraryId, false, false, @TimesWatched, @ReleaseDate, @TimelineDate, @CoverImage, @UniqueId)";
            command.Parameters.AddWithValue("@Title", dto.Title);
            command.Parameters.AddWithValue("@Duration", QueryUtil.GetNullableValueForStorage(dto.DurationInSeconds));
            command.Parameters.AddWithValue("@ExternalRating", dto.ExternalRating);
            command.Parameters.AddWithValue("@UserRating", dto.UserRating);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@Notes", dto.Notes);
            command.Parameters.AddWithValue("@SourceURL", dto.SourceURL);
            command.Parameters.AddWithValue("@SiteURL", dto.SiteURL);
            command.Parameters.AddWithValue("@SeriesId", QueryUtil.GetNullableIdForStorage(dto.SeriesId));
            command.Parameters.AddWithValue("@WatchStatus", dto.WatchStatus);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(dto.PublisherId));
            command.Parameters.AddWithValue("@LibraryId", dto.LibraryId);
            command.Parameters.AddWithValue("@ReleaseDate", QueryUtil.GetNullableValueForStorage(dto.ReleaseDate));
            command.Parameters.AddWithValue("@TimelineDate", QueryUtil.GetNullableValueForStorage(dto.TimelineDate));
            command.Parameters.AddWithValue("@CoverImage", QueryUtil.GetNullableIdForStorage(dto.CoverFileId));
            command.Parameters.AddWithValue("@UniqueId", guid);
            command.Parameters.AddWithValue("@TimesWatched", dto.TimesWatched);
            return command;
        }

        public long CreateVideo(CreateVideoDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetCreateVideoCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();
                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateVideoCommand(SqliteConnection db, Video video, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $@"
                    UPDATE video SET title = @Title, times_watched = @TimesWatched, last_watch_date = @LastWatchDate,
                        duration_in_seconds = @Duration, external_rating = @ExternalRating, user_rating = @UserRating,
                        description = @Description, notes = @Notes, source_url = @SourceURL, site_url = @SiteURL,
                        series_id = @SeriesId, watch_status = @WatchStatus, publisher_id = @PublisherId, library_id = @LibraryId,
                        release_date = @ReleaseDate, timeline_date = @TimelineDate, cover_image = @CoverImage
                    WHERE {idColumn} = @VideoId";

            command.Parameters.AddWithValue("@Title", video.Title);
            command.Parameters.AddWithValue("@TimesWatched", video.TimesWatched);
            command.Parameters.AddWithValue("@LastWatchDate", QueryUtil.GetNullableValueForStorage(video.LastWatchDate));
            command.Parameters.AddWithValue("@Duration", QueryUtil.GetNullableValueForStorage(video.DurationInSeconds));
            command.Parameters.AddWithValue("@ExternalRating", video.ExternalRating);
            command.Parameters.AddWithValue("@UserRating", video.UserRating);
            command.Parameters.AddWithValue("@Description", video.Description);
            command.Parameters.AddWithValue("@Notes", video.Notes);
            command.Parameters.AddWithValue("@SourceURL", video.SourceURL);
            command.Parameters.AddWithValue("@SiteURL", video.SiteURL);
            command.Parameters.AddWithValue("@SeriesId", QueryUtil.GetNullableIdForStorage(video.SeriesId));
            command.Parameters.AddWithValue("@WatchStatus", video.WatchStatus);
            command.Parameters.AddWithValue("@PublisherId", QueryUtil.GetNullableIdForStorage(video.PublisherId));
            command.Parameters.AddWithValue("@LibraryId", video.LibraryId);
            command.Parameters.AddWithValue("@VideoId", idValue);
            command.Parameters.AddWithValue("@ReleaseDate", QueryUtil.GetNullableValueForStorage(video.ReleaseDate));
            command.Parameters.AddWithValue("@TimelineDate", QueryUtil.GetNullableValueForStorage(video.TimelineDate));
            command.Parameters.AddWithValue("@CoverImage", QueryUtil.GetNullableIdForStorage(video.CoverFileId));
            return command;
        }

        public async Task UpdateVideo(Video video) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = GetUpdateVideoCommand(db, video, "video_id", video.VideoId);
                command.ExecuteNonQuery();
            }
        }

        public async Task<Video> GetVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand(SELECT_BASE + " WHERE video_id = @VideoId", db);
                command.Parameters.AddWithValue("@VideoId", videoId);
                var query = command.ExecuteReader();
                if (!query.Read()) {
                    throw new ArgumentException($"There was no video with the id {videoId}");
                }

                return ParseVideo(query);
            }
        }

        public async Task<PaginationResult<Video>> GetVideos(Pagination pagination) {
            return await GetVideos(pagination, false);
        }

        public async Task<PaginationResult<Video>> GetDeletedVideos(Pagination pagination) {
            return await GetVideos(pagination, true);
        }

        public async Task<PaginationResult<Video>> GetVideosInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetVideos(pagination, false, nameFilter, libraryId);
        }

        public async Task<PaginationResult<Video>> GetDeletedVideosInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            return await GetVideos(pagination, true, nameFilter, libraryId);
        }

        public async Task<PaginationResult<Video>> GetVideosByPublisher(long publisherid, Pagination pagination) {
            return await GetVideosByPublisher(pagination, false, publisherid);
        }

        public async Task<PaginationResult<Video>> GetDeletedVideosByPublisher(long publisherid, Pagination pagination) {
            return await GetVideosByPublisher(pagination, true, publisherid);
        }

        public async Task<PaginationResult<Video>> GetVideosInSeries(long seriesId, Pagination pagination) {
            return await GetVideosInSeries(pagination, false, seriesId);
        }

        public async Task<PaginationResult<Video>> GetDeletedVideosInSeries(long seriesId, Pagination pagination) {
            return await GetVideosInSeries(pagination, true, seriesId);
        }

        public async Task<PaginationResult<Video>> GetVideosToWatchInLibrary(long libraryId, Pagination pagination, string nameFilter) {
            var query = $"{SELECT_BASE} WHERE library_id = {libraryId} AND deleted = false AND watch_status = @WatchStatus AND v.title LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
                command.Parameters.AddWithValue("@WatchStatus", VideoWatchStatus.NEED_TO_WATCH);
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseVideo);
        }

        public async Task DeleteVideo(long videoId) {
            UpdateDeletedStatus(videoId, true);
        }

        public async Task PermanentlyRemoveVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video WHERE video_id = {videoId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task RestoreDeletedVideo(long videoId) {
            UpdateDeletedStatus(videoId, false);
        }

        private async Task<List<Video>> GetOrderedVideosList(string orderQuery) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand(orderQuery, db);
                var reader = command.ExecuteReader();

                var ids = new List<long>();
                var map = new Dictionary<long, Video>();
                while (reader.Read()) {
                    ids.Add(reader.GetInt64(0));
                }

                command = new SqliteCommand($"{SELECT_BASE} WHERE deleted = false AND video_id IN ({string.Join(",", ids)})", db);
                reader = command.ExecuteReader();

                var list = new List<Video>();
                while (reader.Read()) {
                    var video = ParseVideo(reader);
                    list.Add(video);
                    map[video.VideoId] = video;
                }

                return new List<Video>(ids.Select((id) => map[id]));
            }
        }

        private string GetVideosInSequenceQuery_FromAndWhereClause(long sequenceId) {
            return $"FROM video_in_sequence WHERE sequence_id = {sequenceId} AND video_id NOT IN (SELECT video_id FROM video WHERE deleted = true) ORDER BY order_in_list";
        }

        public async Task<List<Video>> GetAllVideosInSequence(long sequenceId) {
            return await GetOrderedVideosList($"SELECT video_id {GetVideosInSequenceQuery_FromAndWhereClause(sequenceId)}");
        }

        public int GetNumberOfVideosInSequence(long sequenceId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) {GetVideosInSequenceQuery_FromAndWhereClause(sequenceId)}", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<List<Video>> GetVideosInSeriesInChronologicalOrder(long seriesId) {
            return await GetOrderedVideosList($"SELECT v.video_id FROM series_sequence s, video_in_sequence v WHERE s.series_id = {seriesId} AND v.sequence_id = s.sequence_id AND s.deleted = false AND s.is_season = true AND v.video_id NOT IN (SELECT video_id FROM video WHERE deleted = true) ORDER BY s.season_number, v.order_in_list");
        }

        public int GetNumberOfVideosInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM video WHERE library_id = {libraryId} AND deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public int GetNumberOfWatchedVideosInLibrary(long libraryId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM video WHERE library_id = {libraryId} AND deleted = false AND times_watched > 0", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        private List<CreatorOfVideoFullDto> GetVideosCharacterIsInWithoutActors(long characterId, SqliteConnection db, string characterIdColumn) {
            var firstQuery = $"SELECT {SELECT_BASE_COLUMNS_STRING}, c.creator_id, 'Actor' as role FROM video v, actor_for_video_character c WHERE c.{characterIdColumn} = {characterId} AND c.video_id = v.video_id AND v.deleted = false";
            var secondQuery = "";
            if (characterIdColumn == "creator_id") {
                secondQuery = $"UNION SELECT {SELECT_BASE_COLUMNS_STRING}, vc.creator_id, vc.role FROM video v, video_creator vc WHERE v.deleted = false AND vc.video_id = v.video_id AND vc.creator_id = {characterId}";
            }

            var command = new SqliteCommand($"{firstQuery} {secondQuery}", db);
            var query = command.ExecuteReader();
            var list = new List<CreatorOfVideoFullDto>();
            var characterIds = new Dictionary<long, CreatorOfVideoFullDto>();

            while (query.Read()) {
                var role = query.GetString(22);
                var dto = new CreatorOfVideoFullDto(ParseVideo(query), QueryUtil.GetNullableId(query, 21), "", role);

                var key = dto.Video.VideoId;
                if (characterIds.ContainsKey(key)) {
                    var existingDto = characterIds[key];
                    existingDto.Role += $", {dto.Role}";
                } else {
                    list.Add(dto);
                    characterIds[key] = dto;
                }
            }

            return list;
        }

        private async Task<List<CreatorOfVideoFullDto>> GetVideosCharacterIsIn(long characterId, string characterIdColumnName) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var list = GetVideosCharacterIsInWithoutActors(characterId, db, characterIdColumnName);
                var creatorIds = new List<string>(list.Where(c => c.ActorId != DatabaseConstants.DEFAULT_ID).Select(c => c.ActorId.ToString()));
                var creatorIdsString = string.Join(",", creatorIds);

                var command = new SqliteCommand($"SELECT character_id, name FROM character WHERE character_id IN ({creatorIdsString}) AND deleted = false", db);
                var query = command.ExecuteReader();

                var idToVideoMap = new Dictionary<long, CreatorOfVideoFullDto>();
                foreach (var vid in list) {
                    idToVideoMap[vid.ActorId] = vid;
                }

                while (query.Read()) {
                    var id = query.GetInt64(0);
                    var name = query.GetString(1);
                    idToVideoMap[id].ActorName = name;
                }

                return list;
            }
        }

        public async Task<List<CreatorOfVideoFullDto>> GetVideosCharacterIsIn(long characterId) {
            return await GetVideosCharacterIsIn(characterId, "character_id");
        }

        public async Task<List<CreatorOfVideoFullDto>> GetVideosFeaturingCreator(long creatorId) {
            return await GetVideosCharacterIsIn(creatorId, "creator_id");
        }

        public async Task RemoveCharacterFromVideo(long characterId, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"DELETE FROM actor_for_video_character WHERE character_id = {characterId} AND video_id = {videoId}", db);
                command.ExecuteReader();
            }
        }

        public async Task<int> GetNumberOfVideosInSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM video WHERE series_id = {seriesId} AND (deleted = false OR deletion_due_to_cascade = true)", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<PaginationResult<Video>> GetVideosNotInSeriesSequence(SeriesSequence sequence, long seriesId, string titleFilter, Pagination pagination) {
            var videosToExcludeQuery = sequence.IsSeason ? 
                ($"SELECT vs.video_id FROM video_in_sequence vs, series_sequence ss WHERE vs.sequence_id = ss.sequence_id AND ss.series_id = {seriesId} AND ss.is_season = true AND ss.deleted = false") : 
                ($"SELECT vs.video_id FROM video_in_sequence vs WHERE vs.sequence_id = {sequence.SequenceId}");
            var query = $"{SELECT_BASE} WHERE v.deleted = false AND v.title LIKE @TitleFilter AND v.series_id = {seriesId} AND v.video_id NOT IN ({videosToExcludeQuery})";
            Action<SqliteCommand> parameterize = (SqliteCommand command) => {
                command.Parameters.AddWithValue("@TitleFilter", "%" + titleFilter + "%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseVideo);
        }

        public async Task<int> GetNumberOfVideosAtLocation(long locationId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM video v, video_location vl WHERE vl.video_id = v.video_id AND vl.location_id = {locationId} AND v.deleted = false", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public async Task<PaginationResult<Video>> GetVideosAtLocation(Pagination pagination, long locationId) {
            var command = $"{SELECT_BASE}, video_location vl WHERE vl.video_id = v.video_id AND vl.location_id = {locationId} AND v.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideo);
        }

        public async Task<PaginationResult<Video>> GetVideosNotInPlaylist(PlaylistDto playlist, long libraryId, string titleFilter, Pagination pagination) {
            var videosToExcludeQuery = $"SELECT vs.video_id FROM video_in_sequence vs WHERE vs.sequence_id = {playlist.SequenceId}";
            var libraryClause = libraryId == DatabaseConstants.DEFAULT_ID ? "" : $"v.library_id = {libraryId} AND";
            var query = $"{SELECT_BASE} WHERE v.deleted = false AND v.title LIKE @TitleFilter AND {libraryClause} v.video_id NOT IN ({videosToExcludeQuery})";
            Action<SqliteCommand> parameterize = (SqliteCommand command) => {
                command.Parameters.AddWithValue("@TitleFilter", "%" + titleFilter + "%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseVideo);
        }

        public async Task<PaginationResult<Video>> SearchForVideos(Pagination pagination, long libraryId, List<IVideoSearchQueryGenerator> subqueryGenerators) {
            if (subqueryGenerators.Count == 0) {
                return PaginationResult<Video>.CreateResultFromCurrentPage(new List<Video>(), pagination);
            }

            var subqueries = subqueryGenerators.Select((s) => {
                return s.GetSearchQuery();
            });
            var tables = string.Join(" INTERSECT ", subqueries);

            var command = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM video v, ({tables}) AS vi WHERE v.video_id = vi.video_id AND v.deleted = false AND v.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideo);
        }

        public async Task<Video> GetLastWatchedVideoInSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT {SELECT_BASE_COLUMNS_STRING} FROM video v, watch_history w, series_sequence ss, video_in_sequence vs WHERE w.video_id = v.video_id AND v.deleted = false AND ss.series_id = {seriesId} AND v.series_id = {seriesId} AND ss.sequence_id = vs.sequence_id AND vs.video_id = v.video_id AND ss.deleted = false AND ss.is_season = true ORDER BY w.history_id DESC LIMIT 1", db);
                var reader = command.ExecuteReader();

                if (!reader.Read()) {
                    return null;
                }
                return ParseVideo(reader);
            }
        }

        public async Task<PaginationResult<Video>> GetVideosCharacterIsNotIn(Pagination pagination, long libraryId, long characterId, string titleFilter) {
            var query = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM video v WHERE v.library_id = {libraryId} AND v.deleted = false AND v.title LIKE @TitleFilter AND v.video_id NOT IN (SELECT av.video_id FROM actor_for_video_character av WHERE av.character_id = {characterId})";
            Action<SqliteCommand> parameterize = (SqliteCommand command) => {
                command.Parameters.AddWithValue("@TitleFilter", "%" + titleFilter + "%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseVideo);
        }

        public async Task<PaginationResult<Video>> GetVideosWithFile(Pagination pagination, long fileId) {
            var command = $"{SELECT_BASE}, video_media_file vl WHERE vl.video_id = v.video_id AND vl.media_id = {fileId} AND v.deleted = false UNION {SELECT_BASE} WHERE v.cover_image = {fileId} AND v.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideo);
        }

        private VideoCharacterActorExportDto ParseVideoCharacterActor(SqliteDataReader query) {
            return new VideoCharacterActorExportDto(query.GetString(0), query.GetString(1), QueryUtil.GetNullableId(query, 2));
        }

        public async Task<PaginationResult<VideoCharacterActorExportDto>> GetAllCharactersInAllVideos(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, c.unique_id, ac.creator_id FROM video v, character c, actor_for_video_character ac WHERE v.video_id = ac.video_id AND c.character_id = ac.character_id AND v.deleted = false AND v.library_id = {libraryId} AND c.deleted = false AND c.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoCharacterActor);
        }

        private VideoCreatorExportDto ParseVideoCreator(SqliteDataReader query) {
            return new VideoCreatorExportDto(query.GetString(0), query.GetString(1), query.GetString(2));
        }

        public async Task<PaginationResult<VideoCreatorExportDto>> GetAllVideosCreators(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, c.unique_id, vc.role FROM video v, character c, video_creator vc WHERE v.video_id = vc.video_id AND c.character_id = vc.creator_id AND v.deleted = false AND v.library_id = {libraryId} AND c.deleted = false AND c.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoCreator);
        }

        public async Task UpsertVideos(List<ExportedVideoSimpleDto> videos, Dictionary<string, long> videoIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var videoIdCommand = db.CreateCommand();
                    videoIdCommand.CommandText = "SELECT video_id FROM video WHERE unique_id = @VideoId";
                    videoIdCommand.Parameters.AddWithValue("@VideoId", "");

                    foreach (var v in videos) {
                        var video = v.Details;
                        var command = GetUpdateVideoCommand(db, video, "unique_id", video.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = video.GetCreateVideoDto();
                        command = GetCreateVideoCommand(db, createDto, video.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        videoIdCommand.Parameters["@VideoId"].Value = video.UniqueId;
                        var videoId = (long)videoIdCommand.ExecuteScalar();
                        videoIds[video.UniqueId] = videoId;
                        video.VideoId = videoId;
                    }

                    txn.Commit();
                }
            }
        }

        public async Task UpsertCharactersInVideos(List<VideoCharacterActorExportDto> characters, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var c in characters) {
                        if (c.CharacterId == null || c.VideoId == null) {
                            continue;
                        }

                        var charId = ids[c.CharacterId];
                        var videoId = ids[c.VideoId];
                        var actorId = c.ActorId == null ? DatabaseConstants.DEFAULT_ID : ids[c.ActorId];
                        
                        var command = new SqliteCommand($"UPDATE actor_for_video_character SET creator_id = @ActorId WHERE character_id = {charId} AND video_id = {videoId}", db, txn);
                        command.Parameters.AddWithValue("@ActorId", QueryUtil.GetNullableIdForStorage(actorId));
                        command.ExecuteNonQuery();

                        command = new SqliteCommand($"INSERT OR IGNORE INTO actor_for_video_character(character_id, video_id, creator_id) VALUES({charId}, {videoId}, @ActorId)", db, txn);
                        command.Parameters.AddWithValue("@ActorId", QueryUtil.GetNullableIdForStorage(actorId));
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public async Task UpsertCreatorsOfVideos(List<VideoCreatorExportDto> creators, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var c in creators) {
                        if (c.CreatorId == null || c.VideoId == null) {
                            continue;
                        }

                        var creatorId = ids[c.CreatorId];
                        var videoId = ids[c.VideoId];
                        var role = string.IsNullOrWhiteSpace(c.Role) ? "" : c.Role;
                        
                        var command = new SqliteCommand($"UPDATE video_creator SET role = @Role WHERE creator_id = {creatorId} AND video_id = {videoId}", db, txn);
                        command.Parameters.AddWithValue("@Role", role);
                        command.ExecuteNonQuery();

                        command = new SqliteCommand($"INSERT OR IGNORE INTO video_creator(creator_id, video_id, role) VALUES({creatorId}, {videoId}, @Role)", db, txn);
                        command.Parameters.AddWithValue("@Role", role);
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }
    }
}

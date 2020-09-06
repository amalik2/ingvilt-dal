using Ingvilt.Dto;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Locations;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ingvilt.Models.DataAccess.Search.Media;
using System.Linq;
using Ingvilt.Dto.Export;
using Ingvilt.Constants;

namespace Ingvilt.Repositories {
    public class MediaFileRepository {
        private static readonly string SELECT_BASE = "SELECT m.media_id, m.source_url, m.file_type, m.name, m.create_date, m.unique_id FROM media_file m";

        private string GetMediaFileQuery(long fileId) {
            return $"{SELECT_BASE} WHERE m.media_id = {fileId}";
        }

        private MediaFile ParseMediaFile(SqliteDataReader reader) {
            var fileType = reader.GetInt32(2);
            return new MediaFile(reader.GetInt64(0), reader.GetString(1), new MediaFileType(fileType), reader.GetString(3), reader.GetDateTime(4), reader.GetString(5));
        }

        private SqliteCommand GetCreateMediaFileCommand(SqliteConnection db, CreateMediaFileDto createDto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";
            
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"INSERT {ignoreClause} INTO media_file VALUES(NULL, @Url, @Name, @FileType, CURRENT_TIMESTAMP, @UniqueId)";
            command.Parameters.AddWithValue("@Url", createDto.SourceURL);
            command.Parameters.AddWithValue("@FileType", createDto.FileType.Ordinal);
            command.Parameters.AddWithValue("@Name", createDto.Name);
            command.Parameters.AddWithValue("@UniqueId", guid);
            return command;
        }

        private void SetParametersForCreateMediaFileCommand(SqliteCommand command, CreateMediaFileDto createDto, string guid) {
            command.Parameters["@Url"].Value = createDto.SourceURL;
            command.Parameters["@FileType"].Value = createDto.FileType.Ordinal;
            command.Parameters["@Name"].Value = createDto.Name;
            command.Parameters["@UniqueId"].Value = guid;
        }

        public long CreateMediaFile(CreateMediaFileDto createDto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetCreateMediaFileCommand(db, createDto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateMediaFileCommand(SqliteConnection db, MediaFile file, string idColumn, object idValue) {
            var command = new SqliteCommand($"UPDATE media_file SET source_url = @Url, file_type = @FileType, name = @Name WHERE {idColumn} = @Id", db);
            command.Parameters.AddWithValue("@Url", file.SourceURL);
            command.Parameters.AddWithValue("@Name", file.Name);
            command.Parameters.AddWithValue("@Id", idValue);
            command.Parameters.AddWithValue("@FileType", file.FileType.Ordinal);
            return command;
        }

        private void SetParametersForUpdateMediaFileCommand(SqliteCommand command, MediaFile file) {
            command.Parameters["@Url"].Value = file.SourceURL;
            command.Parameters["@Name"].Value = file.Name;
            command.Parameters["@Id"].Value = file.UniqueId;
            command.Parameters["@FileType"].Value = file.FileType.Ordinal;
        }

        public void UpdateMediaFile(MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetUpdateMediaFileCommand(db, file, "media_id", file.MediaId);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteMediaFile(long fileId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM media_file WHERE media_id = {fileId}", db);
                command.ExecuteNonQuery();
            }
        }

        public MediaFile GetMediaFile(long fileId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand(GetMediaFileQuery(fileId), db);
                var reader = command.ExecuteReader();
                
                if (!reader.Read()) {
                    throw new ArgumentException($"There was no media file with the id: {fileId}");
                }

                return ParseMediaFile(reader);
            }
        }

        public async Task<PaginationResult<MediaFile>> GetMediaFiles(Pagination pagination, string nameFilter = "") {
            var query = $"{SELECT_BASE} WHERE name LIKE @NameFilter";
            return await DataAccessUtil.GetPaginatedResult(pagination, query, (SqliteCommand command) => {
                command.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");
            }, ParseMediaFile);
        }

        public MediaFile GetFileWithUrl(string url) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"{SELECT_BASE} WHERE source_url = @FileUrl", db);
                command.Parameters.AddWithValue("@FileUrl", url);
                var reader = command.ExecuteReader();

                if (!reader.Read()) {
                    return null;
                }

                return ParseMediaFile(reader);
            }
        }

        public int CreateMediaFiles(List<CreateMediaFileDto> dtos) {
            int count = 0;

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var checkUrlCommand = new SqliteCommand("SELECT media_id FROM media_file WHERE source_url = @FileUrl", db, txn);
                    checkUrlCommand.Parameters.AddWithValue("@FileUrl", "");

                    foreach (var dto in dtos) {
                        checkUrlCommand.Parameters["@FileUrl"].Value = dto.SourceURL;
                        var reader = checkUrlCommand.ExecuteReader();

                        // this url already exists
                        var fileExists = reader.Read();
                        reader.Close();
                        if (fileExists) {
                            continue;
                        }

                        var command = GetCreateMediaFileCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        ++count;
                    }

                    txn.Commit();
                }

                return count;
            }
        }

        public List<VideoTag> GetMediaFileTags(MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("SELECT v.tag_id, v.name, v.type FROM tag_on_media_file t, tag v WHERE t.media_file_id = @FileId AND t.tag_id = v.tag_id AND v.deleted = false", db);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                var reader = command.ExecuteReader();

                var tags = new List<VideoTag>();
                while (reader.Read()) {
                    tags.Add(new VideoTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2)));
                }

                return tags;
            }
        }

        public void AddTagToMediaFile(MediaFile file, VideoTag tag) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("INSERT INTO tag_on_media_file VALUES (@TagId, @FileId)", db);
                command.Parameters.AddWithValue("@TagId", tag.TagId);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                command.ExecuteNonQuery();
            }
        }

        public void RemoveTagFromMediaFile(MediaFile file, VideoTag tag) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("DELETE FROM tag_on_media_file WHERE tag_id = @TagId AND media_file_id = @FileId", db);
                command.Parameters.AddWithValue("@TagId", tag.TagId);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                command.ExecuteNonQuery();
            }
        }

        public List<Location> GetMediaFileLocations(MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("SELECT l.location_id, l.name, l.description, l.cover_file_id FROM location_media_file m, location l WHERE m.media_id = @FileId AND l.location_id = m.location_id AND l.deleted = false", db);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                var reader = command.ExecuteReader();

                var locations = new List<Location>();
                while (reader.Read()) {
                    locations.Add(new Location(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), QueryUtil.GetNullableId(reader, 3)));
                }

                return locations;
            }
        }

        public async Task AddLocationToMediaFile(MediaFile file, Location location) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("INSERT INTO location_media_file VALUES (@LocationId, @FileId)", db);
                command.Parameters.AddWithValue("@LocationId", location.LocationId);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                command.ExecuteNonQuery();
            }
        }

        public async Task RemoveLocationFromMediaFile(MediaFile file, Location location) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand("DELETE FROM location_media_file WHERE location_id = @LocationId AND media_id = @FileId", db);
                command.Parameters.AddWithValue("@LocationId", location.LocationId);
                command.Parameters.AddWithValue("@FileId", file.MediaId);
                command.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<MediaFile>> GetFilesForVideo(Pagination pagination, long videoId) {
            var command = $"{SELECT_BASE}, video_media_file vmf WHERE vmf.video_id = {videoId} AND vmf.media_id = m.media_id";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseMediaFile);
        }

        public async Task AddFileToVideo(long videoId, MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"INSERT INTO video_media_file(video_id, media_id) VALUES({videoId}, {file.MediaId})", db);
                command.ExecuteNonQuery();
            }
        }

        private void AddFilesToEntity(long entityId, List<MediaFile> files, string table, string entityIdColumn) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand($"INSERT OR IGNORE INTO {table}({entityIdColumn}, media_id) VALUES({entityId}, @FileId)", db, txn);
                    command.Parameters.AddWithValue("@FileId", -1);

                    foreach (var file in files) {
                        command.Parameters["@FileId"].Value = file.MediaId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void AddFilesToVideo(long videoId, List<MediaFile> files) {
            AddFilesToEntity(videoId, files, "video_media_file", "video_id");
        }

        public void AddFilesToLocation(long locationId, List<MediaFile> files) {
            AddFilesToEntity(locationId, files, "location_media_file", "location_id");
        }

        public void AddFilesToCharacter(long characterId, List<MediaFile> files) {
            AddFilesToEntity(characterId, files, "character_media_file", "character_id");
        }

        public async Task RemoveFileFromVideo(long videoId, MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video_media_file WHERE video_id = {videoId} AND media_id = {file.MediaId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<MediaFile>> GetFilesAtLocation(Pagination pagination, long locationId) {
            var command = $"{SELECT_BASE}, location_media_file lm WHERE lm.media_id = m.media_id AND lm.location_id = {locationId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseMediaFile);
        }

        public void AddFileToCharacter(long characterId, long fileId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"INSERT INTO character_media_file(character_id, media_id) VALUES({characterId}, {fileId})", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task AddFileToCharacter(long characterId, MediaFile file) {
            AddFileToCharacter(characterId, file.MediaId);
        }

        public async Task RemoveFileFromCharacter(long characterId, MediaFile file) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM character_media_file WHERE character_id = {characterId} AND media_id = {file.MediaId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<MediaFile>> GetFilesForCharacter(Pagination pagination, long characterId) {
            var command = $"{SELECT_BASE}, character_media_file cmf WHERE cmf.character_id = {characterId} AND cmf.media_id = m.media_id";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseMediaFile);
        }

        public async Task<PaginationResult<MediaFile>> SearchForFiles(Pagination pagination, List<IMediaFileSearchQueryGenerator> subqueryGenerators) {
            if (subqueryGenerators.Count == 0) {
                return PaginationResult<MediaFile>.CreateResultFromCurrentPage(new List<MediaFile>(), pagination);
            }

            var subqueries = subqueryGenerators.Select((s) => {
                return s.GetSearchQuery();
            });
            var tables = string.Join(" INTERSECT ", subqueries);

            var command = $"{SELECT_BASE}, ({tables}) AS mi WHERE m.media_id = mi.media_id";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseMediaFile);
        }

        private VideoMediaFilesExportDto ParseVideoMediaFile(SqliteDataReader reader) {
            return new VideoMediaFilesExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<VideoMediaFilesExportDto>> GetAllFilesOnVideos(Pagination pagination, long libraryId) {
            var command = $"SELECT v.unique_id, mf.unique_id FROM video v, media_file mf, video_media_file vmf WHERE vmf.video_id = v.video_id AND vmf.media_id = mf.media_id AND v.deleted = false AND v.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoMediaFile);
        }

        private CharacterMediaFilesExportDto ParseCharacterMediaFile(SqliteDataReader reader) {
            return new CharacterMediaFilesExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<CharacterMediaFilesExportDto>> GetAllFilesOnCharacters(Pagination pagination, long libraryId) {
            var command = $"SELECT c.unique_id, mf.unique_id FROM character c, media_file mf, character_media_file vmf WHERE vmf.character_id = c.character_id AND vmf.media_id = mf.media_id AND c.deleted = false AND c.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseCharacterMediaFile);
        }

        public async Task UpsertMediaFiles(List<MediaFile> files, Dictionary<string, long> fileIds) {
            if (files.Count == 0) {
                return;
            }

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var updateCommand = GetUpdateMediaFileCommand(db, files[0], "unique_id", files[0].UniqueId);
                    updateCommand.Transaction = txn;

                    var createCommand = GetCreateMediaFileCommand(db, files[0].GetCreateMediaFileDto(), files[0].UniqueId, true);
                    createCommand.Transaction = txn;

                    var fileIdCommand = new SqliteCommand($"SELECT media_id FROM media_file WHERE unique_id = @UniqueId", db, txn);
                    fileIdCommand.Parameters.AddWithValue("@UniqueId", "");

                    foreach (var file in files) {
                        SetParametersForUpdateMediaFileCommand(updateCommand, file);
                        updateCommand.ExecuteNonQuery();

                        var createDto = file.GetCreateMediaFileDto();
                        SetParametersForCreateMediaFileCommand(createCommand, createDto, file.UniqueId);
                        createCommand.ExecuteNonQuery();

                        fileIdCommand.Parameters["@UniqueId"].Value = file.UniqueId;
                        fileIds[file.UniqueId] = (long)fileIdCommand.ExecuteScalar();
                    }

                    txn.Commit();
                }
            }
        }

        private MediaFile GetMediaFile(string fileId, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand($"{SELECT_BASE} WHERE unique_id = @UniqueId", db, txn);
            command.Parameters.AddWithValue("@UniqueId", fileId);
            var reader = command.ExecuteReader();

            if (!reader.Read()) {
                return null;
            }

            return ParseMediaFile(reader);
        }

        private MediaFile GetMediaFile(string fileId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                return GetMediaFile(fileId, db, null);
            }
        }

        public long GetMediaFileLongId_FromGUID(string guid) {
            var fileId = DatabaseConstants.DEFAULT_ID;
            if (guid != null) {
                var file = GetMediaFile(guid);
                if (file != null) {
                    fileId = file.MediaId;
                }
            }

            return fileId;
        }

        public void UpsertFilesOnCharacters(List<CharacterMediaFilesExportDto> files, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO character_media_file(character_id, media_id) VALUES(@CharacterId, @FileId)", db, txn);
                    command.Parameters.AddWithValue("@CharacterId", "");
                    command.Parameters.AddWithValue("@FileId", "");

                    foreach (var f in files) {
                        if (f.CharacterId == null || f.MediaFileId == null) {
                            continue;
                        }

                        var charId = ids[f.CharacterId];
                        var fileId = ids[f.MediaFileId];

                        command.Parameters["@CharacterId"].Value = charId;
                        command.Parameters["@FileId"].Value = fileId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertFilesOnLocations(List<FileLocationExportDto> files, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO location_media_file(location_id, media_id) VALUES(@LocationId, @FileId)", db, txn);
                    command.Parameters.AddWithValue("@LocationId", "");
                    command.Parameters.AddWithValue("@FileId", "");

                    foreach (var f in files) {
                        if (f.LocationId == null || f.FileId == null) {
                            continue;
                        }

                        var locId = ids[f.LocationId];
                        var fileId = ids[f.FileId];

                        command.Parameters["@LocationId"].Value = locId;
                        command.Parameters["@FileId"].Value = fileId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertFilesOnVideos(List<VideoMediaFilesExportDto> files, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO video_media_file(video_id, media_id) VALUES(@VideoId, @FileId)", db, txn);
                    command.Parameters.AddWithValue("@VideoId", "");
                    command.Parameters.AddWithValue("@FileId", "");

                    foreach (var f in files) {
                        if (f.VideoId == null || f.MediaFileId == null) {
                            continue;
                        }

                        var videoId = ids[f.VideoId];
                        var fileId = ids[f.MediaFileId];

                        command.Parameters["@VideoId"].Value = videoId;
                        command.Parameters["@FileId"].Value = fileId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        private async Task<PaginationResult<MediaFile>> GetFilesNotOnEntity(Pagination pagination, long videoId, string nameFilter, string tableName, string entityIdColumn) {
            var query = $"{SELECT_BASE} WHERE name LIKE @NameFilter AND media_id NOT IN (SELECT media_id FROM {tableName} WHERE {entityIdColumn} = {videoId})";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", $"%{nameFilter}%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseMediaFile);
        }

        public async Task<PaginationResult<MediaFile>> GetFilesNotOnVideo(Pagination pagination, long videoId, string nameFilter) {
            return await GetFilesNotOnEntity(pagination, videoId, nameFilter, "video_media_file", "video_id");
        }

        public async Task<PaginationResult<MediaFile>> GetFilesNotOnCharacter(Pagination pagination, long characterId, string nameFilter) {
            return await GetFilesNotOnEntity(pagination, characterId, nameFilter, "character_media_file", "character_id");
        }

        public async Task<PaginationResult<MediaFile>> GetFilesNotAtLocation(Pagination pagination, long locationId, string nameFilter) {
            return await GetFilesNotOnEntity(pagination, locationId, nameFilter, "location_media_file", "location_id");
        }
    }
}

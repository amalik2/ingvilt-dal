using Ingvilt.Dto.Export;
using Ingvilt.Dto.Tags;
using Ingvilt.Models.DataAccess;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class TagRepository {
        private long CreateTag(CreateTagDto tag, SqliteConnection db, SqliteTransaction txn) {
            var tagCommand = new SqliteCommand("INSERT INTO tag(type, name, deleted, unique_id) VALUES (@TagType, @TagName, false, @UniqueId)", db, txn);
            tagCommand.Parameters.AddWithValue("@TagType", tag.Type);
            tagCommand.Parameters.AddWithValue("@TagName", tag.Name);
            tagCommand.Parameters.AddWithValue("@UniqueId", UniqueIdUtil.GenerateUniqueId());
            tagCommand.ExecuteNonQuery();
            return QueryUtil.GetLastInsertedPrimaryKey(db, txn); 
        }

        private long CreateTag(CreateTagDto tag, string tableName) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    var tagId = CreateTag(tag, db, txn);
                    var tagCommand = new SqliteCommand($"INSERT INTO {tableName} VALUES (@TagId)", db, txn);
                    tagCommand.Parameters.AddWithValue("@TagId", tagId);
                    tagCommand.ExecuteNonQuery();

                    txn.Commit();
                    return tagId;
                }
            }
        }

        private SqliteDataReader ExecuteQueryForTag(SqliteConnection db, long tagId) {
            db.Open();
            // TODO: table type safety
            var command = new SqliteCommand($"SELECT name, type, unique_id FROM tag WHERE tag_id = @TagId", db);
            command.Parameters.AddWithValue("@TagId", tagId);
            var reader = command.ExecuteReader();

            if (!reader.Read()) {
                throw new ArgumentException($"There was no tag with the id: {tagId}");
            }

            return reader;
        }

        private void SetTagDeletedStatus(Tag tag, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var tagCommand = new SqliteCommand($"UPDATE tag SET deleted = {deleted} WHERE tag_id = @TagId", db);
                tagCommand.Parameters.AddWithValue("@TagId", tag.TagId);
                tagCommand.ExecuteNonQuery();
            }
        }

        private void PermanentlyRemoveTag(Tag tag) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var tagCommand = new SqliteCommand($"DELETE FROM tag WHERE tag_id = @TagId", db);
                tagCommand.Parameters.AddWithValue("@TagId", tag.TagId);
                tagCommand.ExecuteNonQuery();
            }
        }

        private SqliteDataReader SearchForTags(SqliteConnection db, bool deleted, string tableName) {
            var command = new SqliteCommand($"SELECT t.tag_id, t.name, t.type, t.unique_id FROM {tableName} v, tag t WHERE v.tag_id = t.tag_id AND t.deleted = @Deleted", db);
            command.Parameters.AddWithValue("@Deleted", deleted);
            return command.ExecuteReader();
        }

        private List<string> LoadAllTagNamesInTable(string tableName) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT t.name FROM {tableName} v, tag t WHERE v.tag_id = t.tag_id", db);
                var reader = command.ExecuteReader();

                var tags = new List<string>();
                while (reader.Read()) {
                    tags.Add(reader.GetString(0));
                }

                return tags;
            }
        }

        private void ValidateEntityType(string entityType) {
            if (entityType != "video" && entityType != "character" && entityType != "media_file" && entityType != "series") {
                throw new ArgumentException("The entity type must be either video or character");
            }
        }

        private void AddTagsToEntity(long entityId, List<long> tagIds, string entityType) {
            ValidateEntityType(entityType);

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    foreach (var tag in tagIds) {
                        var tagCommand = new SqliteCommand($"INSERT INTO tag_on_{entityType}(tag_id, {entityType}_id) VALUES({tag}, {entityId})", db, txn);
                        tagCommand.ExecuteNonQuery();
                    }
                    txn.Commit();
                }
            }
        }

        private void UpdateTagsOnEntity(long entityId, List<long> tagIds, string entityType) {
            ValidateEntityType(entityType);

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    var clearCommand = new SqliteCommand($@"DELETE FROM tag_on_{entityType} WHERE {entityType}_id = {entityId} AND tag_id IN (
    SELECT tag_id FROM tag WHERE deleted = false
)", db, txn);
                    clearCommand.ExecuteNonQuery();

                    foreach (var tag in tagIds) {
                        var tagCommand = new SqliteCommand($"INSERT INTO tag_on_{entityType}(tag_id, {entityType}_id) VALUES({tag}, {entityId})", db, txn);
                        tagCommand.ExecuteNonQuery();
                    }
                    txn.Commit();
                }
            }
        }

        private void RemoveTagFromEntity(long entityId, long tagId, string entityType) {
            ValidateEntityType(entityType);

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var clearCommand = new SqliteCommand($@"DELETE FROM tag_on_{entityType} WHERE {entityType}_id = {entityId} AND tag_id = {tagId}", db);
                clearCommand.ExecuteNonQuery();
            }
        }

        public long CreateVideoTag(CreateVideoTagDto videoTag) {
            return CreateTag(videoTag, "video_tag");
        }

        public long CreateCharacterTag(CreateCharacterTagDto characterTag) {
            return CreateTag(characterTag, "character_tag");
        }

        public VideoTag GetVideoTag(long tagId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                var reader = ExecuteQueryForTag(db, tagId);
                return new VideoTag(tagId, reader.GetString(0), reader.GetString(1), reader.GetString(2));
            }
        }

        public CharacterTag GetCharacterTag(long tagId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                var reader = ExecuteQueryForTag(db, tagId);
                return new CharacterTag(tagId, reader.GetString(0), reader.GetString(1), reader.GetString(2));
            }
        }

        public void DeleteVideoTag(VideoTag tag) {
            SetTagDeletedStatus(tag, true);
        }

        public void DeleteCharacterTag(CharacterTag tag) {
            SetTagDeletedStatus(tag, true);
        }

        public void RestoreVideoTag(VideoTag tag) {
            SetTagDeletedStatus(tag, false);
        }

        public void RestoreCharacterTag(CharacterTag tag) {
            SetTagDeletedStatus(tag, false);
        }

        public void PermanentlyRemoveVideoTag(VideoTag tag) {
            PermanentlyRemoveTag(tag);
        }

        public void PermanentlyRemoveCharacterTag(CharacterTag tag) {
            PermanentlyRemoveTag(tag);
        }

        public List<Tag> GetVideoTags(bool deleted = false) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var reader = SearchForTags(db, deleted, "video_tag");

                var tags = new List<Tag>();
                while (reader.Read()) {
                    tags.Add(new VideoTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }

                return tags;
            }
        }

        public List<Tag> GetDeletedVideoTags() {
            return GetVideoTags(true);
        }

        public List<Tag> GetCharacterTags(bool deleted = false) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var reader = SearchForTags(db, deleted, "character_tag");

                var tags = new List<Tag>();
                while (reader.Read()) {
                    tags.Add(new CharacterTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2)));
                }

                return tags;
            }
        }

        public List<Tag> GetDeletedCharacterTags() {
            return GetCharacterTags(true);
        }

        public long UpdateTag(Tag tag) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var tagCommand = new SqliteCommand($"UPDATE tag SET type = @TagType, name = @TagName WHERE tag_id = @TagId", db);
                tagCommand.Parameters.AddWithValue("@TagType", tag.Type);
                tagCommand.Parameters.AddWithValue("@TagName", tag.Name);
                tagCommand.Parameters.AddWithValue("@TagId", tag.TagId);
                tagCommand.ExecuteNonQuery();
                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        public List<string> LoadAllVideoTagNames() {
            return LoadAllTagNamesInTable("video_tag");
        }

        public List<string> LoadAllCharacterTagNames() {
            return LoadAllTagNamesInTable("character_tag");
        }

        public void AddTagsToVideo(long videoId, List<long> tagIds) {
            AddTagsToEntity(videoId, tagIds, "video");
        }

        public void UpdateTagsOnVideo(long videoId, List<long> tagIds) {
            UpdateTagsOnEntity(videoId, tagIds, "video");
        }

        public void RemoveTagFromVideo(long videoId, long tagId) {
            RemoveTagFromEntity(videoId, tagId, "video");
        }

        public void AddTagsToMediaFile(long fileId, List<long> tagIds) {
            AddTagsToEntity(fileId, tagIds, "media_file");
        }

        public void UpdateTagsOnMediaFile(long fileId, List<long> tagIds) {
            UpdateTagsOnEntity(fileId, tagIds, "media_file");
        }

        public void RemoveTagFromMediaFile(long fileId, long tagId) {
            RemoveTagFromEntity(fileId, tagId, "media_file");
        }

        public void AddTagsToCharacter(long characterId, List<long> tagIds) {
            AddTagsToEntity(characterId, tagIds, "character");
        }

        public void UpdateTagsOnCharacter(long characterId, List<long> tagIds) {
            UpdateTagsOnEntity(characterId, tagIds, "character");
        }

        public void RemoveTagFromCharacter(long characterId, long tagId) {
            RemoveTagFromEntity(characterId, tagId, "character");
        }

        public void AddTagsToSeries(long seriesId, List<long> tagIds) {
            AddTagsToEntity(seriesId, tagIds, "series");
        }

        public void UpdateTagsOnSeries(long seriesId, List<long> tagIds) {
            UpdateTagsOnEntity(seriesId, tagIds, "series");
        }

        public void RemoveTagFromSeries(long seriesId, long tagId) {
            RemoveTagFromEntity(seriesId, tagId, "series");
        }

        public List<Tag> GetTagsOnVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT t.tag_id, t.name, t.type, t.unique_id FROM tag_on_video v, tag t WHERE v.video_id = {videoId} AND v.tag_id = t.tag_id AND t.deleted = false", db);
                var reader = command.ExecuteReader();

                var tags = new List<Tag>();
                while (reader.Read()) {
                    var tag = new VideoTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    tags.Add(tag);
                }

                return tags;
            }
        }

        public List<Tag> GetTagsOnCharacter(long characterId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT t.tag_id, t.name, t.type, t.unique_id FROM tag_on_character v, tag t WHERE v.character_id = {characterId} AND v.tag_id = t.tag_id AND t.deleted = false", db);
                var reader = command.ExecuteReader();

                var tags = new List<Tag>();
                while (reader.Read()) {
                    var tag = new CharacterTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    tags.Add(tag);
                }

                return tags;
            }
        }

        public List<Tag> GetTagsOnMediaFile(long fileId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT t.tag_id, t.name, t.type, t.unique_id FROM tag_on_media_file v, tag t WHERE v.media_file_id = {fileId} AND v.tag_id = t.tag_id AND t.deleted = false", db);
                var reader = command.ExecuteReader();

                var tags = new List<Tag>();
                while (reader.Read()) {
                    var tag = new VideoTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    tags.Add(tag);
                }

                return tags;
            }
        }

        public List<Tag> GetTagsOnSeries(long seriesId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT t.tag_id, t.name, t.type, t.unique_id FROM tag_on_series v, tag t WHERE v.series_id = {seriesId} AND v.tag_id = t.tag_id AND t.deleted = false", db);
                var reader = command.ExecuteReader();

                var tags = new List<Tag>();
                while (reader.Read()) {
                    var tag = new VideoTag(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    tags.Add(tag);
                }

                return tags;
            }
        }

        private SeriesTagExportDto ParseSeriesTagDto(SqliteDataReader reader) {
            return new SeriesTagExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<SeriesTagExportDto>> GetTagsOnAllSeries(Pagination pagination, long libraryId) {
            var query = $"SELECT s.unique_id, t.unique_id FROM tag_on_series ts, tag t, series s WHERE ts.tag_id = t.tag_id AND t.deleted = false AND s.series_id = ts.series_id AND s.deleted = false AND s.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, query, ParseSeriesTagDto);
        }

        private VideoTagExportDto ParseVideoTagDto(SqliteDataReader reader) {
            return new VideoTagExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<VideoTagExportDto>> GetTagsOnAllVideos(Pagination pagination, long libraryId) {
            var query = $"SELECT v.unique_id, t.unique_id FROM tag_on_video ts, tag t, video v WHERE ts.tag_id = t.tag_id AND t.deleted = false AND v.video_id = ts.video_id AND v.deleted = false AND v.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, query, ParseVideoTagDto);
        }

        private FileTagExportDto ParseFileTagDto(SqliteDataReader reader) {
            return new FileTagExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<FileTagExportDto>> GetTagsOnAllFiles(Pagination pagination) {
            var query = $"SELECT mf.unique_id, t.unique_id FROM tag_on_media_file tmf, tag t, media_file mf WHERE tmf.tag_id = t.tag_id AND t.deleted = false AND mf.media_id = tmf.media_file_id";
            return await DataAccessUtil.GetPaginatedResult(pagination, query, ParseFileTagDto);
        }

        private CharacterTagExportDto ParseCharacterTagDto(SqliteDataReader reader) {
            return new CharacterTagExportDto(reader.GetString(0), reader.GetString(1));
        }

        public async Task<PaginationResult<CharacterTagExportDto>> GetTagsOnAllCharacters(Pagination pagination, long libraryId) {
            var query = $"SELECT c.unique_id, t.unique_id FROM tag_on_character toc, tag t, character c WHERE toc.tag_id = t.tag_id AND t.deleted = false AND c.deleted = false AND c.library_id = {libraryId} AND toc.character_id = c.character_id";
            return await DataAccessUtil.GetPaginatedResult(pagination, query, ParseCharacterTagDto);
        }

        private void UpsertTags(List<Tag> tags, string tableName) {
            if (tags.Count == 0) {
                return;
            }

            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var updateTagCommand = new SqliteCommand("UPDATE tag SET type = @Type, name = @Name WHERE unique_id = @UniqueId", db, txn);
                    updateTagCommand.Parameters.AddWithValue("@Name", "");
                    updateTagCommand.Parameters.AddWithValue("@Type", "");
                    updateTagCommand.Parameters.AddWithValue("@UniqueId", "");

                    // TODO: library id
                    var insertTagCommand = new SqliteCommand("INSERT OR IGNORE INTO tag(name, type, deleted, unique_id) VALUES (@Name, @Type, false, @UniqueId)", db, txn);
                    insertTagCommand.Parameters.AddWithValue("@Name", "");
                    insertTagCommand.Parameters.AddWithValue("@Type", "");
                    insertTagCommand.Parameters.AddWithValue("@UniqueId", "");

                    var selectTagCommand = new SqliteCommand("SELECT tag_id FROM tag WHERE unique_id = @UniqueId", db, txn);
                    selectTagCommand.Parameters.AddWithValue("@UniqueId", "");

                    var insertIntoOtherTableCommand = new SqliteCommand($"INSERT OR IGNORE INTO {tableName}(tag_id) VALUES (@TagId)", db, txn);
                    insertIntoOtherTableCommand.Parameters.AddWithValue("@TagId", 0);

                    foreach (var tag in tags) {
                        if (tag.Name == null || tag.Type == null || tag.UniqueId == null) {
                            continue;
                        }

                        updateTagCommand.Parameters["@Name"].Value = tag.Name;
                        updateTagCommand.Parameters["@Type"].Value = tag.Type;
                        updateTagCommand.Parameters["@UniqueId"].Value = tag.UniqueId;
                        updateTagCommand.ExecuteNonQuery();

                        // TODO: library id
                        insertTagCommand.Parameters["@Name"].Value = tag.Name;
                        insertTagCommand.Parameters["@Type"].Value = tag.Type;
                        insertTagCommand.Parameters["@UniqueId"].Value = tag.UniqueId;
                        insertTagCommand.ExecuteNonQuery();

                        selectTagCommand.Parameters["@UniqueId"].Value = tag.UniqueId;
                        var tagId = (long)selectTagCommand.ExecuteScalar();

                        insertIntoOtherTableCommand.Parameters["@TagId"].Value = tagId;
                        insertIntoOtherTableCommand.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertCharacterTags(List<Tag> tags) {
            UpsertTags(tags, "character_tag");
        }

        public void UpsertVideoTags(List<Tag> tags) {
            UpsertTags(tags, "video_tag");
        }

        public void UpsertTagsOnCharacters(List<CharacterTagExportDto> tags, Dictionary<string, long> ids) {
            if (tags.Count == 0) {
                return;
            }
            
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand($"INSERT OR IGNORE INTO tag_on_character(tag_id, character_id) SELECT tag_id, @CharacterId AS 'character_id' FROM tag WHERE unique_id = @TagId", db, txn);
                    command.Parameters.AddWithValue("@CharacterId", -1);
                    command.Parameters.AddWithValue("@TagId", -1);

                    foreach (var t in tags) {
                        if (t.TagId == null || t.CharacterId == null) {
                            continue;
                        }

                        var charId = ids[t.CharacterId];

                        command.Parameters["@CharacterId"].Value = charId;
                        command.Parameters["@TagId"].Value = t.TagId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertTagsOnVideos(List<VideoTagExportDto> tags, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO tag_on_video(tag_id, video_id) SELECT tag_id, @VideoId as 'video_id' FROM tag WHERE unique_id = @TagId", db, txn);
                    command.Parameters.AddWithValue("@VideoId", -1); 
                    command.Parameters.AddWithValue("@TagId", -1);

                    foreach (var t in tags) {
                        if (t.TagId == null || t.VideoId == null) {
                            continue;
                        }

                        var videoId = ids[t.VideoId];

                        command.Parameters["@VideoId"].Value = videoId;
                        command.Parameters["@TagId"].Value = t.TagId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertTagsOnFiles(List<FileTagExportDto> tags, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO tag_on_media_file(tag_id, media_file_id) SELECT tag_id, @FileId as 'media_file_id' FROM tag WHERE unique_id = @TagId", db, txn);
                    command.Parameters.AddWithValue("@TagId", -1);
                    command.Parameters.AddWithValue("@FileId", -1);

                    foreach (var t in tags) {
                        if (t.TagId == null || t.FileId == null) {
                            continue;
                        }

                        var fileId = ids[t.FileId];

                        command.Parameters["@TagId"].Value = t.TagId;
                        command.Parameters["@FileId"].Value = fileId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public void UpsertTagsOnSeries(List<SeriesTagExportDto> tags, Dictionary<string, long> ids) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    var command = new SqliteCommand("INSERT OR IGNORE INTO tag_on_series(tag_id, series_id) SELECT tag_id, @SeriesId as 'series_id' FROM tag WHERE unique_id = @TagId", db, txn);
                    command.Parameters.AddWithValue("@TagId", -1);
                    command.Parameters.AddWithValue("@SeriesId", -1);

                    foreach (var t in tags) {
                        if (t.TagId == null || t.SeriesId == null) {
                            continue;
                        }

                        var seriesId = ids[t.SeriesId];

                        command.Parameters["@TagId"].Value = t.TagId;
                        command.Parameters["@SeriesId"].Value = seriesId;
                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }
    }
}

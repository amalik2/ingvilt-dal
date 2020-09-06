using Ingvilt.Constants;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Util;

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ingvilt.Repositories {
    public class CharacterRepository {
        private static readonly string SELECT_BASE_COLUMNS_STRING = "c.character_id, c.name, c.description, c.birth_date, c.career_start_date, c.career_end_date, c.rating, c.library_id, c.cover_file_id, c.calendar_id, c.deleted, c.creator, c.unique_id";
        private static readonly string SELECT_BASE = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM character c";

        private Character ParseCharacter(SqliteDataReader query) {
            return new Character(query.GetInt64(0), query.GetString(1), query.GetString(2), QueryUtil.GetDateTime(query, 3), QueryUtil.GetDateTime(query, 4), QueryUtil.GetDateTime(query, 5), query.GetDouble(6), query.GetInt64(7), QueryUtil.GetNullableId(query, 8), QueryUtil.GetNullableId(query, 9), query.GetBoolean(10), query.GetBoolean(11), query.GetString(12));
        }

        private VideoCreator ParseVideoCreator(SqliteDataReader query) {
            var character = ParseCharacter(query);
            return new VideoCreator(character, query.GetString(13));
        }

        private void UpdateDeletedStatus(long characterId, bool deleted) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"UPDATE character SET deleted = {deleted}, deletion_due_to_cascade = false WHERE character_id = {characterId}", db);
                command.ExecuteNonQuery();
            }
        }

        private async Task<PaginationResult<Character>> GetCharacters(Pagination pagination, bool deleted, bool creator, long libraryId = DatabaseConstants.DEFAULT_ID, string nameFilter = "") {
            var libraryClause = libraryId == DatabaseConstants.DEFAULT_ID ? "" : $"c.library_id = {libraryId} AND";
            var query = $"{SELECT_BASE} WHERE {libraryClause} c.deleted = {deleted} AND c.creator = {creator} AND c.name LIKE @NameFilter";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseCharacter);
        }

        private string GetCharactersInVideo_QueryFromAndWhereClause(long videoId) {
            return $"FROM actor_for_video_character vc, character c WHERE vc.video_id = {videoId} AND vc.character_id = c.character_id AND c.deleted = false";
        }

        private List<ActorForCharacterFullDto> GetActorsForCharactersInVideo(long videoId, List<Character> characters, SqliteConnection db) {
            var characterIds = characters.Select(c => c.CharacterId);

            var actorCommand = new SqliteCommand($"SELECT a.creator_id, a.character_id, c.name, c.deleted FROM actor_for_video_character a, character c WHERE a.video_id = {videoId} AND a.creator_id = c.character_id AND a.character_id IN ({string.Join(",", characterIds)})", db);
            var actorQuery = actorCommand.ExecuteReader();

            var dtos = new List<ActorForCharacterFullDto>();
            var foundCharacters = new Dictionary<long, ActorForCharacterFullDto>();

            foreach (var character in characters) {
                var dto = new ActorForCharacterFullDto(character, -1, "", "");
                dtos.Add(dto);
                foundCharacters[character.CharacterId] = dto;
            }

            while (actorQuery.Read()) {
                var deleted = actorQuery.GetBoolean(3);
                if (deleted) {
                    continue;
                }

                var characterId = actorQuery.GetInt64(1);
                var character = characters.Find(c => c.CharacterId == characterId);
                var dto = foundCharacters[characterId];

                dto.ActorId = actorQuery.GetInt64(0);
                dto.ActorName = actorQuery.GetString(2);
            }
            return dtos;
        }

        private List<ActorForCharacterFullDto> GetCharactersInVideo(long videoId, string querySuffix) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT {SELECT_BASE_COLUMNS_STRING} {GetCharactersInVideo_QueryFromAndWhereClause(videoId)} {querySuffix}", db);
                var query = command.ExecuteReader();

                var characters = new List<Character>();
                while (query.Read()) {
                    characters.Add(ParseCharacter(query));
                }

                return GetActorsForCharactersInVideo(videoId, characters, db);
            }
        }

        private SqliteCommand GetCreateCharacterCommand(SqliteConnection db, CreateCharacterDto dto, string guid, bool ignoreDuplicates) {
            var ignoreClause = ignoreDuplicates ? "OR IGNORE" : "";

            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"INSERT {ignoreClause} INTO character(name, description, library_id, birth_date, career_start_date, career_end_date, rating, deleted, deletion_due_to_cascade, cover_file_id, calendar_id, creator, unique_id) VALUES(@Name, @Description, @LibraryId, @BirthDate, @CareerStartDate, @CareerEndDate, @Rating, false, false, @CoverFileId, @CalendarId, @IsCreator, @UniqueId)";
            command.Parameters.AddWithValue("@Name", dto.Name);
            command.Parameters.AddWithValue("@Description", dto.Description);
            command.Parameters.AddWithValue("@LibraryId", dto.LibraryId);

            command.Parameters.AddWithValue("@BirthDate", QueryUtil.GetNullableValueForStorage(dto.BirthDate));
            command.Parameters.AddWithValue("@CareerStartDate", QueryUtil.GetNullableValueForStorage(dto.CareerStartDate));
            command.Parameters.AddWithValue("@CareerEndDate", QueryUtil.GetNullableValueForStorage(dto.CareerEndDate));
            command.Parameters.AddWithValue("@Rating", dto.Rating);
            command.Parameters.AddWithValue("@CoverFileId", QueryUtil.GetNullableIdForStorage(dto.CoverMediaId));
            command.Parameters.AddWithValue("@CalendarId", QueryUtil.GetNullableIdForStorage(dto.CalendarId));
            command.Parameters.AddWithValue("@IsCreator", dto.IsCreator);
            command.Parameters.AddWithValue("@UniqueId", guid);

            return command;
        }

        public long CreateCharacter(CreateCharacterDto dto) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetCreateCharacterCommand(db, dto, UniqueIdUtil.GenerateUniqueId(), false);
                command.ExecuteNonQuery();

                return QueryUtil.GetLastInsertedPrimaryKey(db);
            }
        }

        private SqliteCommand GetUpdateCharacterCommand(SqliteConnection db, Character character, string idColumn, object idValue) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.CommandText = $"UPDATE character SET name = @Name, description = @Description, library_id = @LibraryId, birth_date = @BirthDate, career_start_date = @CareerStartDate, career_end_date = @CareerEndDate, rating = @Rating, calendar_id = @CalendarId, cover_file_id = @CoverFileId WHERE {idColumn} = @CharacterId";
            command.Parameters.AddWithValue("@Name", character.Name);
            command.Parameters.AddWithValue("@Description", character.Description);
            command.Parameters.AddWithValue("@LibraryId", character.LibraryId);

            command.Parameters.AddWithValue("@BirthDate", QueryUtil.GetNullableValueForStorage(character.BirthDate));
            command.Parameters.AddWithValue("@CareerStartDate", QueryUtil.GetNullableValueForStorage(character.CareerStartDate));
            command.Parameters.AddWithValue("@CareerEndDate", QueryUtil.GetNullableValueForStorage(character.CareerEndDate));
            command.Parameters.AddWithValue("@Rating", character.Rating);
            command.Parameters.AddWithValue("@CoverFileId", QueryUtil.GetNullableIdForStorage(character.CoverMediaId));
            command.Parameters.AddWithValue("@CharacterId", idValue);
            command.Parameters.AddWithValue("@CalendarId", QueryUtil.GetNullableIdForStorage(character.CalendarId));
            return command;
        }

        public void UpdateCharacter(Character character) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = GetUpdateCharacterCommand(db, character, "character_id", character.CharacterId);
                command.ExecuteNonQuery();
            }
        }

        public Character GetCharacter(long characterId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = SELECT_BASE + " WHERE c.character_id = @CharacterId";
                command.Parameters.AddWithValue("@CharacterId", characterId);
                var query = command.ExecuteReader();
                query.Read();
                return ParseCharacter(query);
            }
        }

        public async Task<PaginationResult<Character>> GetCharacters(Pagination pagination, bool creator) {
            return await GetCharacters(pagination, false, creator);
        }

        public async Task<PaginationResult<Character>> GetDeletedCharacters(Pagination pagination, bool creator) {
            return await GetCharacters(pagination, true, creator);
        }

        public async Task<PaginationResult<Character>> GetCharactersInLibrary(long libraryId, Pagination pagination, bool creator, string nameFilter) {
            return await GetCharacters(pagination, false, creator, libraryId, nameFilter);
        }

        public async Task<PaginationResult<Character>> GetDeletedCharactersInLibrary(long libraryId, Pagination pagination, bool creator, string nameFilter) {
            return await GetCharacters(pagination, true, creator, libraryId, nameFilter);
        }

        public void DeleteCharacter(long characterId) {
            UpdateDeletedStatus(characterId, true);
        }

        public void PermanentlyRemoveCharacter(long characterId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM character WHERE character_id = {characterId}", db);
                command.ExecuteNonQuery();
            }
        }

        public void RestoreDeletedCharacter(long characterId) {
            UpdateDeletedStatus(characterId, false);
        }

        public List<ActorForCharacterFullDto> GetCharactersInVideo(long videoId) {
            return GetCharactersInVideo(videoId, "ORDER BY c.name");
        }

        public List<Character> GetMostPopularCharactersInVideo(long videoId, int numberOfCharacters) {
            var limitClause = numberOfCharacters == 0 ? "" : $"LIMIT {numberOfCharacters}";
            var results = GetCharactersInVideo(videoId, $"ORDER BY c.rating DESC {limitClause}");
            return new List<Character>(results.Select(dto => dto.Character));
        }

        private void InsertCharacterInVideoDetails(long videoId, List<ActorForCharacterFullDto> characters, SqliteConnection db, SqliteTransaction txn) {
            foreach (var character in characters) {
                var tagCommand = new SqliteCommand($"INSERT INTO actor_for_video_character(character_id, video_id, creator_id) VALUES(@CharacterId, {videoId}, @CreatorId)", db, txn);
                tagCommand.Parameters.AddWithValue("@CharacterId", character.Character.CharacterId);
                tagCommand.Parameters.AddWithValue("@CreatorId", QueryUtil.GetNullableIdForStorage(character.ActorId));

                tagCommand.ExecuteNonQuery();
            }
        }

        public void UpdateCharactersInVideo(long videoId, List<ActorForCharacterFullDto> characters) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    var clearCommand = new SqliteCommand($@"DELETE FROM actor_for_video_character WHERE video_id = {videoId} AND character_id IN (
    SELECT character_id FROM character WHERE deleted = false
)", db, txn);
                    clearCommand.ExecuteNonQuery();

                    InsertCharacterInVideoDetails(videoId, characters, db, txn);
                    txn.Commit();
                }
            }
        }

        public void UpdateCharacterInVideo(CreatorOfVideoFullDto dto, long characterId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var clearCommand = new SqliteCommand($"UPDATE actor_for_video_character SET video_id = {dto.Video.VideoId}, character_id = {characterId}, creator_id = @ActorId WHERE video_id = {dto.Video.VideoId} AND character_id = {characterId}", db);
                clearCommand.Parameters.AddWithValue("@ActorId", QueryUtil.GetNullableIdForStorage(dto.ActorId));
                clearCommand.ExecuteNonQuery();
            }
        }

        public void RemoveCharacterFromVideo(long videoId, long characterId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var clearCommand = new SqliteCommand($@"DELETE FROM actor_for_video_character WHERE video_id = {videoId} AND character_id = {characterId}", db);
                clearCommand.ExecuteNonQuery();
            }
        }

        public void AddCharactersInVideo(long videoId, List<ActorForCharacterFullDto> characters) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    InsertCharacterInVideoDetails(videoId, characters, db, txn);
                    txn.Commit();
                }
            }
        }

        public int GetNumbersOfCharactersInVideo(long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT COUNT(*) {GetCharactersInVideo_QueryFromAndWhereClause(videoId)}", db);
                var query = command.ExecuteReader();

                query.Read();
                return query.GetInt32(0);
            }
        }

        private List<CharacterBasicDetails> GetAllCharactersInLibrary(long libraryId, bool creator) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                var command = new SqliteCommand($"SELECT character_id, name FROM character WHERE deleted = false AND library_id = {libraryId} AND creator = {creator}", db);
                var query = command.ExecuteReader();
                var list = new List<CharacterBasicDetails>();

                while (query.Read()) {
                    list.Add(new CharacterBasicDetails(query.GetInt64(0), query.GetString(1)));
                }

                return list;
            }
        }

        public List<CharacterBasicDetails> GetAllCharactersInLibrary(long libraryId) {
            return GetAllCharactersInLibrary(libraryId, false);
        }

        public List<CharacterBasicDetails> GetAllCreatorsInLibrary(long libraryId) {
            return GetAllCharactersInLibrary(libraryId, true);
        }

        private int GetNumberOfCharactersInLibrary(long libraryId, bool isCreator) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"SELECT COUNT(*) FROM character WHERE library_id = {libraryId} AND deleted = false AND creator = {isCreator}", db);
                var reader = command.ExecuteReader();

                reader.Read();
                return reader.GetInt32(0);
            }
        }

        public int GetNumberOfCharactersInLibrary(long libraryId) {
            return GetNumberOfCharactersInLibrary(libraryId, false);
        }

        public int GetNumberOfCreatorsInLibrary(long libraryId) {
            return GetNumberOfCharactersInLibrary(libraryId, true);
        }

        private async Task<PaginationResult<Character>> GetCharactersInSeries(long seriesId, Pagination pagination, string characterIdColumnName) {
            var command = $"{SELECT_BASE}, actor_for_video_character a, video v WHERE v.series_id = {seriesId} AND v.deleted = false AND v.video_id = a.video_id AND a.{characterIdColumnName} = c.character_id AND c.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseCharacter);
        }

        public async Task<PaginationResult<Character>> GetCharactersInSeries(long seriesId, Pagination pagination) {
            return await GetCharactersInSeries(seriesId, pagination, "character_id");
        }

        public async Task<PaginationResult<Character>> GetCreatorsInSeries(long seriesId, Pagination pagination) {
            return await GetCharactersInSeries(seriesId, pagination, "creator_id");
        }

        public async Task<PaginationResult<Character>> SearchForCharacters(Pagination pagination, long libraryId, List<ICharacterSearchQueryGenerator> subqueryGenerators) {
            if (subqueryGenerators.Count == 0) {
                return PaginationResult<Character>.CreateResultFromCurrentPage(new List<Character>(), pagination);
            }

            var subqueries = subqueryGenerators.Select((s) => {
                return s.GetSearchQuery();
            });
            var tables = string.Join(" INTERSECT ", subqueries);

            var command = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM character c, ({tables}) AS ci WHERE c.character_id = ci.character_id AND c.deleted = false AND c.library_id = {libraryId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseCharacter);
        }

        public void AddCharacterToVideos(Character character, List<Video> videos) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                using (var txn = db.BeginTransaction()) {
                    var dto = new ActorForCharacterFullDto(character);
                    foreach (var video in videos) {
                        InsertCharacterInVideoDetails(video.VideoId, new List<ActorForCharacterFullDto> { dto }, db, txn);
                    }
                    txn.Commit();
                }
            }
        }

        public async Task<PaginationResult<VideoCreator>> GetVideoCreators(Pagination pagination, long videoId) {
            var command = $"SELECT {SELECT_BASE_COLUMNS_STRING}, vc.role FROM character c, video_creator vc WHERE c.deleted = false AND c.creator = true AND c.character_id = vc.creator_id AND vc.video_id = {videoId}";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseVideoCreator);
        }

        public void UpdateVideoCreatorRole(VideoCreator creator, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = $"UPDATE video_creator SET role = @Role WHERE creator_id = {creator.CharacterDetails.CharacterId} AND video_id = {videoId}";
                command.Parameters.AddWithValue("@Role", creator.Role);

                command.ExecuteNonQuery();
            }
        }

        public void RemoveCreatorFromVideo(VideoCreator creator, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();
                var command = new SqliteCommand($"DELETE FROM video_creator WHERE creator_id = {creator.CharacterDetails.CharacterId} AND video_id = {videoId}", db);
                command.ExecuteNonQuery();
            }
        }

        public async Task<PaginationResult<Character>> GetVideoCreatorsNotInVideo(Pagination pagination, long videoId, long libraryId, string nameFilter) {
            var query = $"SELECT {SELECT_BASE_COLUMNS_STRING} FROM character c WHERE c.deleted = false AND c.creator = true AND c.name LIKE @NameFilter AND c.library_id = {libraryId} AND c.character_id NOT IN (SELECT vc.creator_id FROM video_creator vc WHERE vc.video_id = {videoId})";
            Action<SqliteCommand> parameterize = (command) => {
                command.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");
            };
            return await DataAccessUtil.GetPaginatedResult(pagination, query, parameterize, ParseCharacter);
        }

        public async Task AddCreatorsToVideo(List<VideoCreator> creators, long videoId) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var creator in creators) {
                        var command = new SqliteCommand();
                        command.Connection = db;
                        command.Transaction = txn;
                        command.CommandText = $"INSERT INTO video_creator(creator_id, video_id, role) VALUES({creator.CharacterDetails.CharacterId}, {videoId}, @Role)";
                        command.Parameters.AddWithValue("@Role", creator.Role);

                        command.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
        }

        public async Task<PaginationResult<Character>> GetCharactersWithFile(Pagination pagination, long fileId) {
            var command = $"{SELECT_BASE}, character_media_file vl WHERE vl.character_id = c.character_id AND vl.media_id = {fileId} AND c.deleted = false UNION {SELECT_BASE} WHERE c.cover_file_id = {fileId} AND c.deleted = false";
            return await DataAccessUtil.GetPaginatedResult(pagination, command, ParseCharacter);
        }

        private Character GetCharacter(string uniqueId, SqliteConnection db, SqliteTransaction txn) {
            var command = new SqliteCommand();
            command.Connection = db;
            command.Transaction = txn;
            command.CommandText = SELECT_BASE + " WHERE unique_id = @CharacterId";
            command.Parameters.AddWithValue("@CharacterId", uniqueId);
            var query = command.ExecuteReader();
            query.Read();
            return ParseCharacter(query);
        }

        public async Task UpsertCharacters(List<ExportedCharacterSimpleDto> characters, Dictionary<string, long> characterIds) {
            using (var db = DataAccessUtil.CreateSqlConnection()) {
                db.Open();

                using (var txn = db.BeginTransaction()) {
                    foreach (var c in characters) {
                        var character = c.Details;
                        var command = GetUpdateCharacterCommand(db, character, "unique_id", character.UniqueId);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var createDto = character.GetCreateCharacterDto();
                        command = GetCreateCharacterCommand(db, createDto, character.UniqueId, true);
                        command.Transaction = txn;
                        command.ExecuteNonQuery();

                        var retrieved = GetCharacter(character.UniqueId, db, txn);
                        characterIds[character.UniqueId] = retrieved.CharacterId;
                        character.CharacterId = retrieved.CharacterId;
                    }

                    txn.Commit();
                }
            }
        }
    }
}

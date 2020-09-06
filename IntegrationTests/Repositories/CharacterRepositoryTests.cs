using Ingvilt.Dto;
using Ingvilt.Dto.Calendars;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using Ingvilt.Util;
using IntegrationTesting.Util;
using IntegrationTestingRedo.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class CharacterRepositoryTests : BaseTest {
        private CharacterRepository repository = new CharacterRepository();
        private VideoRepository videoRepository = new VideoRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private MediaFileRepository mediaFileRepository = new MediaFileRepository();

        private Library testLibrary = null;

        private DateTime mockBirthDate = new DateTime(873, 1, 1);
        private DateTime mockCareerStartDate = new DateTime(930, 1, 1);
        private DateTime mockCareerEndDate = new DateTime(935, 1, 1);

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public CharacterRepositoryTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Character CreateAndRetrieveCharacter(CreateCharacterDto dto) {
            var id = repository.CreateCharacter(dto);
            return repository.GetCharacter(id);
        }

        private CreateCharacterDto GetNewCharacterDetails(string characterName = "Test character", bool isCreator = false) {
            return new CreateCharacterDto(characterName, "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, -1, isCreator);
        }

        private Character CreateAndRetrieveCharacter() {
            return CreateAndRetrieveCharacter(GetNewCharacterDetails());
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new CharacterSortCriteria(true));
        }

        [TestMethod]
        public void TestCreateCharacter() {
            var characterToCreate = GetNewCharacterDetails();
            var characterId = repository.CreateCharacter(characterToCreate);

            var characterRetrieved = repository.GetCharacter(characterId);

            Assert.AreEqual(characterId, characterRetrieved.CharacterId);
            Assert.AreEqual(characterToCreate.Name, characterRetrieved.Name);
            Assert.AreEqual(characterToCreate.Description, characterRetrieved.Description);
            Assert.AreEqual(characterToCreate.LibraryId, characterRetrieved.LibraryId);
            Assert.AreEqual(characterToCreate.BirthDate.Value.ToUniversalTime(), characterRetrieved.BirthDate);
            Assert.AreEqual(characterToCreate.CareerStartDate.Value.ToUniversalTime(), characterRetrieved.CareerStartDate);
            Assert.AreEqual(characterToCreate.CareerEndDate.Value.ToUniversalTime(), characterRetrieved.CareerEndDate);
            Assert.AreEqual(characterToCreate.Rating, characterRetrieved.Rating);
            Assert.AreEqual(characterToCreate.CoverMediaId, characterRetrieved.CoverMediaId);
            Assert.AreEqual(characterToCreate.CalendarId, characterRetrieved.CalendarId);
            Assert.IsNotNull(characterRetrieved.UniqueId);
        }

        [TestMethod]
        public void CreateCharacter_WithCalendar() {
            var calendarRepository = new CalendarRepository();
            var calendarId = calendarRepository.CreateCalendar(new CreateCalendarDto("MM-DD-YYYY", "", "", testLibrary.LibraryId));
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, calendarId);
            var characterId = repository.CreateCharacter(characterToCreate);

            var characterRetrieved = repository.GetCharacter(characterId);

            Assert.AreEqual(characterId, characterRetrieved.CharacterId);
            Assert.AreEqual(characterToCreate.Name, characterRetrieved.Name);
            Assert.AreEqual(characterToCreate.Description, characterRetrieved.Description);
            Assert.AreEqual(characterToCreate.LibraryId, characterRetrieved.LibraryId);
            Assert.AreEqual(characterToCreate.BirthDate.Value.ToUniversalTime(), characterRetrieved.BirthDate);
            Assert.AreEqual(characterToCreate.CareerStartDate.Value.ToUniversalTime(), characterRetrieved.CareerStartDate);
            Assert.AreEqual(characterToCreate.CareerEndDate.Value.ToUniversalTime(), characterRetrieved.CareerEndDate);
            Assert.AreEqual(characterToCreate.Rating, characterRetrieved.Rating);
            Assert.AreEqual(characterToCreate.CoverMediaId, characterRetrieved.CoverMediaId);
            Assert.AreEqual(characterToCreate.CalendarId, characterRetrieved.CalendarId);
        }

        [TestMethod]
        public void CreateCharacter_WithNullDates() {
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", null, null, null, 9, testLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);

            var characterRetrieved = repository.GetCharacter(characterId);

            Assert.AreEqual(characterId, characterRetrieved.CharacterId);
            Assert.AreEqual(characterToCreate.Name, characterRetrieved.Name);
            Assert.AreEqual(characterToCreate.Description, characterRetrieved.Description);
            Assert.AreEqual(characterToCreate.LibraryId, characterRetrieved.LibraryId);
            Assert.AreEqual(characterToCreate.BirthDate, characterRetrieved.BirthDate);
            Assert.AreEqual(characterToCreate.CareerStartDate, characterRetrieved.CareerStartDate);
            Assert.AreEqual(characterToCreate.CareerEndDate, characterRetrieved.CareerEndDate);
            Assert.AreEqual(characterToCreate.Rating, characterRetrieved.Rating);
            Assert.AreEqual(characterToCreate.CoverMediaId, characterRetrieved.CoverMediaId);
            Assert.AreEqual(characterToCreate.CalendarId, characterRetrieved.CalendarId);
        }

        [TestMethod]
        public void TestUpdateCharacter() {
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);

            var characterRetrieved = repository.GetCharacter(characterId);
            var fileId = mediaFileRepository.CreateMediaFile(new CreateMediaFileDto("C:/test.jpg", MediaFileType.IMAGE_TYPE, "test"));
            characterRetrieved.Description += "1";
            characterRetrieved.Name += "2";
            characterRetrieved.BirthDate = new DateTime(1, 1, 1);
            characterRetrieved.CareerStartDate = new DateTime(2, 2, 2);
            characterRetrieved.CareerEndDate = new DateTime(3, 3, 3);
            characterRetrieved.Rating = 3.99;
            repository.UpdateCharacter(characterRetrieved);

            var updatedCharacterRetrieved = repository.GetCharacter(characterId);

            Assert.AreEqual(characterRetrieved.CharacterId, updatedCharacterRetrieved.CharacterId);
            Assert.AreEqual(characterRetrieved.Name, updatedCharacterRetrieved.Name);
            Assert.AreEqual(characterRetrieved.Description, updatedCharacterRetrieved.Description);
            Assert.AreEqual(characterRetrieved.LibraryId, updatedCharacterRetrieved.LibraryId);
            Assert.AreEqual(characterRetrieved.BirthDate.Value.ToUniversalTime(), updatedCharacterRetrieved.BirthDate);
            Assert.AreEqual(characterRetrieved.CareerStartDate.Value.ToUniversalTime(), updatedCharacterRetrieved.CareerStartDate);
            Assert.AreEqual(characterRetrieved.CareerEndDate.Value.ToUniversalTime(), updatedCharacterRetrieved.CareerEndDate);
            Assert.AreEqual(characterRetrieved.Rating, updatedCharacterRetrieved.Rating);
            Assert.AreEqual(characterRetrieved.CalendarId, updatedCharacterRetrieved.CalendarId);
            Assert.AreEqual(characterRetrieved.CoverMediaId, updatedCharacterRetrieved.CoverMediaId);

            CollectionAssert.AreEquivalent(new List<Character>() { updatedCharacterRetrieved }, repository.GetCharacters(GetFirstPage(), false).Result.Results);
        }

        [TestMethod]
        public void TestUpdateCharacter_ShouldNotUpdateDifferentCharacter() {
            var characterDto = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, -1);

            var characterToUpdateId = repository.CreateCharacter(characterDto);
            var characterNotUpdatedId = repository.CreateCharacter(characterDto);

            var characterToUpdate = repository.GetCharacter(characterToUpdateId);
            characterToUpdate.Description += "1";
            repository.UpdateCharacter(characterToUpdate);

            var characterToNotUpdate = repository.GetCharacter(characterNotUpdatedId);

            Assert.AreNotEqual(characterToUpdate.Description, characterToNotUpdate.Description);
        }

        [TestMethod]
        public void TestGetCharactersWithNoneCreated() {
            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;
            Assert.AreEqual(0, characters.Count);
        }

        [TestMethod]
        public void GetCharacters_ShouldNotReturnCreators() {
            var character = CreateAndRetrieveCharacter(GetNewCharacterDetails("name", true));
            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;

            CollectionAssert.AreEquivalent(new List<Character>(), characters);
        }

        [TestMethod]
        public void TestGetCharactersWithOneCreated() {
            var character = CreateAndRetrieveCharacter();
            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;

            var expectedCharacters = new List<Character>();
            expectedCharacters.Add(character);

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void TestGetCharactersWithMultipleCreated() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 5; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }

            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetCharacters_ShouldntReturnDeletedCharacters() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 5; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }
            var deletedCharacter = CreateAndRetrieveCharacter();
            repository.DeleteCharacter(deletedCharacter.CharacterId);

            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetDeletedCharacters_ShouldOnlyReturnDeletedCharacters() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 5; ++i) {
                var character = CreateAndRetrieveCharacter();
                repository.DeleteCharacter(character.CharacterId);
                expectedCharacters.Add(character);
                character.Deleted = true;
            }
            CreateAndRetrieveCharacter();

            var characters = repository.GetDeletedCharacters(GetFirstPage(), false).Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void ShouldNotDeleteCharacter_WhenDifferentLibraryDeleted() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, otherLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);
            var characterRetrieved = repository.GetCharacter(characterId);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            CollectionAssert.AreEquivalent(new List<Character>(), repository.GetDeletedCharacters(GetFirstPage(), false).Result.Results);
            CollectionAssert.AreEquivalent(new List<Character>() { characterRetrieved }, repository.GetCharacters(GetFirstPage(), false).Result.Results);
        }

        [TestMethod]
        public void ShouldUndeleteCharacter_WhenLibraryRestored() {
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var characterRetrieved = repository.GetCharacter(characterId);

            CollectionAssert.AreEquivalent(new List<Character>(), repository.GetDeletedCharacters(GetFirstPage(), false).Result.Results);
            CollectionAssert.AreEquivalent(new List<Character>() { characterRetrieved }, repository.GetCharacters(GetFirstPage(), false).Result.Results);
        }

        [TestMethod]
        public void ShouldNotUndeleteCharacter_WhenDifferentLibraryRestored() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test 2"));
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, otherLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);

            repository.DeleteCharacter(characterId);
            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var characterRetrieved = repository.GetCharacter(characterId);

            CollectionAssert.AreEquivalent(new List<Character>() { characterRetrieved }, repository.GetDeletedCharacters(GetFirstPage(), false).Result.Results);
            CollectionAssert.AreEquivalent(new List<Character>(), repository.GetCharacters(GetFirstPage(), false).Result.Results);
        }

        [TestMethod]
        public void ShouldNotRestoreCharacterDeletedNormally_WhenLibraryRestored() {
            var characterToCreate = new CreateCharacterDto("Test library", "test desc", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, testLibrary.LibraryId, -1, -1);
            var characterId = repository.CreateCharacter(characterToCreate);
            repository.DeleteCharacter(characterId);

            libraryRepository.DeleteLibrary(testLibrary.LibraryId);
            libraryRepository.RestoreDeletedLibrary(testLibrary.LibraryId);
            var characterRetrieved = repository.GetCharacter(characterId);

            CollectionAssert.AreEquivalent(new List<Character>() { characterRetrieved }, repository.GetDeletedCharacters(GetFirstPage(), false).Result.Results);
            CollectionAssert.AreEquivalent(new List<Character>(), repository.GetCharacters(GetFirstPage(), false).Result.Results);
        }

        [TestMethod]
        public void GetCharactersInLibrary_ShouldntReturnCreators() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 2; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }
            CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));

            var characters = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetCharactersInLibrary_ShouldReturnCreators() {
            var expectedCharacters = new List<Character>();
            var creator = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));
            expectedCharacters.Add(creator);

            for (int i = 0; i < 2; ++i) {
                CreateAndRetrieveCharacter();
            }

            var characters = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), true, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetCharactersInLibrary_ShouldntReturnDeletedCharacters() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 3; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }
            var deletedCharacter = CreateAndRetrieveCharacter();
            repository.DeleteCharacter(deletedCharacter.CharacterId);

            var characters = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetCharactersInLibrary_ShouldntReturnCharactersInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            repository.CreateCharacter(new CreateCharacterDto("test", "", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, otherLibrary.LibraryId, -1, -1));

            var characters = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), characters);
        }

        [TestMethod]
        public void GetDeletedCharactersInLibrary_ShouldOnlyReturnDeletedCharacters() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 3; ++i) {
                var character = CreateAndRetrieveCharacter();
                repository.DeleteCharacter(character.CharacterId);
                expectedCharacters.Add(character);
                character.Deleted = true;
            }
            CreateAndRetrieveCharacter();

            var characters = repository.GetDeletedCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetDeletedCharactersInLibrary_ShouldntReturnCharactersInOtherLibrary() {
            var otherLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
            var characterId = repository.CreateCharacter(new CreateCharacterDto("test", "", mockBirthDate, mockCareerStartDate, mockCareerEndDate, 9, otherLibrary.LibraryId, -1, -1));
            repository.GetCharacter(characterId);
            repository.DeleteCharacter(characterId);

            var characters = repository.GetDeletedCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), characters);
        }

        [TestMethod]
        public void RestoreDeleteCharacter_ShouldUndeleteCharacter() {
            var expectedCharacters = new List<Character>();
            for (int i = 0; i < 2; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }
            var deletedCharacter = CreateAndRetrieveCharacter();
            expectedCharacters.Add(deletedCharacter);
            repository.DeleteCharacter(deletedCharacter.CharacterId);
            repository.RestoreDeletedCharacter(deletedCharacter.CharacterId);

            var characters = repository.GetCharacters(GetFirstPage(), false).Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, characters);
        }

        [TestMethod]
        public void GetCharactersInVideo_ShouldReturnCharactersSortedAlphabeticallyByName() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var expectedCharacters = new List<Character>();

            for (int i = 0; i < 3; ++i) {
                var character = CreateAndRetrieveCharacter(GetNewCharacterDetails("test: " + i));
                expectedCharacters.Add(character);
            }

            var characters = new List<ActorForCharacterFullDto>(expectedCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(videoId, characters);

            var actualCharacters = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEqual(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void GetCharactersInVideo_ShouldNotReturnDeletedCharacters() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var expectedCharacters = new List<Character>();

            for (int i = 0; i < 3; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }
            var deletedCharacter = CreateAndRetrieveCharacter();

            var characters = new List<ActorForCharacterFullDto>(expectedCharacters.Select(c => new ActorForCharacterFullDto(c)));
            characters.Add(new ActorForCharacterFullDto(deletedCharacter));
            repository.AddCharactersInVideo(videoId, characters);

            repository.DeleteCharacter(deletedCharacter.CharacterId);

            var actualCharacters = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void GetCharactersInVideo_ShouldNotReturnCharactersInOtherVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var otherVideoId = videoRepository.CreateVideo(videoDto);

            var expectedCharacters = new List<Character>();

            for (int i = 0; i < 2; ++i) {
                CreateAndRetrieveCharacter();
            }

            var characters = new List<ActorForCharacterFullDto>(expectedCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(otherVideoId, characters);

            var actualCharacters = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEquivalent(new List<Character>(), new List<Character>(actualCharacters.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void UpdateCharactersInVideo_ShouldDeleteOldCharacters() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var expectedCharacters = new List<Character>();

            for (int i = 0; i < 1; ++i) {
                var character = CreateAndRetrieveCharacter();
                expectedCharacters.Add(character);
            }

            var characterIds = new List<ActorForCharacterFullDto>(expectedCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(videoId, characterIds);
            repository.UpdateCharactersInVideo(videoId, new List<ActorForCharacterFullDto>());

            var charactersInVideo = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEquivalent(new List<ActorForCharacterFullDto>(), charactersInVideo);
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldAddNewTags() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var oldCharacters = new List<Character>();
            var newCharacters = new List<Character>();

            for (int i = 0; i < 1; ++i) {
                var character = CreateAndRetrieveCharacter();
                oldCharacters.Add(character);
            }

            var characterIds = new List<ActorForCharacterFullDto>(oldCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(videoId, characterIds);

            for (int i = 0; i < 2; ++i) {
                var character = CreateAndRetrieveCharacter();
                newCharacters.Add(character);
            }

            characterIds = new List<ActorForCharacterFullDto>(newCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.UpdateCharactersInVideo(videoId, characterIds);

            var charactersInVideo = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEquivalent(newCharacters, new List<Character>(charactersInVideo.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldNotDeleteSoftDeletedTags() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var oldCharacters = new List<Character>();

            for (int i = 0; i < 1; ++i) {
                var character = CreateAndRetrieveCharacter();
                oldCharacters.Add(character);
            }

            var characterIds = new List<ActorForCharacterFullDto>(oldCharacters.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(videoId, characterIds);
            repository.DeleteCharacter(oldCharacters[0].CharacterId);
            repository.UpdateCharactersInVideo(videoId, new List<ActorForCharacterFullDto>());
            repository.RestoreDeletedCharacter(oldCharacters[0].CharacterId);

            var charactersInVideo = repository.GetCharactersInVideo(videoId);
            CollectionAssert.AreEquivalent(oldCharacters, new List<Character>(charactersInVideo.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldNotDeleteTagsFromOtherVideos() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var videoId = videoRepository.CreateVideo(videoDto);
            var otherVideoId = videoRepository.CreateVideo(videoDto);

            var charactersInVideo = new List<Character>();
            var charactersInOtherVideo = new List<Character>();

            for (int i = 0; i < 1; ++i) {
                var character = CreateAndRetrieveCharacter();
                charactersInVideo.Add(character);
                charactersInOtherVideo.Add(character);
            }

            var characterIds = new List<ActorForCharacterFullDto>(charactersInVideo.Select(c => new ActorForCharacterFullDto(c)));
            repository.AddCharactersInVideo(videoId, characterIds);
            repository.AddCharactersInVideo(otherVideoId, characterIds);
            repository.UpdateCharactersInVideo(videoId, new List<ActorForCharacterFullDto>());

            var retrievedCharacters = repository.GetCharactersInVideo(videoId);
            var retrievedCharactersInOtherVideo = repository.GetCharactersInVideo(otherVideoId);

            CollectionAssert.AreEquivalent(charactersInOtherVideo, new List<Character>(retrievedCharactersInOtherVideo.Select(dto => dto.Character)));
            CollectionAssert.AreEquivalent(new List<ActorForCharacterFullDto>(), retrievedCharacters);
        }

        [TestMethod]
        public void SearchForCharacters_Name_PartialMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterNameQueryGenerator("tes"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>() { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_Name_NoMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterNameQueryGenerator("1"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithoutName_PartialMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithoutNameQueryGenerator("tes"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithoutName_NoMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithoutNameQueryGenerator("1"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>() { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithTags_PartialMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag3 });

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithTags_Match() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag2, tag3 });

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithTagsQueryGenerator("[\"tag 1\", \"tag 3\", \"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>() { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithTags_MultipleCharactersCombinedMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var character2Dto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character2 = CreateAndRetrieveCharacter(character2Dto);

            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag3 });
            tagRepository.AddTagsToCharacter(character2.CharacterId, new List<long> { tag2, tag3 });

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithoutTags_Match() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag3 });

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithoutTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithoutTags_NoMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag3 });

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithoutTagsQueryGenerator("[\"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character> { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_WithoutTags_WithNoTagsCreated() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterWithoutTagsQueryGenerator("[\"tag 2\"]"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character> { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_RatingGreaterThan_Less() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Rating = 0;
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_RatingGreaterThan_Greater() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Rating = 2;
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character> { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_Description_PartialMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Description = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterDescriptionQueryGenerator("tes"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character> { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_Description_NoMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Description = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterDescriptionQueryGenerator("1"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_AgeGreaterThan_Less() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.BirthDate = new DateTime(DateTime.Now.Year - 5, DateTime.Now.Month, DateTime.Now.Day);
            CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterAgeGreaterThanQueryGenerator("6"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void SearchForCharacters_AgeGreaterThan_Greater() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.BirthDate = new DateTime(DateTime.Now.Year - 5, DateTime.Now.Month, DateTime.Now.Day);
            var character = CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterAgeGreaterThanQueryGenerator("4"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character> { character }, videos);
        }

        [TestMethod]
        public void SearchForCharacters_AgeGreaterThan_NoBirthDate() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            CreateAndRetrieveCharacter(characterDto);

            var queries = new List<ICharacterSearchQueryGenerator>();
            queries.Add(new CharacterAgeGreaterThanQueryGenerator("0"));

            var videos = repository.SearchForCharacters(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Character>(), videos);
        }

        [TestMethod]
        public void AddCharacterToVideos_ShouldOnlyAddToSpecifiedVideos() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video1 = videoRepository.GetVideo(video1Id).Result;
            var video2Id = videoRepository.CreateVideo(videoDto);

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            repository.AddCharacterToVideos(character, new List<Video> { video1 });

            var actualCharacters = repository.GetCharactersInVideo(video1Id);
            var charactersInSecondVideo = repository.GetCharactersInVideo(video2Id);
            var expectedCharacters = new List<Character> { character };

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.Character)));
            CollectionAssert.AreEquivalent(new List<Character>(), charactersInSecondVideo);
        }

        [TestMethod]
        public void AddCharacterToVideos_ShouldntRemoveExistingCharacters() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video1 = videoRepository.GetVideo(video1Id).Result;

            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var characterAlreadyInVideo = CreateAndRetrieveCharacter(characterDto);

            repository.AddCharactersInVideo(video1Id, new List<ActorForCharacterFullDto> { new ActorForCharacterFullDto(characterAlreadyInVideo) });
            repository.AddCharacterToVideos(character, new List<Video> { video1 });

            var actualCharacters = repository.GetCharactersInVideo(video1Id);
            var expectedCharacters = new List<Character> { character, characterAlreadyInVideo };

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.Character)));
        }

        [TestMethod]
        public void GetVideoCreators_WithNoCreators() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);

            var actualCharacters = repository.GetVideoCreators(GetFirstPage(), video1Id).Result.Results;
            var expectedCharacters = new List<Character> {};

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.CharacterDetails)));
        }

        [TestMethod]
        public void GetVideoCreators_MultipleVideos() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video2Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));
            var creator2 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test 2", true));

            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator1, "role test") }, video1Id
            );
            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator2, "role 2 test") }, video2Id
            );

            var actualCharacters = repository.GetVideoCreators(GetFirstPage(), video1Id).Result.Results;
            var expectedCharacters = new List<Character> { creator1 };

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.CharacterDetails)));
            Assert.AreEqual("role test", actualCharacters[0].Role);
        }

        [TestMethod]
        public void UpdateVideoCreatorRole_ShouldUpdateRoleInVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video2Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));
            var videoCreator1 = new VideoCreator(creator1, "role test");

            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { videoCreator1 }, video1Id
            );
            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator1, "role 2 test") }, video2Id
            );

            videoCreator1.Role = "new role";
            repository.UpdateVideoCreatorRole(videoCreator1, video1Id);

            var actualCharacters = repository.GetVideoCreators(GetFirstPage(), video1Id).Result.Results;
            var expectedCharacters = new List<Character> { creator1 };

            var video2Chars = repository.GetVideoCreators(GetFirstPage(), video2Id).Result.Results;

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.CharacterDetails)));
            Assert.AreEqual(videoCreator1.Role, actualCharacters[0].Role);
            Assert.AreEqual("role 2 test", video2Chars[0].Role);
        }

        [TestMethod]
        public void RemoveCreatorFromVideo_ShouldRemoveFromCorrectVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video2Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));
            var creator2 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test 2", true));
            var videoCreator1 = new VideoCreator(creator1, "role test");

            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { videoCreator1 }, video1Id
            );
            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator2, "role 2 test") }, video2Id
            );

            repository.RemoveCreatorFromVideo(videoCreator1, video1Id);

            var actualCharacters = repository.GetVideoCreators(GetFirstPage(), video1Id).Result.Results;
            var actualCharactersInSecondVideo = repository.GetVideoCreators(GetFirstPage(), video2Id).Result.Results;
            var expectedCharacters = new List<Character> {};

            CollectionAssert.AreEquivalent(expectedCharacters, new List<Character>(actualCharacters.Select(dto => dto.CharacterDetails)));
            CollectionAssert.AreEquivalent(new List<Character> { creator2 }, new List<Character>(actualCharactersInSecondVideo.Select(dto => dto.CharacterDetails)));
        }

        [TestMethod]
        public void GetVideoCreatorsNotInVideo_WithNoCreators() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);

            var actualCharacters = repository.GetVideoCreatorsNotInVideo(GetFirstPage(), video1Id, testLibrary.LibraryId, "").Result.Results;
            var expectedCharacters = new List<Character> { };

            CollectionAssert.AreEquivalent(expectedCharacters, actualCharacters);
        }

        [TestMethod]
        public void GetVideoCreatorsNotInVideo_WithCreatorNotInVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));

            var actualCharacters = repository.GetVideoCreatorsNotInVideo(GetFirstPage(), video1Id, testLibrary.LibraryId, "").Result.Results;
            var expectedCharacters = new List<Character> { creator1 };

            CollectionAssert.AreEquivalent(expectedCharacters, actualCharacters);
        }

        [TestMethod]
        public void GetVideoCreatorsNotInVideo_WithCreatorInOtherVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);
            var video2Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));

            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator1, "role 2 test") }, video2Id
            );

            var actualCharacters = repository.GetVideoCreatorsNotInVideo(GetFirstPage(), video1Id, testLibrary.LibraryId, "").Result.Results;
            var expectedCharacters = new List<Character> { creator1 };

            CollectionAssert.AreEquivalent(expectedCharacters, actualCharacters);
        }

        [TestMethod]
        public void GetVideoCreatorsNotInVideo_WithCreatorInVideo() {
            var videoDto = CreateVideoUtil.GetNewVideoDetails(testLibrary.LibraryId, -1, -1);
            var video1Id = videoRepository.CreateVideo(videoDto);

            var creator1 = CreateAndRetrieveCharacter(GetNewCharacterDetails("test", true));

            repository.AddCreatorsToVideo(
                new List<VideoCreator>() { new VideoCreator(creator1, "role 2 test") }, video1Id
            );

            var actualCharacters = repository.GetVideoCreatorsNotInVideo(GetFirstPage(), video1Id, testLibrary.LibraryId, "").Result.Results;
            var expectedCharacters = new List<Character> {};

            CollectionAssert.AreEquivalent(expectedCharacters, actualCharacters);
        }

        [TestMethod]
        public void UpsertCharacters_ShouldInsertNewItems() {
            var characters = new List<ExportedCharacterSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var c = new Character(-1, "t" + i, "", null, null, null, 0, testLibrary.LibraryId, -1, -1, false, false, UniqueIdUtil.GenerateUniqueId());
                characters.Add(new ExportedCharacterSimpleDto(c, null, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertCharacters(characters, ids);

            var retChars = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;
            var expectedIds = new Dictionary<string, long>();
            foreach (var s in retChars) {
                expectedIds[s.UniqueId] = s.CharacterId;
            }

            var expectedChars = characters.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedChars, retChars);
            CollectionAssert.AreEquivalent(expectedIds, ids);
        }

        [TestMethod]
        public void UpsertCharacters_ShouldUpdateExistingItems() {
            var characters = new List<ExportedCharacterSimpleDto>();

            for (int i = 0; i < 3; ++i) {
                var c = new Character(-1, "t" + i, "", null, null, null, 0, testLibrary.LibraryId, -1, -1, false, false, UniqueIdUtil.GenerateUniqueId());
                characters.Add(new ExportedCharacterSimpleDto(c, null, null));
            }

            var ids = new Dictionary<string, long>();
            repository.UpsertCharacters(characters, ids);
            characters[0].Details.Name = "new 0";
            characters[2].Details.Name = "new 2";
            repository.UpsertCharacters(characters, ids);

            var retChars = repository.GetCharactersInLibrary(testLibrary.LibraryId, GetFirstPage(), false, "").Result.Results;

            var expectedChars = characters.Select(p => p.Details).ToList();
            CollectionAssert.AreEquivalent(expectedChars, retChars);
        }
    }
}

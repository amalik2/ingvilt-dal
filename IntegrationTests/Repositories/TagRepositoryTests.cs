using Ingvilt.Constants;
using Ingvilt.Dto;
using Ingvilt.Dto.Export;
using Ingvilt.Dto.Tags;
using Ingvilt.Repositories;
using Ingvilt.Services;
using IntegrationTesting.Util;
using IntegrationTestingRedo.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class TagRepositoryTests : BaseTest {
        private TagRepository repository = new TagRepository();
        private VideoRepository videoRepository = new VideoRepository();
        private CharacterService characterService = new CharacterService();
        private Library library;

        private long GetNewVideoId() {
            var dto = CreateVideoUtil.GetNewVideoDetails(library.LibraryId, -1, -1);
            return videoRepository.CreateVideo(dto);
        }

        private long GetNewCharacterId() {
            var dto = CreateCharacterUtil.GetNewCharacterDetails(library.LibraryId);
            return characterService.CreateCharacter(dto);
        }

        public TagRepositoryTests() {
            var libraryRepository = new LibraryRepository();
            var libId = libraryRepository.CreateLibrary(new CreateLibraryDto("test"));
            library = libraryRepository.GetLibrary(libId);
        }

        [TestMethod]
        public void TestCreateVideoTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateVideoTag(new CreateVideoTagDto(tagName, tagType));
            var tag = repository.GetVideoTag(tagId);

            Assert.AreEqual(tagName, tag.Name);
            Assert.AreEqual(tagType, tag.Type);
            Assert.AreEqual(tagId, tag.TagId);
        }

        [TestMethod]
        public void TestCreateCharacterTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto(tagName, tagType));
            var tag = repository.GetCharacterTag(tagId);

            Assert.AreEqual(tagName, tag.Name);
            Assert.AreEqual(tagType, tag.Type);
            Assert.AreEqual(tagId, tag.TagId);
        }

        [TestMethod]
        public void GetVideoTag_ShouldReturnDeletedTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateVideoTag(new CreateVideoTagDto(tagName, tagType));
            var tag = repository.GetVideoTag(tagId);

            repository.DeleteVideoTag(tag);
            Assert.AreEqual(tag, repository.GetVideoTag(tagId));
        }

        [TestMethod]
        public void GetCharacterTag_ShouldReturnDeletedTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto(tagName, tagType));
            var tag = repository.GetCharacterTag(tagId);

            repository.DeleteCharacterTag(tag);
            Assert.AreEqual(tag, repository.GetCharacterTag(tagId));
        }

        [TestMethod]
        public void GetVideoTag_ShouldntReturnPermanentlyDeletedTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateVideoTag(new CreateVideoTagDto(tagName, tagType));
            var tag = repository.GetVideoTag(tagId);

            repository.PermanentlyRemoveVideoTag(tag);
            try {
                repository.GetVideoTag(tagId);
                Assert.IsFalse(true, "A deleted video tag was returned");
            } catch (ArgumentException) {
            }
        }

        [TestMethod]
        public void GetCharacterTag_ShouldntReturnPermanentlyDeletedTag() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto(tagName, tagType));
            var tag = repository.GetCharacterTag(tagId);

            repository.PermanentlyRemoveCharacterTag(tag);
            try {
                repository.GetCharacterTag(tagId);
                Assert.IsFalse(true, "A deleted character tag was returned");
            } catch (ArgumentException) {

            }
        }

        [TestMethod]
        public void GetVideoTags_ShouldReturnNonDeletedTags() {
            var tags = new List<VideoTag>();
            for (int i = 0; i < 3; ++i) {
                var tagId = repository.CreateVideoTag(new CreateVideoTagDto("name", "type"));
                tags.Add(repository.GetVideoTag(tagId));
            }

            var deletedTagId = repository.CreateVideoTag(new CreateVideoTagDto("name", "type"));
            var deletedTag = repository.GetVideoTag(deletedTagId);
            repository.DeleteVideoTag(deletedTag);

            CollectionAssert.AreEquivalent(tags, repository.GetVideoTags());
        }

        [TestMethod]
        public void GetDeletedVideoTags_ShouldReturnDeletedTags() {
            var tags = new List<VideoTag>();
            for (int i = 0; i < 3; ++i) {
                var tagId = repository.CreateVideoTag(new CreateVideoTagDto("name", "type"));
                repository.GetVideoTag(tagId);
            }

            for (int i = 0; i < 3; ++i) {
                var deletedTagId = repository.CreateVideoTag(new CreateVideoTagDto("name", "type"));
                var deletedTag = repository.GetVideoTag(deletedTagId);
                tags.Add(deletedTag);
                repository.DeleteVideoTag(deletedTag);
            }

            CollectionAssert.AreEquivalent(tags, repository.GetDeletedVideoTags());
        }

        [TestMethod]
        public void GetCharacterTags_ShouldReturnNonDeletedTags() {
            var tags = new List<CharacterTag>();
            for (int i = 0; i < 3; ++i) {
                var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto("name", "type"));
                tags.Add(repository.GetCharacterTag(tagId));
            }

            var deletedTagId = repository.CreateCharacterTag(new CreateCharacterTagDto("name", "type"));
            var deletedTag = repository.GetCharacterTag(deletedTagId);
            repository.DeleteCharacterTag(deletedTag);

            CollectionAssert.AreEquivalent(tags, repository.GetCharacterTags());
        }

        [TestMethod]
        public void GetDeletedCharacterTags_ShouldReturnDeletedTags() {
            var tags = new List<CharacterTag>();
            for (int i = 0; i < 3; ++i) {
                var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto("name", "type"));
                repository.GetCharacterTag(tagId);
            }

            for (int i = 0; i < 3; ++i) {
                var deletedTagId = repository.CreateCharacterTag(new CreateCharacterTagDto("name", "type"));
                var deletedTag = repository.GetCharacterTag(deletedTagId);
                tags.Add(deletedTag);
                repository.DeleteCharacterTag(deletedTag);
            }

            CollectionAssert.AreEquivalent(tags, repository.GetDeletedCharacterTags());
        }

        [TestMethod]
        public void GetTagsOnVideo_WithNoTagsOnVideo() {
            var videoId = GetNewVideoId();
            var tags = repository.GetTagsOnVideo(videoId);

            CollectionAssert.AreEquivalent(new List<Tag>(), tags);
        }

        [TestMethod]
        public void GetTagsOnVideo_WithTagsOnVideo() {
            var videoId = GetNewVideoId();
            var tagsOnVideo = new List<long>();

            for (int i = 0; i < 3; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                tagsOnVideo.Add(newTag);
            }
            repository.CreateVideoTag(new CreateVideoTagDto("not on video", ""));

            repository.AddTagsToVideo(videoId, tagsOnVideo);
            var tags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);

            CollectionAssert.AreEquivalent(tagsOnVideo, new List<long>(tags));
        }

        [TestMethod]
        public void GetTagsOnVideo_ShouldntReturnDeletedTags() {
            var videoId = GetNewVideoId();
            var tagsOnVideo = new List<long>();

            for (int i = 0; i < 3; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                tagsOnVideo.Add(newTag);
            }
            var deletedTagId = repository.CreateVideoTag(new CreateVideoTagDto("deleted tag", ""));
            var deletedTag = repository.GetVideoTag(deletedTagId);
            tagsOnVideo.Add(deletedTagId);

            repository.AddTagsToVideo(videoId, tagsOnVideo);
            repository.DeleteVideoTag(deletedTag);

            tagsOnVideo.Remove(deletedTagId);
            var tags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(tagsOnVideo, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldDeleteOldTags() {
            var videoId = GetNewVideoId();
            var tagsOnVideo = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                tagsOnVideo.Add(newTag);
            }

            repository.AddTagsToVideo(videoId, tagsOnVideo);
            repository.UpdateTagsOnVideo(videoId, new List<long>());

            var tags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(new List<long>(), new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldAddNewTags() {
            var videoId = GetNewVideoId();

            var oldTagsOnVideo = new List<long>();
            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                oldTagsOnVideo.Add(newTag);
            }
            repository.AddTagsToVideo(videoId, oldTagsOnVideo);

            var newTagsOnVideo = new List<long>();
            for (int i = 0; i < 2; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("new " + i, ""));
                newTagsOnVideo.Add(newTag);
            }

            repository.UpdateTagsOnVideo(videoId, newTagsOnVideo);

            var tags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(newTagsOnVideo, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldNotDeleteSoftDeletedTags() {
            var videoId = GetNewVideoId();
            var tagsOnVideo = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var undeletedTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                tagsOnVideo.Add(undeletedTag);
            }
            var deletedTagId = repository.CreateVideoTag(new CreateVideoTagDto("deleted", ""));
            var deletedTag = repository.GetVideoTag(deletedTagId);
            tagsOnVideo.Add(deletedTagId);

            repository.AddTagsToVideo(videoId, tagsOnVideo);
            repository.DeleteVideoTag(deletedTag);
            repository.UpdateTagsOnVideo(videoId, new List<long>());
            repository.RestoreVideoTag(deletedTag);

            var tags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(new List<long>() { deletedTagId }, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnVideo_ShouldNotDeleteTagsFromOtherVideos() {
            var videoId = GetNewVideoId();
            var otherVideoId = GetNewVideoId();

            var tagsOnVideo = new List<long>();
            var tagsOnOtherVideo = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateVideoTag(new CreateVideoTagDto("test " + i, ""));
                tagsOnVideo.Add(newTag);
                tagsOnOtherVideo.Add(newTag);
            }

            repository.AddTagsToVideo(otherVideoId, tagsOnOtherVideo);
            repository.AddTagsToVideo(videoId, tagsOnVideo);
            repository.UpdateTagsOnVideo(videoId, new List<long>());

            var retrievedTags = repository.GetTagsOnVideo(videoId).Select(t => t.TagId);
            var retrievedTagsOnOtherVideo = repository.GetTagsOnVideo(otherVideoId).Select(t => t.TagId);

            CollectionAssert.AreEquivalent(tagsOnOtherVideo, new List<long>(retrievedTagsOnOtherVideo));
            CollectionAssert.AreEquivalent(new List<long>(), new List<long>(retrievedTags));
        }

        [TestMethod]
        public void GetTagsOnCharacter_WithNoTagsOnCharacter() {
            var characterId = GetNewCharacterId();
            var tags = repository.GetTagsOnCharacter(characterId);

            CollectionAssert.AreEquivalent(new List<Tag>(), tags);
        }

        [TestMethod]
        public void GetTagsOnCharacter_WithTagsOnCharacter() {
            var characterId = GetNewCharacterId();
            var tagsOnCharacter = new List<long>();

            for (int i = 0; i < 3; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                tagsOnCharacter.Add(newTag);
            }
            repository.CreateCharacterTag(new CreateCharacterTagDto("not on character", ""));

            repository.AddTagsToCharacter(characterId, tagsOnCharacter);
            var tags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);

            CollectionAssert.AreEquivalent(tagsOnCharacter, new List<long>(tags));
        }

        [TestMethod]
        public void GetTagsOnCharacter_ShouldntReturnDeletedTags() {
            var characterId = GetNewCharacterId();
            var tagsOnCharacter = new List<long>();

            for (int i = 0; i < 3; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                tagsOnCharacter.Add(newTag);
            }
            var deletedTagId = repository.CreateCharacterTag(new CreateCharacterTagDto("deleted tag", ""));
            var deletedTag = repository.GetCharacterTag(deletedTagId);
            tagsOnCharacter.Add(deletedTagId);

            repository.AddTagsToCharacter(characterId, tagsOnCharacter);
            repository.DeleteCharacterTag(deletedTag);

            tagsOnCharacter.Remove(deletedTagId);
            var tags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(tagsOnCharacter, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnCharacter_ShouldDeleteOldTags() {
            var characterId = GetNewCharacterId();
            var tagsOnCharacter = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                tagsOnCharacter.Add(newTag);
            }

            repository.AddTagsToCharacter(characterId, tagsOnCharacter);
            repository.UpdateTagsOnCharacter(characterId, new List<long>());

            var tags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(new List<long>(), new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnCharacter_ShouldAddNewTags() {
            var characterId = GetNewCharacterId();

            var oldTagsOnCharacter = new List<long>();
            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                oldTagsOnCharacter.Add(newTag);
            }
            repository.AddTagsToCharacter(characterId, oldTagsOnCharacter);

            var newTagsOnCharacter = new List<long>();
            for (int i = 0; i < 2; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("new " + i, ""));
                newTagsOnCharacter.Add(newTag);
            }

            repository.UpdateTagsOnCharacter(characterId, newTagsOnCharacter);

            var tags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(newTagsOnCharacter, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnCharacter_ShouldNotDeleteSoftDeletedTags() {
            var characterId = GetNewCharacterId();
            var tagsOnCharacter = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var undeletedTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                tagsOnCharacter.Add(undeletedTag);
            }
            var deletedTagId = repository.CreateCharacterTag(new CreateCharacterTagDto("deleted", ""));
            var deletedTag = repository.GetCharacterTag(deletedTagId);
            tagsOnCharacter.Add(deletedTagId);

            repository.AddTagsToCharacter(characterId, tagsOnCharacter);
            repository.DeleteCharacterTag(deletedTag);
            repository.UpdateTagsOnCharacter(characterId, new List<long>());
            repository.RestoreCharacterTag(deletedTag);

            var tags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);
            CollectionAssert.AreEquivalent(new List<long>() { deletedTagId }, new List<long>(tags));
        }

        [TestMethod]
        public void UpdateTagsOnCharacter_ShouldNotDeleteTagsFromOtherCharacters() {
            var characterId = GetNewCharacterId();
            var otherCharacterId = GetNewCharacterId();

            var tagsOnCharacter = new List<long>();
            var tagsOnOtherCharacter = new List<long>();

            for (int i = 0; i < 1; ++i) {
                var newTag = repository.CreateCharacterTag(new CreateCharacterTagDto("test " + i, ""));
                tagsOnCharacter.Add(newTag);
                tagsOnOtherCharacter.Add(newTag);
            }

            repository.AddTagsToCharacter(otherCharacterId, tagsOnOtherCharacter);
            repository.AddTagsToCharacter(characterId, tagsOnCharacter);
            repository.UpdateTagsOnCharacter(characterId, new List<long>());

            var retrievedTags = repository.GetTagsOnCharacter(characterId).Select(t => t.TagId);
            var retrievedTagsOnOtherCharacter = repository.GetTagsOnCharacter(otherCharacterId).Select(t => t.TagId);

            CollectionAssert.AreEquivalent(tagsOnOtherCharacter, new List<long>(retrievedTagsOnOtherCharacter));
            CollectionAssert.AreEquivalent(new List<long>(), new List<long>(retrievedTags));
        }

        [TestMethod]
        public void UpsertCharacterTags_ShouldInsertNewTags() {
            var tags = new List<Tag>();

            for (int i = 0; i < 3; ++i) {
                var newTag = new CharacterTag(DatabaseConstants.DEFAULT_ID, "test " + i, "");
                tags.Add(newTag);
            }

            repository.UpsertCharacterTags(tags);
            var retrievedTags = repository.GetCharacterTags();
            foreach (var tag in tags) {
                foreach (var retrievedTag in retrievedTags) {
                    if (tag.Name == retrievedTag.Name) {
                        tag.TagId = retrievedTag.TagId;
                        break;
                    }
                }
            }

            CollectionAssert.AreEquivalent(tags, retrievedTags);
        }

        [TestMethod]
        public void UpsertCharacterTags_ShouldUpdateExistingTags() {
            var tags = new List<Tag>();

            for (int i = 0; i < 3; ++i) {
                var newTag = new CharacterTag(DatabaseConstants.DEFAULT_ID, "test " + i, "");
                tags.Add(newTag);
            }

            repository.UpsertCharacterTags(tags);
            tags[0].Type = "1";
            tags[2].Type = "2";
            tags[0].Name = "new tag 0";
            tags[2].Name = "new tag 2";
            repository.UpsertCharacterTags(tags);

            var retrievedTags = repository.GetCharacterTags();
            foreach (var tag in tags) {
                foreach (var retrievedTag in retrievedTags) {
                    if (tag.Name == retrievedTag.Name) {
                        tag.TagId = retrievedTag.TagId;
                        break;
                    }
                }
            }

            CollectionAssert.AreEquivalent(tags, retrievedTags);
        }

        [TestMethod]
        public void UpsertTagsOnCharacters_ShouldAddItems() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto(tagName, tagType));
            var tag = repository.GetCharacterTag(tagId);

            var charId = GetNewCharacterId();
            var character = characterService.GetCharacter(charId);

            var ids = new Dictionary<string, long>();
            ids[character.UniqueId] = charId;
            var tagsOnCharacters = new List<CharacterTagExportDto>();
            tagsOnCharacters.Add(new CharacterTagExportDto(character.UniqueId, tag.UniqueId));
            repository.UpsertTagsOnCharacters(tagsOnCharacters, ids);

            var tagsList = repository.GetTagsOnCharacter(charId);

            CollectionAssert.AreEquivalent(new List<Tag> { tag }, tagsList);
        }

        [TestMethod]
        public void UpsertTagsOnCharacters_ShouldNotDuplicate() {
            var tagName = "Test name";
            var tagType = "Test tag";
            var tagId = repository.CreateCharacterTag(new CreateCharacterTagDto(tagName, tagType));
            var tag = repository.GetCharacterTag(tagId);

            var charId = GetNewCharacterId();
            var character = characterService.GetCharacter(charId);

            repository.AddTagsToCharacter(charId, new List<long> { tagId });

            var ids = new Dictionary<string, long>();
            ids[character.UniqueId] = charId;
            var tagsOnCharacters = new List<CharacterTagExportDto>();
            tagsOnCharacters.Add(new CharacterTagExportDto(character.UniqueId, tag.UniqueId));
            repository.UpsertTagsOnCharacters(tagsOnCharacters, ids);

            var tagsList = repository.GetTagsOnCharacter(charId);

            CollectionAssert.AreEquivalent(new List<Tag> { tag }, tagsList);
        }
    }
}

using Ingvilt.Dto;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Locations;
using Ingvilt.Dto.Publishers;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Models.DataAccess.Sorting;
using Ingvilt.Repositories;
using Ingvilt.Services;
using IntegrationTesting.Util;
using IntegrationTestingRedo.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class VideoRepositorySearchTests : BaseTest {
        private VideoRepository repository = new VideoRepository();
        private LibraryRepository libraryRepository = new LibraryRepository();
        private CharacterRepository characterRepository = new CharacterRepository();
        private LocationRepository locationRepository = new LocationRepository();

        private Library testLibrary = null;

        private Library CreateAndRetrieveLibrary(CreateLibraryDto libraryDto) {
            long libraryId = libraryRepository.CreateLibrary(libraryDto);
            return new Library(libraryId, libraryDto);
        }

        public VideoRepositorySearchTests() {
            testLibrary = CreateAndRetrieveLibrary(new CreateLibraryDto("test"));
        }

        private Character CreateAndRetrieveCharacter(CreateCharacterDto dto) {
            var characterId = characterRepository.CreateCharacter(dto);
            return characterRepository.GetCharacter(characterId);
        }

        private Video CreateAndRetrieveVideo(CreateVideoDto dto) {
            var id = repository.CreateVideo(dto);
            return repository.GetVideo(id).Result;
        }

        private CreateVideoDto GetNewVideoDetails(Library library, Publisher publisher, Series series) {
            if (library == null) {
                library = testLibrary;
            }

            long publisherId = publisher == null ? -1 : publisher.PublisherId;
            long seriesId = series == null ? -1 : series.SeriesId;

            return CreateVideoUtil.GetNewVideoDetails(library.LibraryId, publisherId, seriesId);
        }

        private CreateVideoDto GetNewVideoDetails(Publisher publisher) {
            return GetNewVideoDetails(null, publisher, null);
        }

        private CreateVideoDto GetNewVideoDetails(Series series) {
            return GetNewVideoDetails(null, null, series);
        }

        private CreateVideoDto GetNewVideoDetails(Library library = null) {
            return GetNewVideoDetails(library, null, null);
        }

        private Video CreateAndRetrieveVideo() {
            return CreateAndRetrieveVideo(GetNewVideoDetails());
        }

        private Pagination GetFirstPage() {
            return Pagination.FirstPageWithDefaultCount(new VideoCreationDateSortCriteria(true));
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldMatchExactName() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator(characterDto.Name));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldMatchEmptyName() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator(""));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldntMatchCharactersNotContainingNameText() {
            var notFoundCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            notFoundCharacterDto.Name = "name that doesn't match";
            var notFoundCharacter = CreateAndRetrieveCharacter(notFoundCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(notFoundCharacter),
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator("test"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldMatchUppercaseName() {
            var uppercaseCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            uppercaseCharacterDto.Name = "TEST abc";
            var uppercaseCharacter = CreateAndRetrieveCharacter(uppercaseCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(uppercaseCharacter),
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator("test"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldMatchLowercaseName() {
            var lowercaseCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            lowercaseCharacterDto.Name = "test abc";
            var lowercase = CreateAndRetrieveCharacter(lowercaseCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(lowercase)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator("Test"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_PartialMatch() {
            var notFoundCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            notFoundCharacterDto.Name = "name";
            var notFoundCharacter = CreateAndRetrieveCharacter(notFoundCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(notFoundCharacter),
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator("name"));
            queries.Add(new CharacterInVideoSearchQueryGenerator("test"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_Multiple() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var notFoundCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            notFoundCharacterDto.Name = "name";
            var notFoundCharacter = CreateAndRetrieveCharacter(notFoundCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(notFoundCharacter),
                new ActorForCharacterFullDto(character),
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator("name"));
            queries.Add(new CharacterInVideoSearchQueryGenerator("test"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterInVideoRequired_ShouldntHaveDuplicateMatches() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterInVideoSearchQueryGenerator(characterDto.Name));
            queries.Add(new CharacterInVideoSearchQueryGenerator(characterDto.Name));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterNotInVideoRequired_ShouldntIncludeWithCharacterMatchingName() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterNotInVideoSearchQueryGenerator(characterDto.Name));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterNotInVideoRequired_ShouldntIncludeWithCharacterMatchingName_AndOneNotMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var differentNameCharacterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            differentNameCharacterDto.Name = "name";
            var differentNameCharacter = CreateAndRetrieveCharacter(differentNameCharacterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character),
                new ActorForCharacterFullDto(differentNameCharacter)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterNotInVideoSearchQueryGenerator(characterDto.Name));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCharacterNotInVideoRequired_WithNoCharactersMatchingName() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharacterNotInVideoSearchQueryGenerator("lebron"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_OneOfCharactersInVideo_WithOneClauseMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new OneOfCharactersInVideoSearchQueryGenerator("[\"T\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_OneOfCharactersInVideo_WithMultipleClausesMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new OneOfCharactersInVideoSearchQueryGenerator("[\"T\", \"E\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_OneOfCharactersInVideo_WithNoClausesMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new OneOfCharactersInVideoSearchQueryGenerator("[\"x\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_NoneOfCharactersInVideo_WithOneClauseMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new NoneOfCharactersInQueryGenerator("[\"T\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_NoneOfCharactersInVideo_WithMultipleClausesMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new NoneOfCharactersInQueryGenerator("[\"T\", \"E\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_NoneOfCharactersInVideo_WithNoClausesMatching() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            characterDto.Name = "Test";
            var character = CreateAndRetrieveCharacter(characterDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new NoneOfCharactersInQueryGenerator("[\"x\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_CharactersWithTagsInVideo_PartialMatch_WithOneTagMatching() {
            var tagRepository = new TagRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1 });

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharactersWithTagsInVideoQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_CharactersWithTagsInVideo_WithAllTagsMatchingOneCharacter() {
            var tagRepository = new TagRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);

            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag2, tag3 });

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharactersWithTagsInVideoQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_CharactersWithTagsInVideo_WithAllTagsMatchingMultipleCharacter() {
            var tagRepository = new TagRepository();
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var character = CreateAndRetrieveCharacter(characterDto);
            var character2 = CreateAndRetrieveCharacter(characterDto);

            var tag1 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 2", ""));
            var tag3 = tagRepository.CreateCharacterTag(new CreateCharacterTagDto("tag 3", ""));

            tagRepository.AddTagsToCharacter(character.CharacterId, new List<long> { tag1, tag3 });
            tagRepository.AddTagsToCharacter(character2.CharacterId, new List<long> { tag2 });

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character),
                new ActorForCharacterFullDto(character2)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CharactersWithTagsInVideoQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithTags_PartialMatch_WithOneTagMatching() {
            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 2", ""));

            var videoId = repository.CreateVideo(GetNewVideoDetails());

            tagRepository.AddTagsToVideo(videoId, new List<long> { tag1 });

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithTags_WithAllTagsMatching() {
            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 2", ""));

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            tagRepository.AddTagsToVideo(videoId, new List<long> { tag1, tag2 });

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithTags_PartialMatch_WithMultipleVideosCombinedHavingAllTags() {
            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 2", ""));

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video2Id = repository.CreateVideo(GetNewVideoDetails());

            tagRepository.AddTagsToVideo(videoId, new List<long> { tag1 });
            tagRepository.AddTagsToVideo(video2Id, new List<long> { tag2 });

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithoutTags_PartialMatch_WithOneTagMatching() {
            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 2", ""));

            var videoId = repository.CreateVideo(GetNewVideoDetails());

            tagRepository.AddTagsToVideo(videoId, new List<long> { tag1 });

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithoutTagsQueryGenerator("[\"tag 1\", \"tag 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithoutTags_WithNoTagsMatching() {
            var tagRepository = new TagRepository();
            var tag1 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 1", ""));
            var tag2 = tagRepository.CreateVideoTag(new CreateVideoTagDto("tag 2", ""));

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            tagRepository.AddTagsToVideo(videoId, new List<long> { tag2 });

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithoutTagsQueryGenerator("[\"tag 1\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoWithoutTags_WithNoTagsExisting() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoWithoutTagsQueryGenerator("[\"tag 1\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTimesWatchedGreaterThan_LessTimes() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTimesWatchedGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTimesWatchedGreaterThan_EqualTimes() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTimesWatchedGreaterThanQueryGenerator("0"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTimesWatchedGreaterThan_Match() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.TimesWatched = 2;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTimesWatchedGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoUserRatingGreaterThan_Less() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.UserRating = 0;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoUserRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoUserRatingGreaterThan_Equal() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.UserRating = 0;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoUserRatingGreaterThanQueryGenerator("0"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoUserRatingGreaterThan_Match() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.UserRating = 2;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoUserRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoExternalRatingGreaterThan_Less() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.ExternalRating = 0;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoExternalRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoExternalRatingGreaterThan_Equal() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.ExternalRating = 0;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoExternalRatingGreaterThanQueryGenerator("0"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoExternalRatingGreaterThan_Match() {
            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            video.ExternalRating = 2;

            repository.UpdateVideo(video).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoExternalRatingGreaterThanQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTitle_MatchLowercase() {
            var dto = GetNewVideoDetails();
            dto.Title = "Test";
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator(dto.Title.ToLower()));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTitle_MatchUppercase() {
            var dto = GetNewVideoDetails();
            dto.Title = "Test";
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator(dto.Title.ToLower()));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTitle_ExactMatch() {
            var dto = GetNewVideoDetails();
            dto.Title = "Test";
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator(dto.Title));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTitle_PartialMatch() {
            var dto = GetNewVideoDetails();
            dto.Title = "Test";
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator("te"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoTitle_NoMatch() {
            var dto = GetNewVideoDetails();
            dto.Title = "Test";
            var videoId = repository.CreateVideo(dto);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoDescription_PartialMatch() {
            var dto = GetNewVideoDetails();
            dto.Description = "Test";
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator("te"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_VideoDescription_NoMatch() {
            var dto = GetNewVideoDetails();
            dto.Description = "Test";
            var videoId = repository.CreateVideo(dto);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoTitleQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_Series_PartialMatch() {
            var seriesRepository = new SeriesRepository();
            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
            var series = seriesRepository.GetSeries(seriesId).Result;

            var dto = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(dto);
            var video = repository.GetVideo(videoId).Result;

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoSeriesQueryGenerator("te"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_Series_NoMatch() {
            var seriesRepository = new SeriesRepository();
            var seriesId = seriesRepository.CreateSeries(new CreateSeriesDto("test", "", -1, "", -1, testLibrary.LibraryId));
            var series = seriesRepository.GetSeries(seriesId).Result;

            var dto = GetNewVideoDetails(series);
            var videoId = repository.CreateVideo(dto);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoSeriesQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCreatorInVideoRequired_PartialMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var creatorDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            creatorDto.IsCreator = true;
            creatorDto.Name = "Test";

            var character = CreateAndRetrieveCharacter(characterDto);
            var creator = CreateAndRetrieveCharacter(creatorDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character, creator.CharacterId, "", creator.Name)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CreatorInVideoQueryGenerator("tes"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>() { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithCreatorInVideoRequired_NoMatch() {
            var characterDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            var creatorDto = CreateCharacterUtil.GetNewCharacterDetails(testLibrary.LibraryId);
            creatorDto.IsCreator = true;
            creatorDto.Name = "Test";

            var character = CreateAndRetrieveCharacter(characterDto);
            var creator = CreateAndRetrieveCharacter(creatorDto);

            var charsList = new List<ActorForCharacterFullDto>() {
                new ActorForCharacterFullDto(character, creator.CharacterId, "", creator.Name)
            };

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            characterRepository.AddCharactersInVideo(videoId, charsList);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new CreatorInVideoQueryGenerator("1"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithLocations_PartialMatch_WithOneLocationMatching() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            locationRepository.AddLocationsToVideo(new List<long> { location1 }, videoId).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoAtLocationsQueryGenerator("[\"Test 1\", \"Test 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithLocations_WithAllLocationsMatching() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            locationRepository.AddLocationsToVideo(new List<long> { location1, location2 }, videoId).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoAtLocationsQueryGenerator("[\"Test 1\", \"Test 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }

        [TestMethod]
        public void SearchForVideos_WithLocations_PartialMatch_WithMultipleVideosCombinedHavingAllLocations() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video2Id = repository.CreateVideo(GetNewVideoDetails());

            locationRepository.AddLocationsToVideo(new List<long> { location1 }, videoId).ConfigureAwait(false);
            locationRepository.AddLocationsToVideo(new List<long> { location2 }, video2Id).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoAtLocationsQueryGenerator("[\"Test 1\", \"Test 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithoutLocations_PartialMatch_WithOneLocationMatching() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            locationRepository.AddLocationsToVideo(new List<long> { location1 }, videoId).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoNotAtLocationsQueryGenerator("[\"Test 1\", \"Test 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithoutLocations_WithAllLocationsMatching() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            locationRepository.AddLocationsToVideo(new List<long> { location1, location2 }, videoId).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoNotAtLocationsQueryGenerator("[\"Test 1\", \"Test 2\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video>(), videos);
        }

        [TestMethod]
        public void SearchForVideos_WithoutLocations_NoMatches() {
            var locationDto = new CreateLocationDto("Test 1", "", testLibrary.LibraryId, -1, -1);
            var location1 = locationRepository.CreateLocation(locationDto);
            locationDto = new CreateLocationDto("Test 2", "", testLibrary.LibraryId, -1, -1);
            var location2 = locationRepository.CreateLocation(locationDto);

            var videoId = repository.CreateVideo(GetNewVideoDetails());
            var video = repository.GetVideo(videoId).Result;

            locationRepository.AddLocationsToVideo(new List<long> { location2 }, videoId).ConfigureAwait(false);

            var queries = new List<IVideoSearchQueryGenerator>();
            queries.Add(new VideoNotAtLocationsQueryGenerator("[\"Test 1\"]"));

            var videos = repository.SearchForVideos(GetFirstPage(), testLibrary.LibraryId, queries).Result.Results;
            CollectionAssert.AreEquivalent(new List<Video> { video }, videos);
        }
    }
}

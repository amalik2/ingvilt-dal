using Ingvilt.Core;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Videos;
using Ingvilt.Models.DataAccess;
using Ingvilt.Models.DataAccess.Search;
using Ingvilt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class CharacterService {
        private CharacterRepository characterRepository;

        public CharacterService() {
            characterRepository = DependencyInjectionContainer.Container.Resolve<CharacterRepository>();
        }

        public async Task<PaginationResult<Character>> GetCharacters(Pagination pagination) {
            return await characterRepository.GetCharacters(pagination, false);
        }

        public async Task<PaginationResult<Character>> GetDeletedCharacters(Pagination pagination) {
            return await characterRepository.GetDeletedCharacters(pagination, false);
        }

        public async Task<PaginationResult<Character>> GetCharactersInLibrary(bool isCreator, Pagination pagination, long libraryId, string nameFilter) {
            return await characterRepository.GetCharactersInLibrary(libraryId, pagination, isCreator, nameFilter);
        }

        public async Task<PaginationResult<Character>> GetDeletedCharactersInLibrary(bool isCreator, Pagination pagination, long libraryId, string nameFilter) {
            return await characterRepository.GetDeletedCharactersInLibrary(libraryId, pagination, isCreator, nameFilter);
        }

        public Character GetCharacter(long characterId) {
            return characterRepository.GetCharacter(characterId);
        }

        public long CreateCharacter(CreateCharacterDto dto) {
            return characterRepository.CreateCharacter(dto);
        }

        public Character CreateAndRetrieveCharacter(CreateCharacterDto dto) {
            long libraryId = CreateCharacter(dto);
            return new Character(libraryId, dto);
        }

        public async Task DeleteCharacter(Character character) {
            characterRepository.DeleteCharacter(character.CharacterId);
        }

        public void UpdateCharacter(Character character) {
            characterRepository.UpdateCharacter(character);
        }

        public async Task RestoreCharacter(Character character) {
            characterRepository.RestoreDeletedCharacter(character.CharacterId);
        }

        public async Task PermanentlyRemoveCharacter(Character character) {
            characterRepository.PermanentlyRemoveCharacter(character.CharacterId);
        }

        public async Task<List<ActorForCharacterFullDto>> GetCharactersInVideo(long videoId) {
            return characterRepository.GetCharactersInVideo(videoId);
        }

        public async Task<List<Character>> GetMostPopularCharactersInVideo(long videoId, int numberOfCharacters) {
            return characterRepository.GetMostPopularCharactersInVideo(videoId, numberOfCharacters);
        }

        public async Task<int> GetNumberOfCharactersInVideo(long videoId) {
            return characterRepository.GetNumbersOfCharactersInVideo(videoId);
        }

        public async Task RemoveCharacterFromVideo(long videoId, long characterId) {
            characterRepository.RemoveCharacterFromVideo(videoId, characterId);
        }

        public async Task UpdateCharactersInVideo(long videoId, List<ActorForCharacterFullDto> characterIds) {
            characterRepository.UpdateCharactersInVideo(videoId, characterIds);
        }

        public async Task<List<CharacterBasicDetails>> GetAllCharactersInLibrary(long libraryId) {
            return characterRepository.GetAllCharactersInLibrary(libraryId);
        }

        public async Task<List<CharacterBasicDetails>> GetAllCreatorsInLibrary(long libraryId) {
            return characterRepository.GetAllCreatorsInLibrary(libraryId);
        }

        public async Task UpdateActorForVideo(CreatorOfVideoFullDto dto, long characterId) {
            characterRepository.UpdateCharacterInVideo(dto, characterId);
        }

        public async Task<PaginationResult<Character>> GetCharactersInSeries(long seriesId, Pagination pagination) {
            return await characterRepository.GetCharactersInSeries(seriesId, pagination);
        }

        public async Task<PaginationResult<Character>> GetCreatorsInSeries(long seriesId, Pagination pagination) {
            return await characterRepository.GetCreatorsInSeries(seriesId, pagination);
        }

        public async Task<PaginationResult<Character>> SearchForCharacters(Pagination pagination, long libraryId, List<ICharacterSearchQueryGenerator> subqueryGenerators) {
            return await characterRepository.SearchForCharacters(pagination, libraryId, subqueryGenerators);
        }

        public async Task AddCharacterToVideos(Character character, List<Video> videos) {
            characterRepository.AddCharacterToVideos(character, videos);
        }

        public async Task<PaginationResult<VideoCreator>> GetVideoCreators(Pagination pagination, long videoId) {
            return await characterRepository.GetVideoCreators(pagination, videoId);
        }

        public async Task UpdateVideoCreatorRole(VideoCreator creator, long videoId) {
            characterRepository.UpdateVideoCreatorRole(creator, videoId);
        }

        public async Task RemoveCreatorFromVideo(VideoCreator creator, long videoId) {
            characterRepository.RemoveCreatorFromVideo(creator, videoId);
        }

        public async Task<PaginationResult<Character>> GetVideoCreatorsNotInVideo(Pagination pagination, long videoId, long libraryId, string nameFilter) {
            return await characterRepository.GetVideoCreatorsNotInVideo(pagination, videoId, libraryId, nameFilter);
        }

        public async Task AddCreatorsToVideo(List<VideoCreator> creators, long videoId) {
            await characterRepository.AddCreatorsToVideo(creators, videoId);
        }

        public async Task<PaginationResult<Character>> GetCharactersWithFile(Pagination pagination, long fileId) {
            return await characterRepository.GetCharactersWithFile(pagination, fileId);
        }
    }
}

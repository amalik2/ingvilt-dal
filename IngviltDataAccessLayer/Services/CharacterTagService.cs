using Ingvilt.Core;
using Ingvilt.Dto.Tags;
using Ingvilt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class CharacterTagService {
        private TagRepository tagRepository;

        public CharacterTagService() {
            tagRepository = DependencyInjectionContainer.Container.Resolve<TagRepository>();
        }

        public List<Tag> GetCharacterTags() {
            return tagRepository.GetCharacterTags();
        }

        public List<Tag> GetDeletedCharacterTags() {
            return tagRepository.GetDeletedCharacterTags();
        }

        public CharacterTag GetCharacterTag(long tagId) {
            return tagRepository.GetCharacterTag(tagId);
        }

        public long CreateCharacterTag(CreateCharacterTagDto dto) {
            return tagRepository.CreateCharacterTag(dto);
        }

        public CharacterTag CreateAndRetrieveCharacterTag(CreateCharacterTagDto dto) {
            long tagId = CreateCharacterTag(dto);
            return new CharacterTag(tagId, dto);
        }

        public async Task DeleteCharacterTag(CharacterTag tag) {
            tagRepository.DeleteCharacterTag(tag);
        }

        public void UpdateCharacterTag(CharacterTag tag) {
            tagRepository.UpdateTag(tag);
        }

        public async Task RestoreCharacterTag(CharacterTag tag) {
            tagRepository.RestoreCharacterTag(tag);
        }

        public async Task PermanentlyRemoveCharacterTag(CharacterTag tag) {
            tagRepository.PermanentlyRemoveCharacterTag(tag);
        }

        public async Task<List<string>> LoadAllCharacterTagNames() {
            return tagRepository.LoadAllCharacterTagNames();
        }

        public async Task<List<Tag>> GetTagsOnCharacter(long characterId) {
            return tagRepository.GetTagsOnCharacter(characterId);
        }

        public void RemoveTagFromCharacter(long characterId, long tagId) {
            tagRepository.RemoveTagFromCharacter(characterId, tagId);
        }

        public void AddTagsToCharacter(long characterId, List<long> tagIds) {
            tagRepository.AddTagsToCharacter(characterId, tagIds);
        }
    }
}

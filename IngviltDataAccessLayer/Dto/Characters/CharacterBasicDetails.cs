namespace Ingvilt.Dto.Characters {
    public class CharacterBasicDetails {
        public long CharacterId {
            get;
        }

        public string Name {
            get;
        }

        public CharacterBasicDetails(long characterId, string name) {
            CharacterId = characterId;
            Name = name;
        }
    }
}

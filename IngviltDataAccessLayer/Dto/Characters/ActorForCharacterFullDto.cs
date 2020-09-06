using Ingvilt.Constants;
using Ingvilt.Models;

namespace Ingvilt.Dto.Characters {
    public class ActorForCharacterFullDto : BaseObservableProperty {
        private string actorName;
        private long actorId;

        public Character Character {
            get;
            set;
        }

        public long ActorId {
            get {
                return actorId;
            }
            set {
                actorId = value;
                OnPropertyChanged(nameof(ActorId));
            }
        }

        public string ActorRole {
            get;
            set;
        }

        public string ActorName {
            get {
                return actorName;
            }
            set {
                actorName = value;
                OnPropertyChanged(nameof(ActorName));
            }
        }

        public ActorForCharacterFullDto(Character character, long actorId, string actorRole, string actorName) {
            Character = character;
            ActorId = actorId;
            ActorRole = actorRole;
            ActorName = actorName;
        }

        public ActorForCharacterFullDto(Character character) {
            Character = character;
            Clear();
        }

        public override bool Equals(object other) {
            var otherCharacter = other as ActorForCharacterFullDto;
            if (otherCharacter == null) {
                return false;
            }

            return otherCharacter.Character.CharacterId == Character.CharacterId;
        }

        public override int GetHashCode() {
            return ((int)Character.CharacterId << 2) ^ Character.Name.GetHashCode();
        }

        public void Clear() {
            ActorId = DatabaseConstants.DEFAULT_ID;
            ActorRole = "";
            ActorName = "";
        }
    }
}

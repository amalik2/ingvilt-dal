using Ingvilt.Models;

namespace Ingvilt.Dto.Characters {
    public class VideoCreator : BaseObservableProperty {
        public Character CharacterDetails {
            get;
            set;
        }

        public string Role {
            get;
            set;
        }

        public VideoCreator(Character character, string role) {
            CharacterDetails = character;
            Role = role;
        }

        public void NotifyRoleChanged() {
            OnPropertyChanged(nameof(Role));
        }
    }
}

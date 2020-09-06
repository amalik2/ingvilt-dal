using Newtonsoft.Json;

namespace Ingvilt.Dto.Export {
    public class VideoCharacterActorExportDto {
        public string VideoId;
        public string CharacterId;
        public string ActorId;

        [JsonIgnore]
        public long ActorIdLong;

        public VideoCharacterActorExportDto(string videoId, string characterId, long actorId) {
            VideoId = videoId;
            CharacterId = characterId;
            ActorIdLong = actorId;
        }

        [JsonConstructor]
        public VideoCharacterActorExportDto() {
        }
    }
}

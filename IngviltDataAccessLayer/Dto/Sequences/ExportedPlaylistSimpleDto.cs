namespace Ingvilt.Dto.Sequences {
    public class ExportedPlaylistSimpleDto {
        public PlaylistDto Details;
        public string FileId;

        public ExportedPlaylistSimpleDto(PlaylistDto details, string fileId) {
            Details = details;
            FileId = fileId;
        }
    }
}

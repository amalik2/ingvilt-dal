namespace Ingvilt.Dto.Export {
    public class SeriesTagExportDto {
        public string SeriesId;
        public string TagId;

        public SeriesTagExportDto(string seriesId, string tagId) {
            SeriesId = seriesId;
            TagId = tagId;
        }
    }
}

namespace Ingvilt.Dto.Sequences {
    public class ExportedSeriesSequenceSimpleDto {
        public SeriesSequence Details;
        public string FileId;
        public string SeriesId;

        public ExportedSeriesSequenceSimpleDto(SeriesSequence details, string fileId, string seriesId) {
            Details = details;
            FileId = fileId;
            SeriesId = seriesId;
        }
    }
}

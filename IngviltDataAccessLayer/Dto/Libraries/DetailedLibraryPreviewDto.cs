namespace Ingvilt.Dto.Libraries {
    public class DetailedLibraryPreviewDto {
        public int VideosCount {
            get;
        }

        public int CharactersCount {
            get;
        }

        public int CreatorsCount {
            get;
        }

        public int SeriesCount {
            get;
        }

        public int PublishersCount {
            get;
        }

        public DetailedLibraryPreviewDto(int videosCount, int charactersCount, int creatorsCount, int seriesCount, int publishersCount) {
            VideosCount = videosCount;
            CharactersCount = charactersCount;
            CreatorsCount = creatorsCount;
            SeriesCount = seriesCount;
            PublishersCount = publishersCount;
        }
    }
}

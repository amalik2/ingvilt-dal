using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingvilt.Dto.Libraries {
    public class DetailedLibraryDto {
        public int VideosCount {
            get;
        }

        public int WatchedVideosCount {
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

        public int FinishedSeriesCount {
            get;
        }

        public int PublishersCount {
            get;
        }

        public int CalendarsCount {
            get;
        }

        public int LocationsCount {
            get;
        }

        public int PlaylistsCount {
            get;
        }

        public DetailedLibraryDto(int videosCount, int watchedVideosCount, int charactersCount, int creatorsCount, int seriesCount, int finishedSeriesCount, int publishersCount, int calendarsCount, int locationsCount, int playlistsCount) {
            VideosCount = videosCount;
            WatchedVideosCount = watchedVideosCount;
            CharactersCount = charactersCount;
            CreatorsCount = creatorsCount;
            SeriesCount = seriesCount;
            FinishedSeriesCount = finishedSeriesCount;
            PublishersCount = publishersCount;
            CalendarsCount = calendarsCount;
            LocationsCount = locationsCount;
            PlaylistsCount = playlistsCount;
        }
    }
}

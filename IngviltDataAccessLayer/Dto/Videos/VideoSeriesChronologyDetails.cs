namespace Ingvilt.Dto.Videos {
    public class VideoSeriesChronologyDetails {
        public int SeasonNumber {
            get;
            set;
        }

        public int EpisodeNumber {
            get;
            set;
        }

        public VideoSeriesChronologyDetails(int seasonNumber, int episodeNumber) {
            SeasonNumber = seasonNumber;
            EpisodeNumber = episodeNumber;
        }

        public override string ToString() {
            return $"Season {SeasonNumber}, Episode {EpisodeNumber}";
        }
    }
}

using System.Collections.Generic;

namespace Ingvilt.Constants {
    public class GlobalSettingsConstants {
        public static readonly int MAX_NAME_LENGTH = 40;
        public static readonly int MAX_SETTINGS_VALUE_LENGTH = 80;

        public static readonly string VIDEO_PREVIEW_TYPE = "VideoPreview";
        public static readonly string CHARACTER_PREVIEW_TYPE = "CharacterPreview";
        public static readonly string LIBRARY_PREVIEW_TYPE = "LibraryPreview";
        public static readonly string LOCATION_PREVIEW_TYPE = "LocationPreview";
        public static readonly string PLAYLIST_PREVIEW_TYPE = "PlaylistPreview";
        public static readonly string PUBLISHER_PREVIEW_TYPE = "PublisherPreview";
        public static readonly string SERIES_PREVIEW_TYPE = "SeriesPreview";

        public static readonly List<string> ALL_PREVIEW_TYPE_NAMES = new List<string>() {
            VIDEO_PREVIEW_TYPE, CHARACTER_PREVIEW_TYPE, LIBRARY_PREVIEW_TYPE, LOCATION_PREVIEW_TYPE,
            PLAYLIST_PREVIEW_TYPE, PUBLISHER_PREVIEW_TYPE, SERIES_PREVIEW_TYPE
        };
    }
}

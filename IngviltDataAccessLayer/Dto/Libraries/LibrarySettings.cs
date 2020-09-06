using Ingvilt.Util;
using System.Collections.Generic;

namespace Ingvilt.Dto {
    public partial class LibrarySettings {
        public static readonly string DEFAULT_PUBLISHER_LABEL = "Publisher";
        public static readonly string DEFAULT_SERIES_LABEL = "Series";
        public static readonly string DEFAULT_VIDEO_LABEL = "Video";
        public static readonly string DEFAULT_CHARACTER_LABEL = "Character";

        public static readonly string DEFAULT_VIDEO_PREVIEW_DATE_FORMAT = "yyyy";

        public static readonly Dictionary<string, string> DEFAULT_SETTINGS = new Dictionary<string, string>();
        
        public string PublisherLabel {
            get; set;
        }

        public string SeriesLabel {
            get; set;
        }

        public string VideoLabel {
            get; set;
        }

        public string CharacterLabel {
            get; set;
        }

        public string VideoPreviewDateFormat {
            get;
            set;
        }

        static LibrarySettings() {
            DEFAULT_SETTINGS.Add(nameof(PublisherLabel), DEFAULT_PUBLISHER_LABEL);
            DEFAULT_SETTINGS.Add(nameof(SeriesLabel), DEFAULT_SERIES_LABEL);
            DEFAULT_SETTINGS.Add(nameof(VideoLabel), DEFAULT_VIDEO_LABEL);
            DEFAULT_SETTINGS.Add(nameof(CharacterLabel), DEFAULT_CHARACTER_LABEL);
            DEFAULT_SETTINGS.Add(nameof(VideoPreviewDateFormat), DEFAULT_VIDEO_PREVIEW_DATE_FORMAT);
        }

        public LibrarySettings(Dictionary<string, string> settings) {
            PublisherLabel = settings[nameof(PublisherLabel)];
            SeriesLabel = settings[nameof(SeriesLabel)];
            VideoLabel = settings[nameof(VideoLabel)];
            CharacterLabel = settings[nameof(CharacterLabel)];
            VideoPreviewDateFormat = settings[nameof(VideoPreviewDateFormat)];
        }

        public LibrarySettings() : this(DEFAULT_SETTINGS) {
        }

        public LibrarySettings Clone() {
            var settings = new LibrarySettings();
            settings.Copy(this);
            return settings;
        }

        public void Copy(LibrarySettings settings) {
            PublisherLabel = settings.PublisherLabel;
            SeriesLabel = settings.SeriesLabel;
            VideoLabel = settings.VideoLabel;
            CharacterLabel = settings.CharacterLabel;
            VideoPreviewDateFormat = settings.VideoPreviewDateFormat;
        }

        public string ReplacePublisherText(string text) {
            return StringUtil.GetReplacedText(text, "Publisher", PublisherLabel);
        }

        public string ReplaceSeriesText(string text) {
            return StringUtil.GetReplacedText(text, "Series", SeriesLabel);
        }

        public string ReplaceVideoText(string text) {
            return StringUtil.GetReplacedText(text, "Video", VideoLabel);
        }

        public string ReplaceCharacterText(string text) {
            return StringUtil.GetReplacedText(text, "Character", CharacterLabel);
        }
    }
}

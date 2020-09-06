using Ingvilt.Constants;
using Ingvilt.Util;
using System;
using System.Collections.Generic;

namespace Ingvilt.Dto {
    public sealed class VideoLoadEvent {
        public int Ordinal {
            get;
        }

        public static readonly VideoLoadEvent LOAD_AUTOMATICALLY = new VideoLoadEvent(0);
        public static readonly VideoLoadEvent LOAD_ON_HOVER = new VideoLoadEvent(1);
        public static readonly VideoLoadEvent LOAD_ON_CLICK = new VideoLoadEvent(2);

        public VideoLoadEvent(int ordinal) {
            Ordinal = ordinal;
        }

        public override string ToString() {
            return Ordinal.ToString();
        }

        public static VideoLoadEvent Parse(string value) {
            var intValue = int.Parse(value);
            if (intValue == 0) {
                return LOAD_AUTOMATICALLY;
            } else if (intValue == 1) {
                return LOAD_ON_HOVER;
            } else if (intValue == 2) {
                return LOAD_ON_CLICK;
            }

            throw new ArgumentOutOfRangeException("An invalid ordinal was specified");
        }

        public override bool Equals(object obj) {
            var otherType = obj as VideoLoadEvent;
            return Ordinal == otherType.Ordinal;
        }

        public override int GetHashCode() {
            return Ordinal;
        }
    }

    public sealed class EntityPreviewType {
        public int Ordinal {
            get;
        }

        public static readonly EntityPreviewType SIMPLE_VIEW = new EntityPreviewType(0);
        public static readonly EntityPreviewType DETAILED_VIEW = new EntityPreviewType(1);

        public EntityPreviewType(int ordinal) {
            Ordinal = ordinal;
        }

        public override string ToString() {
            return Ordinal.ToString();
        }

        public static EntityPreviewType Parse(string value) {
            var intValue = int.Parse(value);
            if (intValue == 0) {
                return SIMPLE_VIEW;
            } else if (intValue == 1) {
                return DETAILED_VIEW;
            }

            throw new ArgumentOutOfRangeException("An invalid ordinal was specified");
        }

        public override bool Equals(object obj) {
            var otherType = obj as EntityPreviewType;
            return Ordinal == otherType.Ordinal;
        }

        public override int GetHashCode() {
            return Ordinal;
        }
    }

    public class GlobalSettings {
        private static readonly EntityPreviewType DEFAULT_ENTITY_PREVIEW_TYPE = EntityPreviewType.DETAILED_VIEW;

        public static readonly bool DEFAULT_TRACK_TIMES_WATCHED = true;
        public static readonly bool DEFAULT_TRACK_LAST_WATCH_DATE = true;
        public static readonly bool DEFAULT_COLLAPSE_TAG_CATEGORIES = true;

        public static readonly VideoLoadEvent DEFAULT_VIDEO_LOAD_TYPE = VideoLoadEvent.LOAD_ON_HOVER;

        public static readonly Dictionary<string, string> DEFAULT_SETTINGS = new Dictionary<string, string>();

        public static readonly double DEFAULT_VIDEO_VOLUME = 50;

        public bool TrackTimesWatched {
            get; set;
        }

        public bool TrackLastWatchDate {
            get; set;
        }

        public VideoLoadEvent VideoLoadType {
            get;
            set;
        }

        public bool CollapseTagCategories {
            get;
            set;
        }

        public Dictionary<string, EntityPreviewType> PreviewTypes {
            get;
            set;
        }

        public double VideoVolume {
            get;
            set;
        }

        static GlobalSettings() {
            DEFAULT_SETTINGS.Add(nameof(TrackTimesWatched), DEFAULT_TRACK_TIMES_WATCHED.ToString());
            DEFAULT_SETTINGS.Add(nameof(TrackLastWatchDate), DEFAULT_TRACK_LAST_WATCH_DATE.ToString());
            DEFAULT_SETTINGS.Add(nameof(VideoLoadType), DEFAULT_VIDEO_LOAD_TYPE.ToString());
            DEFAULT_SETTINGS.Add(nameof(CollapseTagCategories), DEFAULT_COLLAPSE_TAG_CATEGORIES.ToString());

            foreach (var settingName in GlobalSettingsConstants.ALL_PREVIEW_TYPE_NAMES) {
                if (DEFAULT_SETTINGS.ContainsKey(settingName)) {
                    throw new InvalidOperationException($"{settingName} already exists as a setting");
                }

                DEFAULT_SETTINGS.Add(settingName, DEFAULT_ENTITY_PREVIEW_TYPE.Ordinal.ToString());
            }

            DEFAULT_SETTINGS.Add(nameof(VideoVolume), DEFAULT_VIDEO_VOLUME.ToString());
        }

        public GlobalSettings(Dictionary<string, string> settings) {
            TrackTimesWatched = Boolean.Parse(settings[nameof(TrackTimesWatched)]);
            TrackLastWatchDate = Boolean.Parse(settings[nameof(TrackLastWatchDate)]);
            VideoLoadType = VideoLoadEvent.Parse(settings[nameof(VideoLoadType)]);
            CollapseTagCategories = Boolean.Parse(settings[nameof(CollapseTagCategories)]);

            PreviewTypes = new Dictionary<string, EntityPreviewType>();
            foreach (var settingName in GlobalSettingsConstants.ALL_PREVIEW_TYPE_NAMES) {
                PreviewTypes.Add(settingName, EntityPreviewType.Parse(settings[settingName]));
            }

            VideoVolume = Double.Parse(settings[nameof(VideoVolume)]);
        }

        public GlobalSettings() : this(DEFAULT_SETTINGS) {
        }

        public override bool Equals(object obj) {
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            }

            var otherSettings = obj as GlobalSettings;
            return (TrackTimesWatched == otherSettings.TrackTimesWatched) &&
                (TrackLastWatchDate == otherSettings.TrackLastWatchDate) &&
                (VideoLoadType == otherSettings.VideoLoadType) &&
                (CollapseTagCategories == otherSettings.CollapseTagCategories) &&
                ObjectUtil.AreDictionariesEqual(PreviewTypes, otherSettings.PreviewTypes) &&
                (VideoVolume == otherSettings.VideoVolume);
        }

        public override int GetHashCode() {
            var lastWatchDateOrdinal = TrackLastWatchDate ? 1 : 0;
            var trackTimesWatchedOrdinal = TrackTimesWatched ? 1 : 0;

            return (lastWatchDateOrdinal << 2) ^ trackTimesWatchedOrdinal ^ VideoLoadType.Ordinal;
        }

        public bool ShouldLoadVideosAutomatically() {
            return VideoLoadType == VideoLoadEvent.LOAD_AUTOMATICALLY;
        }

        public bool ShouldLoadVideosOnHover() {
            return VideoLoadType == VideoLoadEvent.LOAD_ON_HOVER;
        }

        public bool ShouldLoadVideosOnClick() {
            return VideoLoadType == VideoLoadEvent.LOAD_ON_CLICK;
        }

        public void SetLoadVideosAutomatically() {
            VideoLoadType = VideoLoadEvent.LOAD_AUTOMATICALLY;
        }

        public void SetLoadVideosOnHover() {
            VideoLoadType = VideoLoadEvent.LOAD_ON_HOVER;
        }

        public void SetLoadVideosOnClick() {
            VideoLoadType = VideoLoadEvent.LOAD_ON_CLICK;
        }

        public GlobalSettings Clone() {
            var clone = new GlobalSettings();
            clone.Copy(this);
            return clone;
        }

        public void Copy(GlobalSettings settings) {
            TrackLastWatchDate = settings.TrackLastWatchDate;
            TrackTimesWatched = settings.TrackTimesWatched;
            VideoLoadType = settings.VideoLoadType;
            CollapseTagCategories = settings.CollapseTagCategories;

            PreviewTypes = settings.PreviewTypes;
            VideoVolume = settings.VideoVolume;
        }
    }
}

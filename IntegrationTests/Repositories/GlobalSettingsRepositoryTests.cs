using Ingvilt.Dto;
using Ingvilt.Repositories;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTesting.Repositories {
    [TestClass]
    public class GlobalSettingsRepositoryTests : BaseTest {
        private GlobalSettingsRepository repository = new GlobalSettingsRepository();

        [TestMethod]
        public void TestCreateInitialSettings() {
            repository.CreateDefaultSettings();
            var settings = repository.LoadSettings();

            Assert.AreEqual(GlobalSettings.DEFAULT_TRACK_TIMES_WATCHED, settings.TrackTimesWatched);
            Assert.AreEqual(GlobalSettings.DEFAULT_TRACK_LAST_WATCH_DATE, settings.TrackLastWatchDate);
        }

        [TestMethod]
        public void CreateInitialSettings_ShouldNotReplaceExistingValues() {
            repository.CreateDefaultSettings();
            var settings = repository.LoadSettings();
            settings.TrackLastWatchDate = !settings.TrackLastWatchDate;
            settings.TrackTimesWatched = !settings.TrackTimesWatched;
            settings.VideoLoadType = VideoLoadEvent.LOAD_AUTOMATICALLY;
            settings.CollapseTagCategories = !settings.CollapseTagCategories;

            repository.UpdateSettings(settings);

            repository.CreateDefaultSettings();
            var refetchedSettings = repository.LoadSettings();

            Assert.AreEqual(settings, refetchedSettings);
        }

        [TestMethod]
        public void TestUpdateSettings() {
            repository.CreateDefaultSettings();
            var settings = repository.LoadSettings();
            settings.TrackLastWatchDate = !settings.TrackLastWatchDate;
            settings.TrackTimesWatched = !settings.TrackTimesWatched;
            settings.VideoLoadType = VideoLoadEvent.LOAD_AUTOMATICALLY;

            repository.UpdateSettings(settings);
            var refetchedSettings = repository.LoadSettings();

            Assert.AreEqual(settings, refetchedSettings);
        }
    }
}

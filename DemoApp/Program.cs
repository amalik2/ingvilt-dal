using Ingvilt.Util;

using System;
using System.IO;

namespace DemoApp {
    public class Program {
        private static readonly string UWP_PACKAGE_ID = "14c1a1df-eb0a-4aea-abfd-e3cea89227dd_xn9tfjes7hkzg";

        private static string GetDefaultDatabasePath() {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var relativePath = $@"Packages\{UWP_PACKAGE_ID}\LocalState\data\data.db";
            return Path.Combine(appDataPath, relativePath);
        }

        private static void InitDatabase() {
            // set this path to your Ingvilt install directory
            DataAccessUtil.DatabasePath = GetDefaultDatabasePath();
            DataAccessUtil.InitDatabase();
        }

        public static void Main(string[] args) {
            InitDatabase();

            // now, you can run any DAL code that you want to execute

            // Example:
            // var libraryRepository = new LibraryRepository();
            // var libraryId = libraryRepository.CreateLibrary(new CreateLibraryDto("TV Shows"));
            // var library = libraryRepository.GetLibrary(libraryId);
        }
    }
}

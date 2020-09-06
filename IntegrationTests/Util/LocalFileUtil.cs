using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ingvilt.Util {
    class LocalFileUtil {
        public static string GetAbsolutePath(string relativePath) {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, relativePath);
        }
    }
}

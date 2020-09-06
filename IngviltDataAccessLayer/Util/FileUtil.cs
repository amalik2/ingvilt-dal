using System.IO;

namespace Ingvilt.Util {
    public class CommonFileUtil {
        public static void EnsureFileExists(string path) {
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var file = File.Create(path);
                file.Close();
            }
        }
    }
}

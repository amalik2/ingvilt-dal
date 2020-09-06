using System;

namespace Ingvilt.Util {
    public class UniqueIdUtil {
        public static string GenerateUniqueId() {
            return Guid.NewGuid().ToString();
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Ingvilt.Util {
    public class ObjectUtil {
        public static bool AreDictionariesEqual<U, V>(IDictionary<U, V> first, IDictionary<U, V> second) {
            return first.Intersect(second).Count() == first.Union(second).Count();
        }
    }
}

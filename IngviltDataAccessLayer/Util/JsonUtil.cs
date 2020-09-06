using Newtonsoft.Json;

namespace Ingvilt.Util {
    public class JsonUtil {

        public static string SerializeAsString<T>(T objectToWrite, Formatting formatting = Formatting.Indented) {
            return JsonConvert.SerializeObject(objectToWrite, formatting, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static T DeserializeFromString<T>(string serializedData) {
            return JsonConvert.DeserializeObject<T>(serializedData, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}

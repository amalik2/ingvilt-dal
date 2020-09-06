using System;
using System.Collections.Generic;

using Ingvilt.Util;
using Newtonsoft.Json;

namespace Ingvilt.Models.DataAccess.Search {
    public class QueryGeneratorWithArrayValue {
        protected List<string> value;

        public QueryGeneratorWithArrayValue(string value) {
            this.value = JsonUtil.DeserializeFromString<List<string>>(value);
            if (this.value.Count == 0) {
                throw new ArgumentException("There are no values");
            }
        }

        public string GetValueAsString() {
            return JsonUtil.SerializeAsString(value, Formatting.None).Replace("'", "\\'");
        }

        public List<string> GetValue() {
            return value;
        }
    }
}

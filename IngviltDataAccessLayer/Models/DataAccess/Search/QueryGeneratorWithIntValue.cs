using System;

namespace Ingvilt.Models.DataAccess.Search {
    public class QueryGeneratorWithIntValue {
        protected int value;

        public QueryGeneratorWithIntValue(string value) {
            this.value = Int32.Parse(value);
        }

        public string GetValueAsString() {
            return value.ToString();
        }
    }
}

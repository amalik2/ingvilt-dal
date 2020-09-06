using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ingvilt.Core {
    public class LoggingService {

        public virtual void Info(string text) {
            Debug.WriteLine(text);
        }

        public virtual void Error(string text) {
            Debug.WriteLine(text);
        }

        public virtual void Error(Exception exception) {
            Debug.WriteLine(exception);
        }

        public virtual async Task Flush() { }
    }
}

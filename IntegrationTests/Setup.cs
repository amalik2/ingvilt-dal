using Ingvilt.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTesting {
    [TestClass]
    public class Setup {
        [AssemblyInitialize]
        public static void Init(TestContext testContext) {
            DataAccessUtil.DatabasePath = ":memory:";
            DataAccessUtil.UsePersistentConnection = true;
            DataAccessUtil.InitDatabase();
        }
    }
}

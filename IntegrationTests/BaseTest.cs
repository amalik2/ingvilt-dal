using Ingvilt.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTesting {
    [TestClass]
    public class BaseTest {
        [TestCleanup]
        public void AfterTestFinished() {
            DataAccessUtil.ClearDatabase();
        }
    }
}

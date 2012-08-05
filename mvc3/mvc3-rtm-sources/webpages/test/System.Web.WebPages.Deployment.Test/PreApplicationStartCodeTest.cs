using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Deployment.Test {
    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void PreApplicationStartCodeDoesNothingIfVersionDoesNotMatchBootstrapperVersion() {
            // Act
            bool loaded = PreApplicationStartCode.StartCore(testVersion: new Version(2, 0, 0, 0));

            // Assert
            Assert.IsFalse(loaded);
        }

        [TestMethod]
        public void PreApplicationStartCodeLoadsWebPagesIfVersionMatchesBootstrapperVersion() {
            // Act
            bool loaded = PreApplicationStartCode.StartCore(testVersion: new Version(1, 0, 0, 0));

            // Assert
            Assert.IsTrue(loaded);
        }

        [TestMethod]
        public void TestPreAppStartClass() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}

using System.Linq;
using System.Web.WebPages.Razor;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Web.Helpers.Test {
    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void StartTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                // Act
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();

                // Assert
                var imports = WebPageRazorHost.GetGlobalImports();
                Assert.IsTrue(imports.Any(ns => ns.Equals("Microsoft.Web.Helpers")));
            });
        }

        [TestMethod]
        public void TestPreAppStartClass() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(Microsoft.Web.Helpers.PreApplicationStartCode));
        }
    }
}



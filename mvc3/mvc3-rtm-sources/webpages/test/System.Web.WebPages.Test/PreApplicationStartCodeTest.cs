using System.Reflection;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void StartTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();
                // Call a second time to ensure multiple calls do not cause issues
                PreApplicationStartCode.Start();

                Assert.IsFalse(RouteTable.Routes.RouteExistingFiles, "We should not be setting RouteExistingFiles");
                Assert.AreEqual(0, RouteTable.Routes.Count, "We should not be adding any routes");

                Assert.IsFalse(PageParser.EnableLongStringsAsResources);

                string formsAuthLoginUrl = (string)typeof(FormsAuthentication).GetField("_LoginUrl", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.IsNull(formsAuthLoginUrl, "Form Auth should not be set up by this assembly's PreAppStart - it should happen in WebMatrix.WebData.dll");
            });
        }

        [TestMethod]
        public void TestPreAppStartClass() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}

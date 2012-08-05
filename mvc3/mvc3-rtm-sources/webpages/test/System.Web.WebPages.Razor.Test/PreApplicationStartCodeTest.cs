using System.Reflection;
using System.Web.Compilation;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Razor.Test {
    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void StartTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();
                var buildProviders = typeof(BuildProvider).GetField("s_dynamicallyRegisteredProviders", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.AreEqual(2, buildProviders.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance).GetValue(buildProviders, new object[] { }));
            });
        }

        [TestMethod]
        public void TestPreAppStartClass() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}

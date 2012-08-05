using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.TestUtils {
    public static class TestContextAssert {
        public static void IsNotNull(TestContext context) {
            Assert.IsNotNull(context, "TestContext should never be null!  To workaround this bug in MSTest, " +
                "add tools\\Patches\\Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter.dll to your GAC.");
        }
    }
}

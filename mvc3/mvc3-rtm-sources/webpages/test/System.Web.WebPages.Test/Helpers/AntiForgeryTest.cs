namespace System.Web.Helpers.Test {
    using System.Web.WebPages.TestUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AntiForgeryTest {
        private static string _antiForgeryTokenCookieName = AntiForgeryData.GetAntiForgeryTokenName("/SomeAppPath");

        [TestMethod]
        public void GetHtml_ThrowsWhenNotCalledInWebContext() {
            ExceptionAssert.ThrowsArgumentException(() => AntiForgery.GetHtml(),
                "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [TestMethod]
        public void GetHtml_ThrowsOnNullContext() {
            ExceptionAssert.ThrowsArgNull(() => AntiForgery.GetHtml(null, null, null, null), "httpContext");
        }

        [TestMethod]
        public void Validate_ThrowsWhenNotCalledInWebContext() {
            ExceptionAssert.ThrowsArgumentException(() => AntiForgery.Validate(),
                "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [TestMethod]
        public void Validate_ThrowsOnNullContext() {
            ExceptionAssert.ThrowsArgNull(() => AntiForgery.Validate(httpContext: null, salt: null), "httpContext");
        }
    }
}

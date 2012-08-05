using System;
using System.Web;
using System.Web.WebPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.WebPages.Test.Helpers {

    [TestClass]
    public class HttpContextExtensionsTest {
        class RedirectData {
            public string RequestUrl { get; set; }
            public string RedirectUrl { get; set; }
        }

        private static HttpContextBase GetContextForRedirectLocal(RedirectData data ) {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.Url).Returns(new Uri(data.RequestUrl));
            contextMock.Setup(context => context.Response.Redirect(It.IsAny<string>())).Callback((string url) => data.RedirectUrl = url);
            return contextMock.Object;
        }

        [TestMethod]
        public void RedirectLocalWithNullGoesToRootTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.AreEqual("~/", data.RedirectUrl);
        }

        [TestMethod]
        public void RedirectLocalWithEmptyStringGoesToRootTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.AreEqual("~/", data.RedirectUrl);
        }

        [TestMethod]
        public void RedirectLocalWithNonLocalGoesToRootTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.AreEqual("~/", data.RedirectUrl);
        }

        [TestMethod]
        public void RedirectLocalWithDifferentHostGoesToRootTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("http://bar");
            Assert.AreEqual("~/", data.RedirectUrl);
        }

        [TestMethod]
        public void RedirectLocalOnSameHostTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("http://foo/bar/baz");
            Assert.AreEqual("http://foo/bar/baz", data.RedirectUrl);
            context.RedirectLocal("http://foo/bar/baz/woot.htm");
            Assert.AreEqual("http://foo/bar/baz/woot.htm", data.RedirectUrl);
        }

        [TestMethod]
        public void RedirectLocalRelativeTest() {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("bar");
            Assert.AreEqual("bar", data.RedirectUrl);
            context.RedirectLocal("bar/hey.you");
            Assert.AreEqual("bar/hey.you", data.RedirectUrl);
        }
    }
}

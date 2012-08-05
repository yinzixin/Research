namespace Microsoft.WebPages.Test.Helpers {
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages;
    using Moq;
    using System;
    using System.Web.WebPages.TestUtils;

    [TestClass]
    public class HttpRequestExtensionsTest {
        private static HttpRequestBase GetRequestForIsUrlLocalToHost(string url) {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.Url).Returns(new Uri(url));
            return contextMock.Object.Request;
        }

        [TestMethod]
        public void IsUrlLocalToHost_ReturnsFalseOnNull() {
            var request = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(request.IsUrlLocalToHost(null));
        }

        [TestMethod]
        public void IsUrlLocalToHost_AcceptsRelativeUrls() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsTrue(helper.IsUrlLocalToHost("foobar.html"));
            Assert.IsTrue(helper.IsUrlLocalToHost("../foobar.html"));
            Assert.IsTrue(helper.IsUrlLocalToHost("fold/foobar.html"));
            Assert.IsTrue(helper.IsUrlLocalToHost("/"));
            Assert.IsTrue(helper.IsUrlLocalToHost("/www.hackerz.com"));
            Assert.IsTrue(helper.IsUrlLocalToHost("~/foobar.html"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectValidButUnsafeRelativeUrls() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost("http:/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("hTtP:foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("http:/www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("HtTpS:/www.hackerz.com"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_AcceptsUrlsOnTheSameHost() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsTrue(helper.IsUrlLocalToHost("http://www.mysite.com/appDir/foobar.html"));
            Assert.IsTrue(helper.IsUrlLocalToHost("http://WWW.MYSITE.COM"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectsUrlsOnLocalHost() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost("http://localhost/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("http://127.0.0.1/foobar.html"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_AcceptsUrlsOnTheSameHostButDifferentScheme() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsTrue(helper.IsUrlLocalToHost("https://www.mysite.com/"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectsUrlsOnDifferentHost() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost("http://www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("https://www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("hTtP://www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("HtTpS://www.hackerz.com"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectsUrlsWithTooManySchemeSeparatorCharacters() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost("http://///www.hackerz.com/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("https://///www.hackerz.com/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("HtTpS://///www.hackerz.com/foobar.html"));

            Assert.IsFalse(helper.IsUrlLocalToHost("http:///www.hackerz.com/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("http:////www.hackerz.com/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("http://///www.hackerz.com/foobar.html"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectsUrlsWithMissingSchemeName() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost("//www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("//www.hackerz.com/foobar.html"));
            Assert.IsFalse(helper.IsUrlLocalToHost("///www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost("//////www.hackerz.com"));
        }

        [TestMethod]
        public void IsUrlLocalToHost_RejectsInvalidUrls() {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.IsFalse(helper.IsUrlLocalToHost(@"http:\\www.hackerz.com"));
            Assert.IsFalse(helper.IsUrlLocalToHost(@"http:\\www.hackerz.com\"));
        }
    }
}

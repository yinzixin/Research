using System.Collections;
using System.IO;
using System.Security;
using System.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class WebPageHttpHandlerTest {
        [TestMethod]
        public void ConstructorThrowsWithNullPage() {
            ExceptionAssert.ThrowsArgNull(() => new WebPageHttpHandler(null), "webPage");
        }

        [TestMethod]
        public void IsReusableTest() {
            WebPage dummyPage = new DummyPage();
            Assert.IsFalse(new WebPageHttpHandler(dummyPage).IsReusable);
        }

        [TestMethod]
        public void ProcessRequestTest() {
            var contents = "test";
            var tw = new StringWriter(new StringBuilder());
            var httpContext = CreateContext(tw);
            var page = Utils.CreatePage(p => p.Write(contents));
            new WebPageHttpHandler(page).ProcessRequest(httpContext);
            Assert.AreEqual(contents, tw.ToString());
        }

        [Ignore] // TODO: Figure out how to renable this (need to mock MapPath)
        [TestMethod]
        public void SourceFileHeaderTest() {
            var contents = "test";
            var tw = new StringWriter(new StringBuilder());
            var httpContext = CreateContext(tw);
            var page = Utils.CreatePage(p => p.Write(contents));
            new WebPageHttpHandler(page).ProcessRequest(httpContext);
            Assert.AreEqual(contents, tw.ToString());
            Assert.AreEqual(1, page.PageContext.SourceFiles.Count);
            Assert.IsTrue(page.PageContext.SourceFiles.Contains("~/index.cshtml"));
        }


        [TestMethod]
        public void GenerateSourceFilesHeaderGenerates2047EncodedValue() {
            // Arrange
            string headerKey = null, headerValue = null;
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.Setup(c => c.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback(
                (string key, string value) => { headerKey = key; headerValue = value; });
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Items).Returns(new Hashtable());

            var webPageContext = new WebPageContext(context.Object, page: null, model: null);
            webPageContext.SourceFiles.Add("foo");
            webPageContext.SourceFiles.Add("bar");
            webPageContext.SourceFiles.Add("λ");

            // Act
            WebPageHttpHandler.GenerateSourceFilesHeader(webPageContext);

            // Assert
            Assert.AreEqual(headerKey, "X-SourceFiles");
            Assert.AreEqual(headerValue, "=?UTF-8?B?Zm9vfGJhcnzOuw==?=");
        }

        [TestMethod]
        public void ExceptionTest() {
            var contents = "test";
            var httpContext = CreateContext();
            var page = Utils.CreatePage(p => {
                throw new InvalidOperationException(contents);
            });
            ExceptionAssert.Throws<HttpUnhandledException>(
                () => new WebPageHttpHandler(page).ProcessRequest(httpContext),
                e => e.InnerException is InvalidOperationException && e.InnerException.Message == contents);
        }

        [TestMethod]
        public void SecurityExceptionTest() {
            var contents = "test";
            var httpContext = CreateContext();
            var page = Utils.CreatePage(p => {
                throw new SecurityException(contents);
            });
            ExceptionAssert.Throws<SecurityException>(
                () => new WebPageHttpHandler(page).ProcessRequest(httpContext),
                contents);
        }

        [TestMethod]
        public void CreateFromVirtualPathTest() {
            var contents = "test";
            var tw = new StringWriter(new StringBuilder());
            var httpContext = CreateContext(tw);
            var handler = WebPageHttpHandler.CreateFromVirtualPath("~/hello/test.cshtml",
                (path, type) => Utils.CreatePage(p => p.Write(contents)));
            handler.ProcessRequest(httpContext);
            Assert.AreEqual(contents, tw.ToString());
        }

        [TestMethod]
        public void VersionHeaderTest() {
            bool headerSet = false;
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(response => response.AppendHeader("X-AspNetWebPages-Version", "1.0")).Callback(() => headerSet = true);

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.SetupGet(context => context.Response).Returns(mockResponse.Object);

            WebPageHttpHandler.AddVersionHeader(mockContext.Object);
            Assert.IsTrue(headerSet);
        }

        [TestMethod]
        public void CreateFromVirtualPathNonWebPageTest() {
            var handler = new WebPageHttpHandler(new DummyPage());
            var result = WebPageHttpHandler.CreateFromVirtualPath("~/hello/test.cshtml",
                (path, type) => handler);
            Assert.AreEqual(handler, result);
        }

        private static HttpContext CreateContext() {
            return CreateContext(new StringWriter(new StringBuilder()));
        }

        private static HttpContext CreateContext(TextWriter tw) {
            var filename = "default.aspx";
            var url = "http://localhost/WebSite1/subfolder1/default.aspx";
            var request = new HttpRequest(filename, url, null);
            var response = new HttpResponse(tw);
            var httpContext = new HttpContext(request, response);
            return httpContext;
        }

        private sealed class DummyPage : WebPage {
            public override void Execute() {
            }
        }
    }
}

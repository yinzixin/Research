using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.WebPages.Resources;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class WebPageTest {
        private const string XmlHttpRequestKey = "X-Requested-With";
        private const string XmlHttpRequestValue = "XMLHttpRequest";

        [TestMethod]
        public void NormalizeLayoutPagePathTest() {
            var layoutPage = "Layout.cshtml";
            var layoutPath1 = "~/MyApp/Layout.cshtml";
            var page = new Mock<WebPage>() { CallBase = true }.Object;
            page.VirtualPath = ("~/MyApp/index.cshtml");
            Assert.AreEqual(layoutPath1, page.NormalizeLayoutPagePath(layoutPage, path => path == layoutPath1));
            ExceptionAssert.Throws<HttpException>(() => page.NormalizeLayoutPagePath(layoutPage, path => false),
                String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_LayoutPageNotFound, layoutPage, layoutPath1));
        }

        [TestMethod]
        public void UrlDataBasicTests() {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "one/2/3.0/4.0005");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.AreEqual("one", page.UrlData[0]);
            Assert.AreEqual(2, page.UrlData[1].AsInt());
            Assert.AreEqual(3.0f, page.UrlData[2].AsFloat());
            Assert.AreEqual(4.0005m, page.UrlData[3].AsDecimal());
        }

        [TestMethod]
        public void UrlDataEmptyTests() {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "one///two/");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.AreEqual("one", page.UrlData[0]);
            Assert.IsTrue(page.UrlData[1].IsEmpty());
            Assert.IsTrue(page.UrlData[2].IsEmpty());
            Assert.AreEqual("two", page.UrlData[3]);
            Assert.IsTrue(page.UrlData[4].IsEmpty());
        }

        [TestMethod]
        public void UrlDataReadOnlyTest() {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "one/2/3.0/4.0005");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            ExceptionAssert.Throws<NotSupportedException>(() => { page.UrlData.Add("bogus"); }, "The UrlData collection is read-only.");
            ExceptionAssert.Throws<NotSupportedException>(() => { page.UrlData.Insert(0, "bogus"); }, "The UrlData collection is read-only.");
            ExceptionAssert.Throws<NotSupportedException>(() => { page.UrlData.Remove("one"); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void UrlDataOutOfBoundsTest() {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.AreEqual(String.Empty, page.UrlData[0]);
            Assert.AreEqual(String.Empty, page.UrlData[1]);
        }

        [TestMethod]
        public void CreateInstanceFromVirtualPathTest() {
            var vpath = "~/hello/test.cshtml";
            var page = CreateMockPageWithPostContext().Object;
            var result = WebPage.CreateInstanceFromVirtualPath(vpath, (path, type) => { return page; });
            Assert.AreEqual(page, result);
            Assert.AreEqual(vpath, result.VirtualPath);
        }

        [TestMethod]
        public void NullModelTest() {
            var page = CreateMockPageWithPostContext().Object;
            page.PageContext.Model = null;
            Assert.IsNull(page.Model);
        }

        internal class ModelTestClass {
            public string Prop1 { get; set; }
            public string GetProp1() {
                return Prop1;
            }
            public override string ToString() {
                return Prop1;
            }
        }

        [TestMethod]
        public void ModelTest() {
            var v = "value1";
            var page = CreateMockPageWithPostContext().Object;
            var model = new ModelTestClass() { Prop1 = v };
            page.PageContext.Model = model;
            Assert.IsNotNull(page.Model);
            Assert.AreEqual(v, page.Model.Prop1);
            Assert.AreEqual(v, page.Model.GetProp1());
            Assert.AreEqual(v, page.Model.ToString());
            Assert.AreEqual(model, (ModelTestClass)page.Model);
            // No such property
            Assert.IsNull(page.Model.Prop2);
            // No such method
            ExceptionAssert.Throws<MissingMethodException>(() => page.Model.DoSomething());
        }

        [TestMethod]
        public void AnonymousObjectModelTest() {
            var v = "value1";
            var page = CreateMockPageWithPostContext().Object;
            var model = new { Prop1 = v };
            page.PageContext.Model = model;
            Assert.IsNotNull(page.Model);
            Assert.AreEqual(v, page.Model.Prop1);
            // No such property
            Assert.IsNull(page.Model.Prop2);
            // No such method
            ExceptionAssert.Throws<MissingMethodException>(() => page.Model.DoSomething());
        }

        [TestMethod]
        public void SessionPropertyTest() {
            var page = CreateMockPageWithPostContext().Object;
            Assert.AreEqual(0, page.Session.Count);
        }

        [TestMethod]
        public void AppStatePropertyTest() {
            var page = CreateMockPageWithPostContext().Object;
            Assert.AreEqual(0, page.AppState.Count);
        }

        [TestMethod]
        public void ExecutePageHierarchyTest() {
            var page = new Mock<WebPage>();
            page.Object.TopLevelPage = true;

            var executors = new List<IWebPageRequestExecutor>();

            // First executor returns false
            var executor1 = new Mock<IWebPageRequestExecutor>();
            executor1.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(false);
            executors.Add(executor1.Object);

            // Second executor returns true
            var executor2 = new Mock<IWebPageRequestExecutor>();
            executor2.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(true);
            executors.Add(executor2.Object);

            // Third executor should never get called, since we stop after the first true
            var executor3 = new Mock<IWebPageRequestExecutor>();
            executor3.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(false);
            executors.Add(executor3.Object);

            page.Object.ExecutePageHierarchy(executors);

            // Make sure the first two got called but not the third
            executor1.Verify(exec => exec.Execute(It.IsAny<WebPage>()));
            executor2.Verify(exec => exec.Execute(It.IsAny<WebPage>()));
            executor3.Verify(exec => exec.Execute(It.IsAny<WebPage>()), Times.Never());
        }

        [TestMethod]
        public void IsPostReturnsTrueWhenMethodIsPost() {
            // Arrange
            var page = CreateMockPageWithPostContext();

            // Act and Assert
            Assert.IsTrue(page.Object.IsPost);
        }

        [TestMethod]
        public void IsPostReturnsFalseWhenMethodIsNotPost() {
            // Arrange
            var methods = new[] { "GET", "DELETE", "PUT", "RANDOM" };


            // Act and Assert
            Assert.IsTrue(methods.All(method => !CreateMockPageWithContext(method).Object.IsPost));
        }

        [TestMethod]
        public void IsAjaxReturnsTrueWhenRequestContainsAjaxHeader() {
            // Arrange
            var headers = new NameValueCollection();
            headers.Add("X-Requested-With", "XMLHttpRequest");
            var context = CreateContext("GET", new NameValueCollection(), headers);
            var page = CreatePage(context);

            // Act and Assert
            Assert.IsTrue(page.Object.IsAjax);
        }

        [TestMethod]
        public void IsAjaxReturnsTrueWhenRequestBodyContainsAjaxHeader() {
            // Arrange
            var headers = new NameValueCollection();
            headers.Add("X-Requested-With", "XMLHttpRequest");
            var context = CreateContext("POST", headers, headers);
            var page = CreatePage(context);

            // Act and Assert
            Assert.IsTrue(page.Object.IsAjax);
        }

        [TestMethod]
        public void IsAjaxReturnsFalseWhenRequestDoesNotContainAjaxHeaders() {
            // Arrange
            var page = CreateMockPageWithPostContext();

            // Act and Assert
            Assert.IsTrue(!page.Object.IsAjax);
        }

        private static Mock<WebPage> CreatePage(Mock<HttpContextBase> context) {
            var page = new Mock<WebPage>() { CallBase = true };
            var pageContext = new WebPageContext();
            page.Object.Context = context.Object;
            page.Object.PageContext = pageContext;
            return page;
        }

        private static Mock<WebPage> CreateMockPageWithPostContext() {
            return CreateMockPageWithContext("POST");
        }

        private static Mock<WebPage> CreateMockPageWithContext(string httpMethod) {
            var context = CreateContext(httpMethod, new NameValueCollection());
            var page = CreatePage(context);
            return page;
        }

        private static Mock<HttpContextBase> CreateContext(string httpMethod, NameValueCollection queryString, NameValueCollection httpHeaders = null) {
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.HttpMethod).Returns(httpMethod);
            request.Setup(r => r.QueryString).Returns(queryString);
            request.Setup(r => r.Form).Returns(new NameValueCollection());
            request.Setup(r => r.Files).Returns(new Mock<HttpFileCollectionBase>().Object);
            request.Setup(c => c.Headers).Returns(httpHeaders);
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Items).Returns(new Hashtable());
            context.Setup(c => c.Session).Returns(new Mock<HttpSessionStateBase>().Object);
            context.Setup(c => c.Application).Returns(new Mock<HttpApplicationStateBase>().Object);
            context.Setup(c => c.Cache).Returns(new System.Web.Caching.Cache());
            context.Setup(c => c.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            return context;
        }

    }
}

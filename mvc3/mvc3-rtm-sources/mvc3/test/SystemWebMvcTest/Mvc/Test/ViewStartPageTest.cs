using System.Web.Routing;
using System.Web.TestUtil;
using System.Web.WebPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.Mvc.Test {
    [TestClass]
    public class ViewStartPageTest {
        [TestMethod]
        public void Html_DelegatesToChildPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var helper = new HtmlHelper<object>(new ViewContext() { ViewData = new ViewDataDictionary() }, viewPage.Object, new RouteCollection());
            viewPage.Object.Html = helper;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.Html;

            // Assert
            Assert.AreSame(helper, result);
        }

        [TestMethod]
        public void Url_DelegatesToChildPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var helper = new UrlHelper(new RequestContext());
            viewPage.Object.Url = helper;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.Url;

            // Assert
            Assert.AreSame(helper, result);
        }

        [TestMethod]
        public void ViewContext_DelegatesToChildPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var viewContext = new ViewContext();
            viewPage.Object.ViewContext = viewContext;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.ViewContext;

            // Assert
            Assert.AreSame(viewContext, result);
        }

        [TestMethod]
        public void ViewStartPageChild_ThrowsOnNonMvcChildPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            viewStart.ChildPage = new Mock<WebPage>().Object;

            // Act + Assert
            ExceptionHelper.ExpectException<InvalidOperationException>(delegate() {
                var c = viewStart.ViewStartPageChild;
            }, "A ViewStartPage can be used only with with a page that derives from WebViewPage or another ViewStartPage.");
        }

        [TestMethod]
        public void ViewStartPageChild_WorksWithWebViewPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>();
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.ViewStartPageChild;

            // Assert
            Assert.AreSame(viewPage.Object, result);
        }

        [TestMethod]
        public void ViewStartPageChild_WorksWithAnotherRazorStartPage() {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var anotherViewStart = new Mock<ViewStartPage>();
            viewStart.ChildPage = anotherViewStart.Object;

            // Act
            var result = viewStart.ViewStartPageChild;

            // Assert
            Assert.AreSame(anotherViewStart.Object, result);
        }

        class MockViewStartPage : ViewStartPage {
            public override void Execute() {
            }
        }
    }
}

namespace System.Web.Mvc.Test {
    using System;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RedirectResultTest {

        private static string _baseUrl = "http://www.contoso.com/";

        [TestMethod]
        public void ConstructorSetsUrl() {
            // Act
            var result = new RedirectResult(_baseUrl);

            // Assert
            Assert.AreSame(_baseUrl, result.Url);
            Assert.IsFalse(result.Permanent);
        }

        [TestMethod]
        public void ConstructorSetsUrlAndPermanent() {
            // Act
            var result = new RedirectResult(_baseUrl, permanent: true);

            // Assert
            Assert.AreSame(_baseUrl, result.Url);
            Assert.IsTrue(result.Permanent);
        }

        [TestMethod]
        public void ConstructorWithEmptyUrlThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    new RedirectResult(String.Empty);
                },
                "url");

            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    new RedirectResult(String.Empty, true);
                },
                "url");
        }

        [TestMethod]
        public void ConstructorWithNullUrlThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    new RedirectResult(url: null);
                },
                "url");

            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    new RedirectResult(url: null, permanent: true);
                },
                "url");
        }

        [TestMethod]
        public void ExecuteResultCallsResponseRedirect() {
            // Arrange
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(o => o.Redirect(_baseUrl, false /* endResponse */)).Verifiable();
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(o => o.Response).Returns(mockResponse.Object);
            ControllerContext context = new ControllerContext(mockContext.Object, new RouteData(), new Mock<ControllerBase>().Object);
            var result = new RedirectResult(_baseUrl);

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResponse.Verify();
        }

        [TestMethod]
        public void ExecuteResultWithPermanentCallsResponseRedirectPermanent() {
            // Arrange
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(o => o.RedirectPermanent(_baseUrl, false /* endResponse */)).Verifiable();
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(o => o.Response).Returns(mockResponse.Object);
            ControllerContext context = new ControllerContext(mockContext.Object, new RouteData(), new Mock<ControllerBase>().Object);
            var result = new RedirectResult(_baseUrl, permanent: true);

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResponse.Verify();
        }

        [TestMethod]
        public void ExecuteResultWithNullControllerContextThrows() {
            // Arrange
            var result = new RedirectResult(_baseUrl);

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    result.ExecuteResult(null /* context */);
                },
                "context");
        }

        [TestMethod]
        public void RedirectInChildActionThrows() {
            // Arrange
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.PARENT_ACTION_VIEWCONTEXT] = new ViewContext();
            ControllerContext context = new ControllerContext(new Mock<HttpContextBase>().Object, routeData, new Mock<ControllerBase>().Object);
            RedirectResult result = new RedirectResult(_baseUrl);

            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => result.ExecuteResult(context),
                MvcResources.RedirectAction_CannotRedirectInChildAction
            );
        }

    }
}

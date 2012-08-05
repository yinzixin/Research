namespace System.Web.Mvc.Test {
    using System.Web.Routing;
    using System.Web.SessionState;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MvcRouteHandlerTest {
        [TestMethod]
        public void GetHttpHandlerReturnsMvcHandlerWithRouteData() {
            // Arrange
            var routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            var context = new RequestContext(new Mock<HttpContextBase>().Object, routeData);
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(f => f.GetControllerSessionBehavior(context, "controllerName"))
                             .Returns(SessionStateBehavior.Default)
                             .Verifiable();
            IRouteHandler rh = new MvcRouteHandler(controllerFactory.Object);

            // Act
            IHttpHandler httpHandler = rh.GetHttpHandler(context);

            // Assert
            MvcHandler h = httpHandler as MvcHandler;
            Assert.IsNotNull(h, "The handler should be a valid MvcHandler instance");
            Assert.AreEqual<RequestContext>(context, h.RequestContext);
        }

        [TestMethod]
        public void GetHttpHandlerAsksControllerFactoryForSessionBehaviorOfController() {
            // Arrange
            var httpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            var requestContext = new RequestContext(httpContext.Object, routeData);
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(f => f.GetControllerSessionBehavior(requestContext, "controllerName"))
                             .Returns(SessionStateBehavior.ReadOnly)
                             .Verifiable();
            IRouteHandler routeHandler = new MvcRouteHandler(controllerFactory.Object);

            // Act
            routeHandler.GetHttpHandler(requestContext);

            // Assert
            controllerFactory.Verify();
            httpContext.Verify(c => c.SetSessionStateBehavior(SessionStateBehavior.ReadOnly));
        }
    }
}

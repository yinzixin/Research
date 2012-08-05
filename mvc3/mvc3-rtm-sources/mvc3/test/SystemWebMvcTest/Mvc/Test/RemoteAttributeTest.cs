namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RemoteAttributeTest {
        // Good route name, bad route name
        // Controller + Action

        [TestMethod]
        public void GuardClauses() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RemoteAttribute(null, "controller"),
                "action");
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RemoteAttribute("action", null),
                "controller");
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RemoteAttribute(null),
                "routeName");
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => RemoteAttribute.FormatPropertyForClientValidation(String.Empty),
                "property");
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RemoteAttribute("foo").FormatAdditionalFieldsForClientValidation(String.Empty),
                "property");
        }

        [TestMethod]
        public void IsValidAlwaysReturnsTrue() {
            // Act & Assert
            Assert.IsTrue(new RemoteAttribute("RouteName", "ParameterName").IsValid(null));
            Assert.IsTrue(new RemoteAttribute("ActionName", "ControllerName", "ParameterName").IsValid(null));
        }

        [TestMethod]
        public void BadRouteNameThrows() {
            // Arrange
            ControllerContext context = new ControllerContext();
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("RouteName");

            // Act & Assert
            ExceptionHelper.ExpectArgumentException(
                () => new List<ModelClientValidationRule>(attribute.GetClientValidationRules(metadata, context)),
                "A route named 'RouteName' could not be found in the route collection.\r\nParameter name: name");
        }

        [TestMethod]
        public void NoRouteWithActionControllerThrows() {
            // Arrange
            ControllerContext context = new ControllerContext();
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");

            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => new List<ModelClientValidationRule>(attribute.GetClientValidationRules(metadata, context)),
                "No url for remote validation could be found.");
        }

        [TestMethod]
        public void GoodRouteNameReturnsCorrectClientData() {
            // Arrange
            string url = null;
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("RouteName");
            attribute.RouteTable.Add("RouteName", new Route("my/url", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.AreEqual("remote", rule.ValidationType);
            Assert.AreEqual("'Length' is invalid.", rule.ErrorMessage);
            Assert.AreEqual(2, rule.ValidationParameters.Count);
            Assert.AreEqual("/my/url", rule.ValidationParameters["url"]);
        }

        [TestMethod]
        public void ActionControllerReturnsCorrectClientDataWithoutNamedParameters() {
            // Arrange
            string url = null;

            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");
            attribute.RouteTable.Add(new Route("{controller}/{action}", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.AreEqual("remote", rule.ValidationType);
            Assert.AreEqual("'Length' is invalid.", rule.ErrorMessage);
            Assert.AreEqual(2, rule.ValidationParameters.Count);
            Assert.AreEqual("/Controller/Action", rule.ValidationParameters["url"]);
            Assert.AreEqual("*.Length", rule.ValidationParameters["additionalfields"]);
            ExceptionHelper.ExpectException<KeyNotFoundException>(
                () => rule.ValidationParameters["type"],
                "The given key was not present in the dictionary.");
        }

        [TestMethod]
        public void ActionControllerReturnsCorrectClientDataWithNamedParameters() {
            // Arrange
            string url = null;

            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");
            attribute.HttpMethod = "POST";
            attribute.AdditionalFields = "Password,ConfirmPassword";

            attribute.RouteTable.Add(new Route("{controller}/{action}", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.AreEqual("remote", rule.ValidationType);
            Assert.AreEqual("'Length' is invalid.", rule.ErrorMessage);
            Assert.AreEqual(3, rule.ValidationParameters.Count);
            Assert.AreEqual("/Controller/Action", rule.ValidationParameters["url"]);
            Assert.AreEqual("*.Length,*.Password,*.ConfirmPassword", rule.ValidationParameters["additionalfields"]);
            Assert.AreEqual("POST", rule.ValidationParameters["type"]);
        }

        private ControllerContext GetMockControllerContext(string url) {
            Mock<ControllerContext> context = new Mock<ControllerContext>();
            context.Setup(c => c.HttpContext.Request.ApplicationPath)
                   .Returns("/");
            context.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>()))
                   .Callback<string>(vpath => url = vpath)
                   .Returns(() => url);

            return context.Object;
        }

        private class TestableRemoteAttribute : RemoteAttribute {
            public RouteCollection RouteTable = new RouteCollection();

            public TestableRemoteAttribute(string action, string controller)
                : base(action, controller) { }

            public TestableRemoteAttribute(string routeName)
                : base(routeName) { }

            protected override RouteCollection Routes {
                get {
                    return RouteTable;
                }
            }
        }
    }
}

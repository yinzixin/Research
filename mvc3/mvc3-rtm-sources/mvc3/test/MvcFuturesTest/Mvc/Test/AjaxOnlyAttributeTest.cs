namespace Microsoft.Web.Mvc.Test {
    using System;
    using System.Collections.Specialized;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.Mvc;
    using Moq;

    [TestClass]
    public class AjaxOnlyAttributeTest {

        [TestMethod]
        public void IsValidForRequestReturnsFalseIfHeaderNotPresent() {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();
            ControllerContext controllerContext = GetControllerContext(containsHeader: false);

            // Act
            bool isValid = attr.IsValidForRequest(controllerContext, null);

            // Assert
            Assert.IsFalse(isValid, "If the header *specifically* is not present, the attribute must decline matching the request.");
        }

        [TestMethod]
        public void IsValidForRequestReturnsTrueIfHeaderIsPresent() {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();
            ControllerContext controllerContext = GetControllerContext(containsHeader: true);

            // Act
            bool isValid = attr.IsValidForRequest(controllerContext, null);

            // Assert
            Assert.IsTrue(isValid, "If the header is present, the attribute must match the request.");
        }

        [TestMethod]
        public void IsValidForRequestThrowsIfControllerContextIsNull() {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    attr.IsValidForRequest(null, null);
                }, "controllerContext");
        }

        private static ControllerContext GetControllerContext(bool containsHeader) {
            Mock<ControllerContext> mockContext = new Mock<ControllerContext>() { DefaultValue = DefaultValue.Mock };

            NameValueCollection nvc = new NameValueCollection();
            if (containsHeader) {
                nvc["X-Requested-With"] = "XMLHttpRequest";
            }

            mockContext.Setup(o => o.HttpContext.Request.Headers).Returns(nvc);
            mockContext.Setup(o => o.HttpContext.Request["X-Requested-With"]).Returns("XMLHttpRequest"); // always assume the request contains this, e.g. as a form value

            return mockContext.Object;
        }

    }
}

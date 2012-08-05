using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class WebPageRenderingBaseTest {
        [TestMethod]
        public void SetCultureThrowsIfValueIsNull() {
            // Arrange
            string value = null;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => webPageRenderingBase.Culture = value, "value");
        }

        [TestMethod]
        public void SetCultureThrowsIfValueIsEmpty() {
            // Arrange
            string value = String.Empty;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => webPageRenderingBase.Culture = value, "value");
        }

        [TestMethod]
        public void SetUICultureThrowsIfValueIsNull() {
            // Arrange
            string value = null;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => webPageRenderingBase.UICulture = value, "value");
        }

        [TestMethod]
        public void SetUICultureThrowsIfValueIsEmpty() {
            // Arrange
            string value = String.Empty;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => webPageRenderingBase.UICulture = value, "value");
        }
    }
}

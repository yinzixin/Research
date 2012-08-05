namespace System.Web.Mvc.Test {
    using System.Web.Mvc.Razor;
    using System.Web.WebPages.Razor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MvcWebRazorHostFactoryTest {
        [TestMethod]
        public void Constructor() {
            new MvcWebRazorHostFactory();

            // All is cool
        }

        [TestMethod]
        public void CreateHost_ReplacesRegularHostWithMvcSpecificOne() {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("foo.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(MvcWebPageRazorHost));
        }

        [TestMethod]
        public void CreateHost_DoesNotChangeAppStartFileHost() {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("_appstart.cshtml", null);

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(MvcWebPageRazorHost));
        }

        [TestMethod]
        public void CreateHost_DoesNotChangePageStartFileHost() {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("_pagestart.cshtml", null);

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(MvcWebPageRazorHost));
        }
    }
}

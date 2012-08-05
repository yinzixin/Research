using System.IO;
using System.Text;
using System.Web.WebPages.TestUtils;
using System.Web.WebPages.ApplicationParts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class ResourceHandlerTest {
        private const string _fileContent = "contents of jpeg file";

        [TestMethod]
        public void ResourceHandlerWritesContentsOfFileToStream() {
            // Arrange
            var applicationPart = new ApplicationPart(BuildAssembly(), "~/my-app-assembly");
            MemoryStream stream = new MemoryStream();
            var response = new Mock<HttpResponseBase>();
            response.SetupGet(c => c.OutputStream).Returns(stream);
            response.SetupSet(c => c.ContentType = "image/jpeg").Verifiable();
            var resourceHandler = new ResourceHandler(applicationPart, "bar.foo.jpg");

            // Act
            resourceHandler.ProcessRequest(response.Object);

            // Assert
            response.Verify();
            Assert.AreEqual(Encoding.Default.GetString(stream.ToArray()), _fileContent);
        }

        [TestMethod]
        public void ResourceHandlerThrows404IfResourceNotFound() {
            // Arrange
            var applicationPart = new ApplicationPart(BuildAssembly(), "~/my-app-assembly");
            MemoryStream stream = new MemoryStream();
            var response = new Mock<HttpResponseBase>();
            response.SetupGet(c => c.OutputStream).Returns(stream);
            response.SetupSet(c => c.ContentType = "image/jpeg").Verifiable();
            var resourceHandler = new ResourceHandler(applicationPart, "does-not-exist");

            // Act and Assert
            ExceptionAssert.Throws<HttpException>(() => resourceHandler.ProcessRequest(response.Object),
                "The resource file \"does-not-exist\" could not be found.");
        }


        private static IResourceAssembly BuildAssembly(string name = "my-assembly") {
            Mock<TestResourceAssembly> assembly = new Mock<TestResourceAssembly>();
            assembly.SetupGet(c => c.Name).Returns("my-assembly");

            byte[] content = Encoding.Default.GetBytes(_fileContent);
            assembly.Setup(c => c.GetManifestResourceStream("my-assembly.bar.foo.jpg")).Returns(new MemoryStream(content));

            assembly.Setup(c => c.GetManifestResourceNames()).Returns(new[] { "my-assembly.bar.foo.jpg" });

            return assembly.Object;
        }
    }
}

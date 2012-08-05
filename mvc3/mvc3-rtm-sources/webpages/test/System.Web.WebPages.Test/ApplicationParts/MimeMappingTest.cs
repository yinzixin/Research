using System.Web.WebPages.TestUtils;
using Microsoft.Internal.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class MimeMappingTest {
        [TestMethod]
        public void MimeMappingThrowsForNullFileName() {
            // Arrange
            string fileName = null;

            // Act and Assert
            ExceptionAssert.ThrowsArgNull(() => MimeMapping.GetMimeMapping(fileName), "fileName");
        }

        [TestMethod]
        public void MimeMappingReturnsGenericTypeForUnknownExtensions() {
            // Arrange
            string fileName = "file.does-not-exist";

            // Act
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            // Assert
            Assert.AreEqual("application/octet-stream", mimeType);
        }

        [TestMethod]
        public void MimeMappingReturnsGenericTypeForNoExtensions() {
            // Arrange
            string fileName = "file";

            // Act
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            // Assert
            Assert.AreEqual("application/octet-stream", mimeType);
        }


    }
}

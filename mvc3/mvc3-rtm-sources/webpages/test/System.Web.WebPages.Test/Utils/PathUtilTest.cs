using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class PathUtilTest {
        [TestMethod]
        public void GetExtensionForNullPathsReturnsNull() {
            // Arrange
            string path = null;

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.IsNull(extension);
        }

        [TestMethod]
        public void GetExtensionForEmptyPathsReturnsEmptyString() {
            // Arrange
            string path = String.Empty;

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.AreEqual(0, extension.Length);
        }

        [TestMethod]
        public void GetExtensionReturnsEmptyStringForPathsThatDoNotContainExtension() {
            // Arrange
            string[] paths = new[] { "SomePath", "SomePath/", "SomePath/MorePath", "SomePath/MorePath/" };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.IsTrue(extensions.All(ext => ext.Length == 0));
        }

        [TestMethod]
        public void GetExtensionReturnsEmptyStringForPathsContainingPathInfo() {
            // Arrange
            string[] paths = new[] { "SomePath.cshtml/", "SomePath.html/path/info" };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.IsTrue(extensions.All(ext => ext.Length == 0));
        }

        [TestMethod]
        public void GetExtensionReturnsEmptyStringForPathsTerminatingWithADot() {
            // Arrange
            string[] paths = new[] { "SomePath.", "SomeDirectory/SomePath/SomePath.", "SomeDirectory/SomePath.foo." };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.IsTrue(extensions.All(ext => ext.Length == 0));
        }

        [TestMethod]
        public void GetExtensionReturnsExtensionsForPathsTerminatingInExtension() {
            // Arrange
            string path1 = "SomePath.cshtml";
            string path2 = "SomeDir/SomePath.txt";

            // Act
            string ext1 = PathUtil.GetExtension(path1);
            string ext2 = PathUtil.GetExtension(path2);

            // Assert
            Assert.AreEqual(ext1, ".cshtml");
            Assert.AreEqual(ext2, ".txt");
        }

        [TestMethod]
        public void GetExtensionDoesNotThrowForPathsWithInvalidCharacters() {
            // Arrange
            // Repro from test case in Bug 93828
            string path = "Insights/110786998958803%7C2.d24wA6Y3MiT2w8p3OT4yTw__.3600.1289415600-708897727%7CRLN-t1w9bXtKWZ_11osz15Rk_jY";

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.AreEqual(".1289415600-708897727%7CRLN-t1w9bXtKWZ_11osz15Rk_jY", extension);
        }
    }
}

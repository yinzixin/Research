using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class ApplicationPartTest {
        [TestMethod]
        public void ResolveVirtualPathResolvesRegularPathsUsingBaseVirtualPath() {
            // Arrange
            var basePath = "~/base/";
            var path = "somefile";
            var appPartRoot = "~/app/";

            // Act
            var virtualPath = ApplicationPart.ResolveVirtualPath(appPartRoot, basePath, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/base/somefile");
        }

        [TestMethod]
        public void ResolveVirtualPathResolvesAppRelativePathsUsingAppVirtualPath() {
            // Arrange
            var basePath = "~/base";
            var path = "@/somefile";
            var appPartRoot = "~/app/";

            // Act
            var virtualPath = ApplicationPart.ResolveVirtualPath(appPartRoot, basePath, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/app/somefile");
        }

        [TestMethod]
        public void ResolveVirtualPathDoesNotAffectRootRelativePaths() {
            // Arrange
            var basePath = "~/base";
            var path = "~/somefile";
            var appPartRoot = "~/app/";

            // Act
            var virtualPath = ApplicationPart.ResolveVirtualPath(appPartRoot, basePath, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/somefile");
        }

        [TestMethod]
        public void GetResourceNameFromVirtualPathForTopLevelPath() {
            // Arrange
            var moduleName = "my-module";
            var path = "foo.baz";

            // Act 
            var name = ApplicationPart.GetResourceNameFromVirtualPath(moduleName, path);

            // Assert
            Assert.AreEqual(name, moduleName + "." + path);
        }

        [TestMethod]
        public void GetResourceNameFromVirtualPathForItemInSubDir() {
            // Arrange
            var moduleName = "my-module";
            var path = "/bar/foo";

            // Act 
            var name = ApplicationPart.GetResourceNameFromVirtualPath(moduleName, path);

            // Assert
            Assert.AreEqual(name, "my-module.bar.foo");
        }

        [TestMethod]
        public void GetResourceNameFromVirtualPathForItemWithSpaces() {
            // Arrange
            var moduleName = "my-module";
            var path = "/program files/data files/my file .foo";

            // Act 
            var name = ApplicationPart.GetResourceNameFromVirtualPath(moduleName, path);

            // Assert
            Assert.AreEqual(name, "my-module.program_files.data_files.my file .foo");
        }

        [TestMethod]
        public void GetResourceVirtualPathForTopLevelItem() {
            // Arrange
            var moduleName = "my-module";
            var moduleRoot = "~/root-path";
            var path = moduleRoot + "/foo.txt";

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + moduleName + "/" + "foo.txt");
        }

        [TestMethod]
        public void GetResourceVirtualPathForTopLevelItemAndModuleRootWithTrailingSlash() {
            // Arrange
            var moduleName = "my-module";
            var moduleRoot = "~/root-path/";
            var path = moduleRoot + "/foo.txt";

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + moduleName + "/" + "foo.txt");
        }

        [TestMethod]
        public void GetResourceVirtualPathForTopLevelItemAndNestedModuleRootPath() {
            // Arrange
            var moduleName = "my-module";
            var moduleRoot = "~/root-path/sub-path";
            var path = moduleRoot + "/foo.txt";

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + moduleName + "/" + "foo.txt");
        }

        [TestMethod]
        public void GetResourceVirtualPathEncodesModuleName() {
            // Arrange
            var moduleName = "Debugger Package v?&%";
            var moduleRoot = "~/root-path/sub-path";
            var path = moduleRoot + "/foo.txt";

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + "Debugger%20Package%20v?&%" + "/" + "foo.txt");
        }

        [TestMethod]
        public void GetResourceVirtualPathForNestedItemPath() {
            // Arrange
            var moduleName = "DebuggerPackage";
            var moduleRoot = "~/root-path/sub-path";
            var itemPath = "some-path/some-more-please/foo.txt";
            var path = moduleRoot + "/" + itemPath;

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + moduleName + "/" + itemPath);
        }

        [TestMethod]
        public void GetResourceVirtualPathForItemPathWithParameters() {
            // Arrange
            var moduleName = "DebuggerPackage";
            var moduleRoot = "~/root-path/sub-path";
            var itemPath = "some-path/some-more-please/foo.jpg?size=45&height=20";
            var path = moduleRoot + "/" + itemPath;

            // Act
            var virtualPath = ApplicationPart.GetResourceVirtualPath(moduleName, moduleRoot, path);

            // Assert
            Assert.AreEqual(virtualPath, "~/r.ashx/" + moduleName + "/" + itemPath);
        }
    }
}

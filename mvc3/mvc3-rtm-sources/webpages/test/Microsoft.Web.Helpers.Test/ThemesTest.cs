using System;
using System.Web.Hosting;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Web.Helpers.Test {
    [TestClass]
    public class ThemesTest {
        [TestMethod]
        public void Initialize_WithBadParams_Throws() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Themes.Initialize(null, "foo"), "themeDirectory");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Themes.Initialize("", "foo"), "themeDirectory");

            ExceptionAssert.ThrowsArgNullOrEmpty(() => Themes.Initialize("~/folder", null), "defaultTheme");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Themes.Initialize("~/folder", ""), "defaultTheme");
        }

        [TestMethod]
        public void CurrentThemeThrowsIfAssignedNullOrEmpty() {
            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => { Themes.CurrentTheme = null; }, "value");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => { Themes.CurrentTheme = String.Empty; }, "value");
        }

        [TestMethod]
        public void InitializationTests() {
            // Ensure that these properties are invoked only through this method since they are static. Any call that involves using the value 
            // of these properties are flaky after this call.
            

            // Attempt to read properties before initialization
            ExceptionAssert.Throws<InvalidOperationException>(() => Themes.CurrentTheme = "Foo", 
                @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            ExceptionAssert.Throws<InvalidOperationException>(() => { var x = Themes.CurrentTheme; },
                @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            ExceptionAssert.Throws<InvalidOperationException>(() => {var x = Themes.AvailableThemes; }, 
                   @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            ExceptionAssert.Throws<InvalidOperationException>(() => { var x = Themes.DefaultTheme; },
                   @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            ExceptionAssert.Throws<InvalidOperationException>(() => { var x = Themes.GetResourcePath("baz"); },
               @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            ExceptionAssert.Throws<InvalidOperationException>(() => { var x = Themes.GetResourcePath("baz", "some-file"); },
               @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";
            Themes.Initialize(themeDirectory: themeDirectory, defaultTheme: defaultTheme);

            // Ensure Theme use scope storage to store properties
            Assert.AreEqual(ScopeStorage.CurrentScope[Themes._themesInitializedKey], true);
            Assert.AreEqual(ScopeStorage.CurrentScope[Themes._themeDirectoryKey], themeDirectory);
            Assert.AreEqual(ScopeStorage.CurrentScope[Themes._defaultThemeKey], defaultTheme);

            // CurrentTheme falls back to default theme when null
            Assert.AreEqual(Themes.CurrentTheme, defaultTheme);

            var value = "random-value";
            Themes.CurrentTheme = value;
            Assert.AreEqual(Themes.CurrentTheme, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[Themes._currentThemeKey], value);
        }

        [TestMethod]
        public void GetResource_ReturnsCorrectResource() {

            // if the current folder has the file, give the current folder's file
            Assert.AreEqual("current", Themes.GetResourcePath("current", "default", "folder", "foo.cs", GetResourceHelperMethod));

            // if current folder doesn't but default does, fall through to the default's file
            Assert.AreEqual("fallthrough", Themes.GetResourcePath("bad", "default", "folder", "foo.cs", GetResourceHelperMethod));

            // if neither have them, the result should be null
            Assert.IsNull(Themes.GetResourcePath("bad", "bad", "folder", "foo.cs", GetResourceHelperMethod));
        }

        [TestMethod]
        public void AvaliableThemes() {
            // Arrange
            var mockParentDir = new Mock<VirtualDirectory>("parent");
            mockParentDir.SetupGet(v => v.Name).Returns("parent");

            var mockDir = new Mock<VirtualDirectory>("bar");
            mockDir.SetupGet(v => v.Name).Returns("bar");

            mockParentDir.SetupGet(v => v.Directories).Returns(new[] { mockDir.Object });

            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(v => v.GetDirectory("parent")).Returns(mockParentDir.Object);

            // Act
            var actual = Themes.GetAvailableThemes(vpp.Object, "parent");

            // Assert
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("bar", actual[0]);
        }

        [TestMethod]
        public void NonExistentDirectory_MatchingFile_ReturnsNull() {
            // Arrange

            // build vpp
            var vpp = new Mock<VirtualPathProvider>();

            var mockNoFilesDir = new Mock<VirtualDirectory>("nofiles");

            vpp.Setup(v => v.GetDirectory("nofiles")).Returns(mockNoFilesDir.Object);

            // Act
            var actual = Themes.FindMatchingFile(vpp.Object, "wrongfolder", "file.cs");

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void FileWithSlash_ReturnsNull() {
            // Arrange

            // folder structure:
            // /root
            //   /foo
            //      /bar.cs
            // testing that a file specified as foo/bar in folder root will return null
            var vpp = new Mock<VirtualPathProvider>();

            var mockRootDir = new Mock<VirtualDirectory>("root");

            var mockFooDir = new Mock<VirtualDirectory>("foo");

            var mockFile = new Mock<VirtualFile>("/root/foo/bar.cs");

            mockFooDir.SetupGet(v => v.Files).Returns(new[] { mockFile.Object });

            mockRootDir.SetupGet(v => v.Directories).Returns(new[] { mockFooDir.Object });

            vpp.Setup(v => v.GetDirectory("root")).Returns(mockRootDir.Object);

            // Act
            var actual = Themes.FindMatchingFile(vpp.Object, "root", "foo/bar.cs");

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EmptyDirectory_MatchingFile_ReturnsNull() {
            // Arrange

            // build vpp
            var vpp = new Mock<VirtualPathProvider>();

            var mockNoFilesDir = new Mock<VirtualDirectory>("nofiles");

            vpp.Setup(v => v.GetDirectory("nofiles")).Returns(mockNoFilesDir.Object);

            // Act
            var actual = Themes.FindMatchingFile(vpp.Object, "nofiles", "file.cs");

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void MatchingFiles_ReturnsCorrectFile() {
            // Arrange

            // build vpp
            var vpp = new Mock<VirtualPathProvider>();

            var mockNoMatchingFilesDir = new Mock<VirtualDirectory>("nomatchingfiles");

            var mockFile = new Mock<VirtualFile>("/nomatchingfiles/foo.cs");

            mockNoMatchingFilesDir.SetupGet(v => v.Files).Returns(new[] { mockFile.Object });

            vpp.Setup(v => v.GetDirectory("nomatchingfiles")).Returns(mockNoMatchingFilesDir.Object);

            // Act
            var bar = Themes.FindMatchingFile(vpp.Object, "nomatchingfiles", "bar.cs");
            var foo = Themes.FindMatchingFile(vpp.Object, "nomatchingfiles", "foo.cs");
            // Assert
            Assert.IsNull(bar);
            Assert.AreEqual("/nomatchingfiles/foo.cs", foo);
        }

        // Helper method to test that when given the correct args
        // we can get the corre fallthrough behavior.
        // ("currentfolder", ---) == "current"
        // ("defaultfolder", ---) == "fallthrough
        // ("badfolder", ---) == null
        private static string GetResourceHelperMethod(string arg1, string arg2) {
            if (arg1.Equals(@"current\folder")) {
                return "current";
            }
            else if (arg1.Equals(@"default\folder")) {
                return "fallthrough";
            }
            else {
                return null;
            }
        }
    }
}

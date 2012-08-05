namespace System.Web.Mvc.Razor.Test {
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MvcWebPageRazorHostTest {
        [TestMethod]
        public void Constructor() {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");

            Assert.AreEqual("foo.cshtml", host.VirtualPath);
            Assert.AreEqual("bar", host.PhysicalPath);
            Assert.AreEqual(typeof(WebViewPage).FullName, host.DefaultBaseClass);
        }

        [TestMethod]
        public void ConstructorRemovesUnwantedNamespaceImports() {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");

            Assert.IsFalse(host.NamespaceImports.Contains("System.Web.WebPages.Html"));

            // Even though MVC no longer needs to remove the following two namespaces
            // (because they are no longer imported by System.Web.WebPages), we want
            // to make sure that they don't get introduced again by default.
            Assert.IsFalse(host.NamespaceImports.Contains("WebMatrix.Data"));
            Assert.IsFalse(host.NamespaceImports.Contains("WebMatrix.WebData"));
        }

        [TestMethod]
        public void DecorateCodeGenerator_ThrowsOnNull() {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            ExceptionHelper.ExpectArgumentNullException(delegate() {
                host.DecorateCodeGenerator(null);
            }, "incomingCodeGenerator");
        }

        [TestMethod]
        public void DecorateGodeGenerator_ReplacesCSharpCodeGeneratorWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            var generator = new CSharpRazorCodeGenerator("someClass", "root.name", "foo.cshtml", host);

            // Act
            var result = host.DecorateCodeGenerator(generator);

            // Assert
            Assert.IsInstanceOfType(result, typeof(MvcCSharpRazorCodeGenerator));
            Assert.AreEqual("someClass", result.ClassName);
            Assert.AreEqual("root.name", result.RootNamespaceName);
            Assert.AreEqual("foo.cshtml", result.SourceFileName);
            Assert.AreSame(host, result.Host);
        }

        [TestMethod]
        public void DecorateGodeGenerator_ReplacesVBCodeGeneratorWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.vbhtml", "bar");
            var generator = new VBRazorCodeGenerator("someClass", "root.name", "foo.vbhtml", host);

            // Act
            var result = host.DecorateCodeGenerator(generator);

            // Assert
            Assert.IsInstanceOfType(result, typeof(MvcVBRazorCodeGenerator));
            Assert.AreEqual("someClass", result.ClassName);
            Assert.AreEqual("root.name", result.RootNamespaceName);
            Assert.AreEqual("foo.vbhtml", result.SourceFileName);
            Assert.AreSame(host, result.Host);
        }

        [TestMethod]
        public void DecorateCodeParser_ThrowsOnNull() {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            ExceptionHelper.ExpectArgumentNullException(delegate() {
                host.DecorateCodeParser(null);
            }, "incomingCodeParser");
        }

        [TestMethod]
        public void DecorateCodeParser_ReplacesCSharpCodeParserWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            var parser = new CSharpCodeParser();

            // Act
            var result = host.DecorateCodeParser(parser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(MvcCSharpRazorCodeParser));
        }

        [TestMethod]
        public void DecorateCodeParser_ReplacesVBCodeParserWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.vbhtml", "bar");
            var parser = new VBCodeParser();

            // Act
            var result = host.DecorateCodeParser(parser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(MvcVBRazorCodeParser));
        }
    }
}

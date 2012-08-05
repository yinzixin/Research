using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test {
    [TestClass]
    public class CSharpRazorCodeLanguageTest {

        [TestMethod]
        public void CreateCodeParserReturnsNewCSharpCodeParser() {
            // Arrange
            RazorCodeLanguage service = new CSharpRazorCodeLanguage();

            // Act
            ParserBase parser = service.CreateCodeParser();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser, typeof(CSharpCodeParser));
        }

        [TestMethod]
        public void CreateCodeGeneratorParserListenerReturnsNewCSharpCodeGeneratorParserListener() {
            // Arrange
            RazorCodeLanguage service = new CSharpRazorCodeLanguage();

            // Act
            RazorEngineHost host = new RazorEngineHost(service);
            RazorCodeGenerator generator = service.CreateCodeGenerator("Foo", "Bar", "Baz", host);

            // Assert
            Assert.IsNotNull(generator);
            Assert.IsInstanceOfType(generator, typeof(CSharpRazorCodeGenerator));
            Assert.AreEqual("Foo", generator.ClassName);
            Assert.AreEqual("Bar", generator.RootNamespaceName);
            Assert.AreEqual("Baz", generator.SourceFileName);
            Assert.AreSame(host, generator.Host);
        }

        [TestMethod]
        public void CodeDomProviderTypeReturnsVBCodeProvider() {
            // Arrange
            RazorCodeLanguage service = new CSharpRazorCodeLanguage();

            // Assert
            Assert.AreEqual(typeof(CSharpCodeProvider), service.CodeDomProviderType);
        }
    }
}

using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test {
    [TestClass]
    public class VBRazorCodeLanguageTest {
        [TestMethod]
        public void CreateCodeParserReturnsNewVBCodeParser() {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Act
            ParserBase parser = service.CreateCodeParser();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser, typeof(VBCodeParser));
        }

        [TestMethod]
        public void CreateCodeGeneratorParserListenerReturnsNewCSharpCodeGeneratorParserListener() {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Act
            RazorEngineHost host = new RazorEngineHost(new VBRazorCodeLanguage());
            RazorCodeGenerator generator = service.CreateCodeGenerator("Foo", "Bar", "Baz", host);

            // Assert
            Assert.IsNotNull(generator);
            Assert.IsInstanceOfType(generator, typeof(VBRazorCodeGenerator));
            Assert.AreEqual("Foo", generator.ClassName);
            Assert.AreEqual("Bar", generator.RootNamespaceName);
            Assert.AreEqual("Baz", generator.SourceFileName);
            Assert.AreSame(host, generator.Host);
        }

        [TestMethod]
        public void CodeDomProviderTypeReturnsVBCodeProvider() {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Assert
            Assert.AreEqual(typeof(VBCodeProvider), service.CodeDomProviderType);
        }
    }
}

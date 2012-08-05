using CodeDom = System.CodeDom.Compiler;
using System.IO;
using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;
using System.Web.Razor.Generator;

namespace System.Web.Razor.Test {
    [TestClass]
    public class RazorTemplateEngineTest {
        [TestMethod]
        public void ConstructorRequiresNonNullHost() {
            ExceptionAssert.ThrowsArgNull(() => new RazorTemplateEngine(null), "host");
        }

        [TestMethod]
        public void ConstructorInitializesHost() {
            // Arrange
            RazorEngineHost host = new RazorEngineHost(new CSharpRazorCodeLanguage());

            // Act
            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            // Assert
            Assert.AreSame(host, engine.Host);
        }

        [TestMethod]
        public void CreateParserMethodIsConstructedFromHost() {
            // Arrange
            RazorEngineHost host = CreateHost();
            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            // Act
            RazorParser parser = engine.CreateParser();
            
            // Assert
            Assert.IsInstanceOfType(parser.CodeParser, typeof(CSharpCodeParser));
            Assert.IsInstanceOfType(parser.MarkupParser, typeof(HtmlMarkupParser));
        }

        [TestMethod]
        public void CreateParserMethodSetsParserContextToDesignTimeModeIfHostSetToDesignTimeMode() {
            // Arrange
            RazorEngineHost host = CreateHost();
            RazorTemplateEngine engine = new RazorTemplateEngine(host);
            host.DesignTimeMode = true;

            // Act
            RazorParser parser = engine.CreateParser();

            // Assert
            Assert.IsTrue(parser.DesignTimeMode);
        }

        [TestMethod]
        public void CreateParserMethodPassesParsersThroughDecoratorMethodsOnHost() {
            // Arrange
            CodeParser expectedCode = new Mock<CodeParser>().Object;
            MarkupParser expectedMarkup = new Mock<MarkupParser>().Object;

            var mockHost = new Mock<RazorEngineHost>(new CSharpRazorCodeLanguage()) { CallBase = true };
            mockHost.Setup(h => h.DecorateCodeParser(It.IsAny<CSharpCodeParser>()))
                    .Returns(expectedCode);
            mockHost.Setup(h => h.DecorateMarkupParser(It.IsAny<HtmlMarkupParser>()))
                    .Returns(expectedMarkup);
            RazorTemplateEngine engine = new RazorTemplateEngine(mockHost.Object);

            // Act
            RazorParser actual = engine.CreateParser();

            // Assert
            Assert.AreEqual(expectedCode, actual.CodeParser);
            Assert.AreEqual(expectedMarkup, actual.MarkupParser);
        }

        [TestMethod]
        public void CreateCodeGeneratorMethodPassesCodeGeneratorThroughDecorateMethodOnHost() {
            // Arrange
            var mockHost = new Mock<RazorEngineHost>(new CSharpRazorCodeLanguage()) { CallBase = true };
            
            RazorCodeGenerator expected = new Mock<RazorCodeGenerator>("Foo", "Bar", "Baz", mockHost.Object).Object;
            
            mockHost.Setup(h => h.DecorateCodeGenerator(It.IsAny<CSharpRazorCodeGenerator>()))
                    .Returns(expected);
            RazorTemplateEngine engine = new RazorTemplateEngine(mockHost.Object);

            // Act
            RazorCodeGenerator actual = engine.CreateCodeGenerator("Foo", "Bar", "Baz");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTemplateWrapsTextBufferInTextBufferReaderAndPassesToParseTemplateCore() {
            // Arrange
            Mock<RazorTemplateEngine> mockEngine = new Mock<RazorTemplateEngine>(CreateHost());
            TextReader reader = new StringReader("foo");
            CancellationTokenSource source = new CancellationTokenSource();

            // Act
            mockEngine.Object.ParseTemplate(reader, cancelToken: source.Token);

            // Assert
            mockEngine.Verify(e => e.ParseTemplateCore(It.Is<LookaheadTextReader>(l => ReferenceEquals(((BufferingTextReader)l).InnerReader, reader)),
                                                       source.Token));
        }

        [TestMethod]
        public void GenerateCodeWrapsTextBufferInTextBufferReaderAndPassesToGenerateCodeCore() {
            // Arrange
            Mock<RazorTemplateEngine> mockEngine = new Mock<RazorTemplateEngine>(CreateHost());
            TextReader reader = new StringReader("foo");
            CancellationTokenSource source = new CancellationTokenSource();
            string className = "Foo";
            string ns = "Bar";
            string src = "Baz";

            // Act
            mockEngine.Object.GenerateCode(reader, className: className, rootNamespace: ns, sourceFileName: src, cancelToken: source.Token);

            // Assert
            mockEngine.Verify(e => e.GenerateCodeCore(It.Is<LookaheadTextReader>(l => ReferenceEquals(((BufferingTextReader)l).InnerReader, reader)),
                                                      className, ns, src, source.Token));
        }

        [TestMethod]
        public void ParseTemplateOutputsResultsOfParsingProvidedTemplateSource() {
            // Arrange
            RazorTemplateEngine engine = new RazorTemplateEngine(CreateHost());

            // Act
            ParserResults results = engine.ParseTemplate(new StringTextBuffer("foo @bar("));

            // Assert
            Assert.IsFalse(results.Success);
            Assert.AreEqual(1, results.ParserErrors.Count);
            Assert.IsNotNull(results.Document);
        }

        [TestMethod]
        public void GenerateOutputsResultsOfParsingAndGeneration() {
            // Arrange
            RazorTemplateEngine engine = new RazorTemplateEngine(CreateHost());

            // Act
            GeneratorResults results = engine.GenerateCode(new StringTextBuffer("foo @bar("));

            // Assert
            Assert.IsFalse(results.Success);
            Assert.AreEqual(1, results.ParserErrors.Count);
            Assert.IsNotNull(results.Document);
            Assert.IsNotNull(results.GeneratedCode);
            Assert.IsNull(results.DesignTimeLineMappings);
        }

        [TestMethod]
        public void GenerateOutputsDesignTimeMappingsIfDesignTimeSetOnHost() {
            // Arrange
            RazorTemplateEngine engine = new RazorTemplateEngine(CreateHost(designTime: true));

            // Act
            GeneratorResults results = engine.GenerateCode(new StringTextBuffer("foo @bar()"), className: null, rootNamespace: null, sourceFileName: "foo.cshtml");

            // Assert
            Assert.IsTrue(results.Success);
            Assert.AreEqual(0, results.ParserErrors.Count);
            Assert.IsNotNull(results.Document);
            Assert.IsNotNull(results.GeneratedCode);
            Assert.IsNotNull(results.DesignTimeLineMappings);
        }

        private static RazorEngineHost CreateHost(bool designTime = false) {
            return new RazorEngineHost(new CSharpRazorCodeLanguage()) {
                DesignTimeMode = designTime
            };
        }
    }
}

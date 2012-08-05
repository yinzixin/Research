namespace System.Web.Mvc.Razor.Test {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MvcCSharpRazorCodeParserTest {
        [TestMethod]
        public void Constructor_AddsModelKeyword() {
            var parser = new TestMvcCSharpRazorCodeParser();

            Assert.IsTrue(parser.RazorKeywords.ContainsKey("model"));
        }

        [TestMethod]
        public void ParseModelKeyword_HandlesSingleInstance() {
            // Arrange + Act
            var document = "@model    Foo";
            var spans = ParseDocument(document);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(7, 0, 7), "   Foo", "Foo"),
            };
            CollectionAssert.AreEqual(expectedSpans, spans);
        }

        [TestMethod]
        public void ParseModelKeyword_HandlesNullableTypes() {
            // Arrange + Act
            var document = "@model Foo?\r\nBar";
            var spans = ParseDocument(document);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(7, 0, 7), "Foo?\r\n", "Foo?"),
                new MarkupSpan(new SourceLocation(13, 1, 0), "Bar"),

            };
            CollectionAssert.AreEqual(expectedSpans, spans);
        }

        [TestMethod]
        public void ParseModelKeyword_ErrorOnMissingModelType() {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document = "@model   ";
            var spans = ParseDocument(document, errors);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new CodeSpan(new SourceLocation(7, 0, 7), "  "),
            };
            var expectedErrors = new[] {
                new RazorError("The 'model' keyword must be followed by a type name on the same line.", new SourceLocation(6, 0, 6), 1)
            };
            CollectionAssert.AreEqual(expectedSpans, spans);
            CollectionAssert.AreEqual(expectedErrors, errors);
        }

        [TestMethod]
        public void ParseModelKeyword_ErrorOnMultipleModelStatements() {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
@"@model Foo
@model Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(7, 0, 7), "Foo\r\n", "Foo"),
                new TransitionSpan(new SourceLocation(12, 1, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(13, 1, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(19, 1, 7), "Bar", "Bar"),
            };

            var expectedErrors = new[] {
                new RazorError("Only one 'model' statement is allowed in a file.", new SourceLocation(18, 1, 6), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.AreEqual(i.expected, i.span));
            CollectionAssert.AreEqual(expectedSpans, spans);
            CollectionAssert.AreEqual(expectedErrors, errors);
        }

        [TestMethod]
        public void ParseModelKeyword_ErrorOnModelFollowedByInherits() {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
@"@model Foo
@inherits Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(7, 0, 7), "Foo\r\n", "Foo"),
                new TransitionSpan(new SourceLocation(12, 1, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(13, 1, 1), "inherits ") { AcceptedCharacters = AcceptedCharacters.None },
                new InheritsSpan(new SourceLocation(22, 1, 10), "Bar", "Bar")
            };

            var expectedErrors = new[] {
                new RazorError("The 'inherits' keyword is not allowed when a 'model' keyword is used.", new SourceLocation(21, 1, 9), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.AreEqual(i.expected, i.span));
            CollectionAssert.AreEqual(expectedSpans, spans, "Spans do not match");
            CollectionAssert.AreEqual(expectedErrors, errors, "Errors do not match");
        }


        [TestMethod]
        public void ParseModelKeyword_ErrorOnInheritsFollowedByModel() {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
@"@inherits Bar
@model Foo";
            var spans = ParseDocument(document, errors);

            // Assert
            var expectedSpans = new Span[] {
                new TransitionSpan(new SourceLocation(0, 0, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(1, 0, 1), "inherits ") { AcceptedCharacters = AcceptedCharacters.None },
                new InheritsSpan(new SourceLocation(10, 0, 10), "Bar\r\n", "Bar"),
                new TransitionSpan(new SourceLocation(15, 1, 0), "@") { AcceptedCharacters = AcceptedCharacters.None },
                new MetaCodeSpan(new SourceLocation(16, 1, 1), "model ") { AcceptedCharacters = AcceptedCharacters.None },
                new ModelSpan(new SourceLocation(22, 1, 7), "Foo", "Foo"),
            };

            var expectedErrors = new[] {
                new RazorError("The 'inherits' keyword is not allowed when a 'model' keyword is used.", new SourceLocation(9, 0, 9), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.AreEqual(i.expected, i.span));
            CollectionAssert.AreEqual(expectedSpans, spans, "Spans do not match");
            CollectionAssert.AreEqual(expectedErrors, errors, "Errors do not match");
        }

        private static List<Span> ParseDocument(string documentContents, List<RazorError> errors = null) {
            errors = errors ?? new List<RazorError>();
            var reader = new BufferingTextReader(new StringReader(documentContents));
            var markupParser = new HtmlMarkupParser();
            var parserConsumerMock = new Mock<ParserVisitor>();
            List<Span> spans = new List<Span>();
            parserConsumerMock.Setup(consumer => consumer.VisitSpan(It.IsAny<Span>())).Callback<Span>(span => spans.Add(span));
            parserConsumerMock.Setup(consumer => consumer.VisitError(It.IsAny<RazorError>())).Callback<RazorError>(error => errors.Add(error));
            var codeParser = new TestMvcCSharpRazorCodeParser();
            var context = new ParserContext(reader, codeParser, markupParser, markupParser, parserConsumerMock.Object);
            codeParser.Context = context;
            markupParser.Context = context;
            markupParser.ParseDocument();
            return spans;
        }

        private sealed class TestMvcCSharpRazorCodeParser : MvcCSharpRazorCodeParser {
            public new Dictionary<string, object> RazorKeywords { get { return base.RazorKeywords.ToDictionary(pair => pair.Key, pair => (object)pair.Value); } }
        }
    }
}

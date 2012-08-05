using System.IO;
using System.Web.Razor.Parser;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class RazorParserTest {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConstructorRequiresNonNullCodeParser() {
            ExceptionAssert.ThrowsArgNull(() => new RazorParser(null, new HtmlMarkupParser()), "codeParser");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullMarkupParser() {
            ExceptionAssert.ThrowsArgNull(() => new RazorParser(new CSharpCodeParser(), null), "markupParser");
        }

        [TestMethod]
        public void ParseMethodCallsParseDocumentOnMarkupParserAndReturnsResults() {
            // Arrange
            RazorParser parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());

            // Act/Assert
            ParserTestBase.EvaluateResults(TestContext,
                                           parser.Parse(new StringReader("foo @bar baz")),
                                           new MarkupBlock(
                                            new MarkupSpan("foo "),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(" baz")
                                           ));
        }

        [TestMethod]
        public void ParseMethodUsesProvidedParserListenerIfSpecified() {
            // Arrange
            RazorParser parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            SyntaxTreeBuilderVisitor builder = new SyntaxTreeBuilderVisitor();

            // Act
            parser.Parse(new StringReader("foo @bar baz"), builder);

            // Assert
            ParserTestBase.EvaluateResults(TestContext,
                                           builder.Results,
                                           new MarkupBlock(
                                            new MarkupSpan("foo "),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(" baz")
                                           ));
        }

        [TestMethod]
        public void ParseMethodSetsUpRunWithSpecifiedCodeParserMarkupParserAndListenerAndPassesToMarkupParser() {
            RunParseWithListenerTest((parser, reader, listener) => parser.Parse(reader, listener));
        }

        private static void RunParseWithListenerTest(Action<RazorParser, TextReader, ParserVisitor> parserAction) {
            // Arrange
            MarkupParser markupParser = new MockMarkupParser();
            ParserBase codeParser = new CSharpCodeParser();
            RazorParser parser = new RazorParser(codeParser, markupParser);
            TextReader expectedReader = new StringReader("foo");
            ParserVisitor expectedListener = new Mock<ParserVisitor>().Object;

            // Act
            parserAction(parser, expectedReader, expectedListener);

            // Assert
            ParserContext actualContext = markupParser.Context;
            Assert.IsNotNull(actualContext);
            Assert.AreSame(markupParser, actualContext.MarkupParser);
            Assert.AreSame(markupParser, actualContext.ActiveParser);
            Assert.AreSame(codeParser, actualContext.CodeParser);
            Assert.AreSame(expectedReader, ((BufferingTextReader)actualContext.Source).InnerReader);
            Assert.AreSame(expectedListener, actualContext.Visitor);
        }

        private class MockMarkupParser : MarkupParser {
            public override void ParseDocument() {
            }

            public override void ParseSection(Tuple<string, string> nestingSequences, bool caseSensitive = true) {
            }

            public override void ParseBlock() {
            }

            public override bool IsEndTag() {
                return false;
            }

            public override bool IsStartTag() {
                return false;
            }
        }
    }
}

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class HtmlDocumentTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseDocumentMethodThrowsArgNullExceptionOnNullContext() {
            // Arrange
            HtmlMarkupParser parser = new HtmlMarkupParser();

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => parser.ParseDocument(), RazorResources.Parser_Context_Not_Set);
        }

        [TestMethod]
        public void ParseSectionMethodThrowsArgNullExceptionOnNullContext() {
            // Arrange
            HtmlMarkupParser parser = new HtmlMarkupParser();

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => parser.ParseSection(null, true), RazorResources.Parser_Context_Not_Set);
        }

        [TestMethod]
        public void ParseDocumentOutputsEmptyBlockWithEmptyMarkupSpanIfContentIsEmptyString() {
            ParseDocumentTest(String.Empty, new MarkupBlock(new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ParseDocumentOutputsWhitespaceOnlyContentAsSingleWhitespaceMarkupSpan() {
            SingleSpanDocumentTest("          ", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentAcceptsSwapTokenAtEndOfFileAndOutputsZeroLengthCodeSpan() {
            ParseDocumentTest("@",
                                new MarkupBlock(
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan(String.Empty,
                                                                   CSharpCodeParser.DefaultKeywords,
                                                                   acceptTrailingDot: false,
                                                                   acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(RazorResources.ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock, new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseDocumentWithinSectionDoesNotCreateDocumentLevelSpan() {
            ParseDocumentTest(@"@section Foo {
    <html></html>
}",
  new MarkupBlock(
      new SectionBlock(
        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
        new SectionHeaderSpan("section Foo {", sectionName: "Foo", acceptedCharacters: AcceptedCharacters.Any),
            new MarkupBlock(
                new MarkupSpan(@"
    <html></html>
")
            ),
            new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
      ),
      new MarkupSpan(String.Empty)
  )
);
        }

        [TestMethod]
        public void ParseDocumentParsesWholeContentAsOneSpanIfNoSwapCharacterEncountered() {
            SingleSpanDocumentTest("foo <bar>baz</bar>", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentHandsParsingOverToCodeParserWhenAtSignEncounteredAndEmitsOutput() {
            ParseDocumentTest("foo @bar baz",
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
        public void ParseDocumentEmitsAtSignAsMarkupIfAtEndOfFile() {
            ParseDocumentTest("foo @",
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan(String.Empty,
                                                                   CSharpCodeParser.DefaultKeywords,
                                                                   acceptTrailingDot: false,
                                                                   acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(RazorResources.ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock, new SourceLocation(5, 0, 5)));
        }

        [TestMethod]
        public void ParseDocumentEmitsCodeBlockIfFirstCharacterIsSwapCharacter() {
            ParseDocumentTest("@bar",
                                new MarkupBlock(
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseDocumentDoesNotSwitchToCodeOnEmailAddressInText() {
            SingleSpanDocumentTest("<foo>anurse@microsoft.com</foo>", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentDoesNotSwitchToCodeOnEmailAddressInAttribute() {
            SingleSpanDocumentTest("<a href=\"mailto:anurse@microsoft.com\">Email me</a>", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentDoesNotReturnErrorOnMismatchedTags() {
            SingleSpanDocumentTest("Foo <div><p></p></p> Baz", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentReturnsOneMarkupSegmentIfNoCodeBlocksEncountered() {
            SingleSpanDocumentTest("Foo <p>Baz<!--Foo-->Bar<!-F> Qux", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentRendersTextPseudoTagAsMarkup() {
            SingleSpanDocumentTest("Foo <text>Foo</text>", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentAcceptsEndTagWithNoMatchingStartTag() {
            SingleSpanDocumentTest("Foo </div> Bar", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseDocumentNoLongerSupportsDollarOpenBraceCombination() {
            ParseDocumentTest("<foo>${bar}</foo>",
                                new MarkupBlock(
                                    new MarkupSpan("<foo>${bar}</foo>")
                                ));
        }

        [TestMethod]
        public void ParseDocumentTreatsTwoAtSignsAsEscapeSequence() {
            HtmlParserTestUtils.RunSingleAtEscapeTest(ParseDocumentTest, lastSpanAcceptedCharacters: AcceptedCharacters.Any);
        }

        [TestMethod]
        public void ParseDocumentTreatsPairsOfAtSignsAsEscapeSequence() {
            HtmlParserTestUtils.RunMultiAtEscapeTest(ParseDocumentTest, lastSpanAcceptedCharacters: AcceptedCharacters.Any);
        }
    }
}

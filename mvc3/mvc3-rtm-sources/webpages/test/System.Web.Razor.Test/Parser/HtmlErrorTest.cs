using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class HtmlErrorTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseBlockThrowsErrorIfTagHasNoName() {
            SingleSpanBlockTest("<>Foo", "<", BlockType.Markup, SpanKind.Markup, AcceptedCharacters.Any,
                                new RazorError(RazorResources.ParseError_OuterTagMissingName, new SourceLocation(1,0,1)));
        }

        [TestMethod]
        public void ParseBlockAllowsInvalidTagNamesAsLongAsParserCanIdentifyEndTag() {
            SingleSpanBlockTest("<1-foo+bar baz>foo</1-foo+bar>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockThrowsErrorIfStartTextTagContainsTextAfterName() {
            ParseBlockTest("<text foo bar></text>",
                            new MarkupBlock(
                                new TransitionSpan("<text"),
                                new MarkupSpan(" foo bar>"),
                                new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(RazorResources.ParseError_TextTagCannotContainAttributes, SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockThrowsErrorIfEndTextTagContainsTextAfterName() {
            ParseBlockTest("<text></text foo bar>",
                            new MarkupBlock(
                                new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new TransitionSpan("</text"),
                                new MarkupSpan(" ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(RazorResources.ParseError_TextTagCannotContainAttributes, new SourceLocation(6,0,6)));
        }

        [TestMethod]
        public void ParseBlockThrowsExceptionIfBlockDoesNotStartWithTag() {
            ParseBlockTest("foo bar <baz>",
                            new MarkupBlock(),
                            new RazorError(RazorResources.ParseError_MarkupBlock_Must_Start_With_Tag, SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockStartingWithEndTagProducesRazorErrorThenOutputsMarkupSegmentAndEndsBlock() {
            ParseBlockTest("</foo> bar baz",
                            new MarkupBlock(
                                new MarkupSpan("</foo> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_UnexpectedEndTag, "foo"), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockWithUnclosedTopLevelTagThrowsMissingEndTagParserExceptionOnOutermostUnclosedTag() {
            ParseBlockTest("<p><foo></bar>",
                            new MarkupBlock(
                                new MarkupSpan("<p><foo></bar>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "p"), new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockWithUnclosedTagAtEOFThrowsMissingEndTagException() {
            ParseBlockTest("<foo>blah blah blah blah blah",
                            new MarkupBlock(
                                new MarkupSpan("<foo>blah blah blah blah blah")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "foo"), new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockWithUnfinishedTagAtEOFThrowsIncompleteTagException() {
            ParseBlockTest("<foo bar=baz",
                            new MarkupBlock(
                                new MarkupSpan("<foo bar=baz")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_UnfinishedTag, "foo"), new SourceLocation(0, 0, 0)));
        }
    }
}

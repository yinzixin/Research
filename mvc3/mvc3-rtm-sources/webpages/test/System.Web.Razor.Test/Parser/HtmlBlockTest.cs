using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class HtmlBlockTest : CsHtmlMarkupParserTestBase {
        private static readonly TestFile Nested1000 = TestFile.Create("nested-1000.html");

        [TestMethod]
        public void ParseBlockMethodThrowsArgNullExceptionOnNullContext() {
            // Arrange
            HtmlMarkupParser parser = new HtmlMarkupParser();

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => parser.ParseBlock(), RazorResources.Parser_Context_Not_Set);
        }
        
        [TestMethod]
        public void ParseBlockAcceptsTrailingWhitespaceAndNewlineIfBlockCanStillGrow() {
            ParseBlockTest(@"<                      
   ",
                           new MarkupBlock(
                               new MarkupSpan(@"<                      
")),
                           designTimeParser: true,
                           expectedErrors: new RazorError(RazorResources.ParseError_OuterTagMissingName, 
                                                           new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseBlockAllowsStartAndEndTagsToDifferInCase() {
            SingleSpanBlockTest("<li><p>Foo</P></lI>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockReadsToEndOfLineIfFirstCharacterAfterTransitionIsColon() {
            ParseBlockTest(@"@:<li>Foo Bar Baz
bork",
                            new MarkupBlock(
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MetaCodeSpan(":"),
                                new SingleLineMarkupSpan(@"<li>Foo Bar Baz
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockStopsParsingSingleLineBlockAtEOFIfNoEOLReached() {
            ParseBlockTest("@:foo bar",
                            new MarkupBlock(
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MetaCodeSpan(":"),
                                new SingleLineMarkupSpan(@"foo bar")
                            ));
        }

        [TestMethod]
        public void ParseBlockTreatsTwoAtSignsAsEscapeSequence() {
            HtmlParserTestUtils.RunSingleAtEscapeTest(ParseBlockTest);
        }

        [TestMethod]
        public void ParseBlockTreatsPairsOfAtSignsAsEscapeSequence() {
            HtmlParserTestUtils.RunMultiAtEscapeTest(ParseBlockTest);
        }

        [TestMethod]
        public void ParseBlockParsesUntilMatchingEndTagIfFirstNonWhitespaceCharacterIsStartTag() {
            SingleSpanBlockTest("<baz><boz><biz></biz></boz></baz>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAllowsUnclosedTagsAsLongAsItCanRecoverToAnExpectedEndTag() {
            SingleSpanBlockTest("<foo><bar><baz></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockWithSelfClosingTagJustEmitsTag() {
            SingleSpanBlockTest("<foo />", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockCanHandleSelfClosingTagsWithinBlock() {
            SingleSpanBlockTest("<foo><bar /></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsTagsWithAttributes() {
            SingleSpanBlockTest("<foo bar=\"baz\"><biz><boz zoop=zork/></biz></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAllowsCloseAngleBracketInAttributeValueIfDoubleQuoted() {
            SingleSpanBlockTest("<foo><bar baz=\">\" /></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAllowsCloseAngleBracketInAttributeValueIfSingleQuoted() {
            SingleSpanBlockTest("<foo><bar baz=\'>\' /></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAllowsSlashInAttributeValueIfDoubleQuoted() {
            SingleSpanBlockTest("<foo><bar baz=\"/\"></bar></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAllowsSlashInAttributeValueIfSingleQuoted() {
            SingleSpanBlockTest("<foo><bar baz=\'/\'></bar></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTerminatesAtEOF() {
            SingleSpanBlockTest("<foo>", "<foo>", BlockType.Markup, SpanKind.Markup,
                                         new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "foo"), new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockSupportsCommentAsBlock() {
            SingleSpanBlockTest("<!-- foo -->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsCommentWithinBlock() {
            SingleSpanBlockTest("<foo>bar<!-- zoop -->baz</foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockProperlyBalancesCommentStartAndEndTags() {
            SingleSpanBlockTest("<!--<foo></bar>-->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTerminatesAtEOFWhenParsingComment() {
            SingleSpanBlockTest("<!--<foo>", "<!--<foo>", BlockType.Markup, SpanKind.Markup);
        }

        [TestMethod]
        public void ParseBlockOnlyTerminatesCommentOnFullEndSequence() {
            SingleSpanBlockTest("<!--<foo>--</bar>-->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTerminatesCommentAtFirstOccurrenceOfEndSequence() {
            SingleSpanBlockTest("<foo><!--<foo></bar-->--></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTreatsMalformedTagsAsContent() {
            SingleSpanBlockTest("<foo></!-- bar --></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockParsesSGMLDeclarationAsEmptyTag() {
            SingleSpanBlockTest("<foo><!DOCTYPE foo bar baz></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTerminatesSGMLDeclarationAtFirstCloseAngle() {
            SingleSpanBlockTest("<foo><!DOCTYPE foo bar> baz></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockParsesXMLProcessingInstructionAsEmptyTag() {
            SingleSpanBlockTest("<foo><?xml foo bar baz?></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockTerminatesXMLProcessingInstructionAtQuestionMarkCloseAnglePair() {
            SingleSpanBlockTest("<foo><?xml foo bar?> baz</foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockDoesNotTerminateXMLProcessingInstructionAtCloseAngleUnlessPreceededByQuestionMark() {
            SingleSpanBlockTest("<foo><?xml foo bar> baz?></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsScriptTagsWithLessThanSignsInThem() {
            SingleSpanBlockTest(@"<script>if(foo<bar) { alert(""baz""); }</script>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsScriptTagsWithSpacedLessThanSignsInThem() {
            SingleSpanBlockTest(@"<script>if(foo < bar) { alert(""baz""); }</script>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAcceptsEmptyTextTag() {
            ParseBlockTest("<text/>",
                            new MarkupBlock(
                               new TransitionSpan("<text/>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockAcceptsTextTagAsOuterTagButDoesNotRender() {
            ParseBlockTest("<text>Foo Bar <foo> Baz</text> zoop",
                            new MarkupBlock(
                               new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new MarkupSpan("Foo Bar <foo> Baz"),
                               new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new MarkupSpan(" ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockRendersLiteralTextTagIfDoubled() {
            ParseBlockTest("<text><text>Foo Bar <foo> Baz</text></text> zoop",
                           new MarkupBlock(
                                new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MarkupSpan("<text>Foo Bar <foo> Baz</text>"),
                                new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MarkupSpan(" ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                           ));
        }

        [TestMethod]
        public void ParseBlockDoesNotConsiderPsuedoTagWithinMarkupBlock() {
            SingleSpanBlockTest("<foo><text><bar></bar></foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockStopsParsingMidEmptyTagIfEOFReached() {
            SingleSpanBlockTest("<br/", "<br/", BlockType.Markup, SpanKind.Markup,
                                new RazorError(String.Format(RazorResources.ParseError_UnfinishedTag, "br"), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockCanParse1000NestedElements() {
            string content = Nested1000.ReadAllText();
            SingleSpanDocumentTest(content, BlockType.Markup, SpanKind.Markup);
        }
    }
}

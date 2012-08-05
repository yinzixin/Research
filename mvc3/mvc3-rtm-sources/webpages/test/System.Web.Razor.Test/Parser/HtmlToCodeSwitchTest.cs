using System.Web.Razor.Parser;
using System.Web.Razor.Test.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class HtmlToCodeSwitchTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseBlockSwitchesToCodeWhenSwapCharacterEncounteredMidTag() {
            ParseBlockTest("<foo @bar />",
                            new MarkupBlock(
                                new MarkupSpan("<foo "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan(" />", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSwitchesToCodeWhenSwapCharacterEncounteredInAttributeValue() {
            ParseBlockTest(@"<foo bar=""@baz"" />",
                            new MarkupBlock(
                                new MarkupSpan(@"<foo bar="""),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("baz", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan(@""" />", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSwitchesToCodeWhenSwapCharacterEncounteredInTagContent() {
            ParseBlockTest("<foo>@bar<baz>@boz</baz></foo>",
                            new MarkupBlock(
                                new MarkupSpan("<foo>"),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan("<baz>"),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("boz", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan("</baz></foo>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockParsesCodeWithinSingleLineMarkup() {
            ParseBlockTest(@"@:<li>Foo @Bar Baz
bork",
                            new MarkupBlock(
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MetaCodeSpan(":"),
                                new SingleLineMarkupSpan("<li>Foo "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("Bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new SingleLineMarkupSpan(@" Baz
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsCodeWithinComment() {
            ParseBlockTest("<foo><!-- @foo --></foo>",
                            new MarkupBlock(
                                new MarkupSpan("<foo><!-- "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan(" --></foo>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsCodeWithinSGMLDeclaration() {
            ParseBlockTest("<foo><!DOCTYPE foo @bar baz></foo>",
                            new MarkupBlock(
                                new MarkupSpan("<foo><!DOCTYPE foo "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan(" baz></foo>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsCodeWithinXMLProcessingInstruction() {
            ParseBlockTest("<foo><?xml foo @bar baz?></foo>",
                            new MarkupBlock(
                                new MarkupSpan("<foo><?xml foo "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan(" baz?></foo>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockDoesNotSwitchToCodeOnEmailAddressInText() {
            SingleSpanBlockTest("<foo>anurse@microsoft.com</foo>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockDoesNotSwitchToCodeOnEmailAddressInAttribute() {
            SingleSpanBlockTest("<a href=\"mailto:anurse@microsoft.com\">Email me</a>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

       [ TestMethod]
        public void ParseBlockGivesWhitespacePreceedingAtToCodeIfThereIsNoMarkupOnThatLine() {
            ParseBlockTest(@"   <ul>
    @foreach(var p in Products) {
        <li>Product: @p.Name</li>
    }
    </ul>",
                            new MarkupBlock(
                                new MarkupSpan(@"   <ul>
"),
                                new StatementBlock(
                                    new CodeSpan("    "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CodeSpan(@"foreach(var p in Products) {
"),
                                    new MarkupBlock(
                                        new MarkupSpan(@"        <li>Product: "),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan("p.Name", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new MarkupSpan(@"</li>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new CodeSpan(@"    }
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupSpan(@"    </ul>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void CSharpCodeParserDoesNotAcceptLeadingOrTrailingWhitespaceInDesignMode() {
            ParseBlockTest(@"   <ul>
    @foreach(var p in Products) {
        <li>Product: @p.Name</li>
    }
    </ul>",
                            new MarkupBlock(
                                new MarkupSpan(@"   <ul>
    "),
                                new StatementBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CodeSpan(@"foreach(var p in Products) {
        "),
                                    new MarkupBlock(
                                        new MarkupSpan(@"<li>Product: "),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan("p.Name", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new MarkupSpan(@"</li>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new CodeSpan(@"
    }", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupSpan(@"
    </ul>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ), designTimeParser: true);
        }
    }
}

using System.Web.Razor.Test.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Text;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBHtmlDocumentTest : VBHtmlMarkupParserTestBase {
        [TestMethod]
        public void BlockCommentInMarkupDocumentIsHandledCorrectly() {
            ParseDocumentTest(@"<ul>
                @* This is a block comment </ul> *@ foo",
                new MarkupBlock(
                    new MarkupSpan(@"<ul>
                "),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan(" This is a block comment </ul> "),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(" foo")
                ));
        }

        [TestMethod]
        public void BlockCommentInMarkupBlockIsHandledCorrectly() {
            ParseBlockTest(@"<ul>
                @* This is a block comment </ul> *@ foo </ul>",
                new MarkupBlock(
                    new MarkupSpan(@"<ul>
                "),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan(" This is a block comment </ul> "),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(" foo </ul>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void BlockCommentAtStatementStartInCodeBlockIsHandledCorrectly() {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    @* User is logged in! End If *@
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If Request.IsAuthenticated Then
    "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan(" User is logged in! End If "),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(@"
    Write(""Hello friend!"")
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInStatementInCodeBlockIsHandledCorrectly() {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = @* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If Request.IsAuthenticated Then
    Dim foo = "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan(" User is logged in! End If "),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(@" bar
    Write(""Hello friend!"")
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInStringInCodeBlockIsIgnored() {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = ""@* User is logged in! End If *@ bar""
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If Request.IsAuthenticated Then
    Dim foo = ""@* User is logged in! End If *@ bar""
    Write(""Hello friend!"")
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInTickCommentInCodeBlockIsIgnored() {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = '@* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If Request.IsAuthenticated Then
    Dim foo = '@* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInRemCommentInCodeBlockIsIgnored() {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = REM @* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If Request.IsAuthenticated Then
    Dim foo = REM @* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.Foo@*bar*@",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html.Foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan(String.Empty),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan("bar"),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentAfterDotOfImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.@*bar*@",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan("."),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan("bar"),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInParensOfImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.Foo(@*bar*@ 4)",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html.Foo(", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new ImplicitExpressionSpan(" 4)", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInConditionIsHandledCorrectly() {
            ParseDocumentTest(@"@If @*bar*@ Then End If",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"If "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(" Then End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInExplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@(1 + @*bar*@ 1)",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"1 + "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(" 1"),
                        new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }
    }
}

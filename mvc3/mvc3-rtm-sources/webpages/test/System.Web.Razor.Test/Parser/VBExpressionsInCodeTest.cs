using System.Web.Razor.Test.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBExpressionsInCodeTest : VBHtmlCodeParserTestBase {
        [TestMethod]
        public void InnerImplicitExpressionWithOnlySingleAtAcceptsSingleSpaceOrNewlineAtDesignTime() {
            ParseBlockTest(@"Code
    @
End Code",
                           new StatementBlock(
                               new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(@"
    "),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, 
                                                              CSharpCodeParser.DefaultKeywords, 
                                                              acceptTrailingDot: true,
                                                              acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                               ),
                               new CodeSpan(@"
"),
                               new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           designTimeParser: true);
        }

        [TestMethod]
        public void InnerImplicitExpressionDoesNotAcceptDotAfterAt() {
            ParseBlockTest(@"Code
    @.
End Code",
                           new StatementBlock(
                               new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(@"
    "),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                               ),
                               new CodeSpan(@".
"),
                               new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           designTimeParser: true);
        }

        [TestMethod]
        public void ParserAcceptsTrailingDotInImplicitExpressionInCodeBlock() {
            RunInCodeImplicitExpressionTest("Foo.Bar.");
        }

        [TestMethod]
        public void ParserAcceptsSimpleImplicitExpressionInCodeBlock() {
            RunInCodeImplicitExpressionTest("Foo");
        }

        [TestMethod]
        public void ParserAcceptsImplicitExpressionWithDotsInCodeBlock() {
            RunInCodeImplicitExpressionTest("Foo.Bar.Baz");
        }

        [TestMethod]
        public void ParserAcceptsImplicitExpressionWithParensInCodeBlock() {
            RunInCodeImplicitExpressionTest("Foo().Bar().Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithStuffInParensInCodeBlock() {
            RunInCodeImplicitExpressionTest("Foo().Bar(sdfkhj sdfksdfjs \")\" sjdfkjsdf).Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithCommentInParensInCodeBlock() {
            RunInCodeImplicitExpressionTest(@"Foo().Bar(sdfkhj sdfksdfjs "")"" '))))))))
sjdfkjsdf).Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleExplicitExpressionInCodeBlock() {
            RunInCodeExplicitExpressionTest("Foo");
        }

        [TestMethod]
        public void ParseBlockSupportsExplicitExpressionWithMethodCallsInCodeBlock() {
            RunInCodeExplicitExpressionTest("Foo(Of String).Bar(1, 2, 3).Biz");
        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInStringsInCodeBlock() {
            RunInCodeExplicitExpressionTest("Foo(Of String).Bar(\")\").Biz");
        }

        [TestMethod]
        public void ParseBlockCorrectlyTerminatesStringsWithEscapedQuotesInCodeBlock() {
            RunInCodeExplicitExpressionTest("Foo(Of String).Bar(\"Foo\"\"Bar)\"\"Baz\").Biz");
        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInREMCommentInCodeBlock() {
            RunInCodeExplicitExpressionTest(@"Foo.Bar. _
REM )
Baz()
");
        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInTickComment() {
            RunInCodeExplicitExpressionTest(@"Foo.Bar. _
' )
Baz()
");
        }

        private void RunInCodeExpressionTest(string expression, ExpressionBlock expressionBlock) {
            ParseBlockTest(@"If foo IsNot Nothing Then
    @" + expression + @"
End If",
                            new StatementBlock(
                                new CodeSpan(@"If foo IsNot Nothing Then
    "),
                                expressionBlock,
                                new CodeSpan(@"
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        private void RunInCodeExplicitExpressionTest(string expression) {
            RunInCodeExpressionTest("(" + expression + ")", new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(expression),
                                        new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ));
        }

        private void RunInCodeImplicitExpressionTest(string expression) {
            RunInCodeExpressionTest(expression, new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan(expression, VBCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ));
        }
    }
}

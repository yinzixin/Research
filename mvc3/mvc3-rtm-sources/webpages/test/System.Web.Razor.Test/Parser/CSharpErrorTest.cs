using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpErrorTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockCapturesWhitespaceToEndOfLineInInvalidUsingStatementAndTreatsAsFileCode() {
            ParseBlockTest(@"using          

",
                            new StatementBlock(
                                new CodeSpan(@"using          
")
                            ));
        }

        [TestMethod]
        public void ParseBlockMethodOutputsOpenCurlyAsCodeSpanIfEofFoundAfterOpenCurlyBrace() {
            ParseBlockTest("{",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty) { AutoCompleteString = "}" }
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                          RazorResources.BlockName_Code,
                                                          "}", "{"),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockMethodOutputsZeroLengthCodeSpanIfStatementBlockEmpty() {
            ParseBlockTest("{}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockMethodDisplaysErrorIfWhitespaceFollowsTransition() {
            ParseBlockTest(@"
",
                           new ExpressionBlock(
                               new ImplicitExpressionSpan(String.Empty,
                                                          CSharpCodeParser.DefaultKeywords,
                                                          acceptTrailingDot: false,
                                                          acceptedCharacters: AcceptedCharacters.NonWhiteSpace)), 
                           new RazorError(RazorResources.ParseError_Unexpected_WhiteSpace_At_Start_Of_CodeBlock_CS, SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockMethodParsesNothingIfFirstCharacterIsNotIdentifierStartOrParenOrBrace() {
            ParseBlockTest("!!!", 
                           new ExpressionBlock(
                               new ImplicitExpressionSpan(String.Empty, 
                                                          CSharpCodeParser.DefaultKeywords, 
                                                          acceptTrailingDot: false, 
                                                          acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                           new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "!"),
                                           new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockShouldReportErrorAndTerminateAtEOFIfIfParenInExplicitExpressionUnclosed() {
            ParseBlockTest(@"(foo bar
baz",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"foo bar
baz")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                          RazorResources.BlockName_ExplicitExpression, ')', '('),
                                            new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockShouldReportErrorAndTerminateAtMarkupIfIfParenInExplicitExpressionUnclosed() {
            ParseBlockTest(@"(foo bar
<html>
baz
</html",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"foo bar
")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                          RazorResources.BlockName_ExplicitExpression, ')', '('),
                                            new SourceLocation(0, 0, 0)));
        }

        [TestMethod]
        public void ParseBlockCorrectlyHandlesInCorrectTransitionsIfImplicitExpressionParensUnclosed() {
            ParseBlockTest(@"Href(
<h1>@Html.Foo(Bar);</h1>
",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Href(
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                          "(", ")"),
                                            new SourceLocation(4, 0, 4)));
        }

        [TestMethod]
        // Test for fix to Dev10 884975 - Incorrect Error Messaging
        public void ParseBlockShouldReportErrorAndTerminateAtEOFIfParenInImplicitExpressionUnclosed() {
            ParseBlockTest(@"Foo(Bar(Baz)
Biz
Boz",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo(Bar(Baz)
Biz
Boz", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                          "(", ")"),
                                            new SourceLocation(3, 0, 3)));
        }

        [TestMethod]
        // Test for fix to Dev10 884975 - Incorrect Error Messaging
        public void ParseBlockShouldReportErrorAndTerminateAtMarkupIfParenInImplicitExpressionUnclosed() {
            ParseBlockTest(@"Foo(Bar(Baz)
Biz
<html>
Boz
</html>",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo(Bar(Baz)
Biz
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                          "(", ")"),
                                            new SourceLocation(3, 0, 3)));
        }

        [TestMethod]
        // Test for fix to Dev10 884975 - Incorrect Error Messaging
        public void ParseBlockShouldReportErrorAndTerminateAtEOFIfBracketInImplicitExpressionUnclosed() {
            ParseBlockTest(@"Foo[Bar[Baz]
Biz
Boz",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo[Bar[Baz]
Biz
Boz", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                          "[", "]"),
                                            new SourceLocation(3, 0, 3)));
        }

        [TestMethod]
        // Test for fix to Dev10 884975 - Incorrect Error Messaging
        public void ParseBlockShouldReportErrorAndTerminateAtMarkupIfBracketInImplicitExpressionUnclosed() {
            ParseBlockTest(@"Foo[Bar[Baz]
Biz
<b>
Boz
</b>",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo[Bar[Baz]
Biz
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                          "[", "]"),
                                            new SourceLocation(3, 0, 3)));
        }

        // Simple EOF handling errors:
        [TestMethod]
        public void ParseBlockReportsErrorIfExplicitCodeBlockUnterminatedAtEOF() {
            ParseBlockTest("{ var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, RazorResources.BlockName_Code, '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfClassBlockUnterminatedAtEOF() {
            ParseBlockTest("functions { var foo = bar; if(foo != null) { bar(); } ",
                            new FunctionsBlock(
                                new MetaCodeSpan("functions {", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "functions", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfIfBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("if");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfElseBlockUnterminatedAtEOF() {
            ParseBlockTest("if(foo) { baz(); } else { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("if(foo) { baz(); } else { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "else", '}', '{'), new SourceLocation(19, 0, 19)));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfElseIfBlockUnterminatedAtEOF() {
            ParseBlockTest("if(foo) { baz(); } else if { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("if(foo) { baz(); } else if { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "else if", '}', '{'), new SourceLocation(19, 0, 19)));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfDoBlockUnterminatedAtEOF() {
            ParseBlockTest("do { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("do { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "do", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfTryBlockUnterminatedAtEOF() {
            ParseBlockTest("try { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("try { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "try", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfCatchBlockUnterminatedAtEOF() {
            ParseBlockTest("try { baz(); } catch(Foo) { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("try { baz(); } catch(Foo) { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "catch", '}', '{'), new SourceLocation(15, 0, 15)));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfFinallyBlockUnterminatedAtEOF() {
            ParseBlockTest("try { baz(); } finally { var foo = bar; if(foo != null) { bar(); } ",
                            new StatementBlock(
                                new CodeSpan("try { baz(); } finally { var foo = bar; if(foo != null) { bar(); } ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "finally", '}', '{'), new SourceLocation(15, 0, 15)));
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfForBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("for");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfForeachBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("foreach");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfWhileBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("while");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfSwitchBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("switch");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfLockBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("lock");
        }

        [TestMethod]
        public void ParseBlockReportsErrorIfUsingBlockUnterminatedAtEOF() {
            RunUnterminatedSimpleKeywordBlock("using");
        }

        [TestMethod]
        public void ParseBlockRequiresControlFlowStatementsToHaveBraces() {
            string expectedMessage = String.Format(RazorResources.ParseError_SingleLine_ControlFlowStatements_Not_Allowed, "{", "<");
            ParseBlockTest("if(foo) <p>Bar</p> else if(bar) <p>Baz</p> else <p>Boz</p>",
                            new StatementBlock(
                                new CodeSpan("if(foo)"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Bar</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("else if(bar)"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Baz</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("else"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Boz</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(String.Empty)
                            ),
                            new RazorError(expectedMessage, new SourceLocation(8, 0, 8)),
                            new RazorError(expectedMessage, new SourceLocation(32, 0, 32)),
                            new RazorError(expectedMessage, new SourceLocation(48, 0, 48)));
        }

        [TestMethod]
        public void ParseBlockIncludesUnexpectedCharacterInSingleStatementControlFlowStatementError() {
            ParseBlockTest("if(foo)) { var bar = foo; }",
                            new StatementBlock(
                                new CodeSpan("if(foo)) { var bar = foo; }")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_SingleLine_ControlFlowStatements_Not_Allowed, "{", ")"), new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockOutputsErrorIfAtSignFollowedByLessThanSignAtStatementStart() {
            ParseBlockTest("if(foo) { @<p>Bar</p> }",
                            new StatementBlock(
                                new CodeSpan("if(foo) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan("<p>Bar</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}")
                            ),
                            new RazorError(RazorResources.ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start, new SourceLocation(11, 0, 11)));
        }

        [TestMethod]
        public void ParseBlockTerminatesIfBlockAtEOLWhenRecoveringFromMissingCloseParen() {
            ParseBlockTest(@"if(foo bar
baz",
                            new StatementBlock(
                                new CodeSpan(@"if(foo bar
")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(2, 0, 2)));
        }

        [TestMethod]
        public void ParseBlockTerminatesForeachBlockAtEOLWhenRecoveringFromMissingCloseParen() {
            ParseBlockTest(@"foreach(foo bar
baz",
                           new StatementBlock(
                               new CodeSpan(@"foreach(foo bar
")
                           ),
                           new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockTerminatesWhileClauseInDoStatementAtEOLWhenRecoveringFromMissingCloseParen() {
            ParseBlockTest(@"do { } while(foo bar
baz",
                           new StatementBlock(
                               new CodeSpan(@"do { } while(foo bar
")
                           ),
                           new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(12, 0, 12)));
        }

        [TestMethod]
        public void ParseBlockTerminatesUsingBlockAtEOLWhenRecoveringFromMissingCloseParen() {
            ParseBlockTest(@"using(foo bar
baz",
                            new StatementBlock(
                                new CodeSpan(@"using(foo bar
")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(5, 0, 5)));
        }

        [TestMethod]
        public void ParseBlockResumesIfStatementAfterOpenParen() {
            ParseBlockTest(@"if(
else { <p>Foo</p> }",
                            new StatementBlock(
                                new CodeSpan(@"if(
else {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Foo</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(2, 0, 2)));
        }

        [TestMethod]
        public void ParseBlockTerminatesNormalCSharpStringsAtEOLIfEndQuoteMissing() {
            SingleSpanBlockTest(@"if(foo) {
    var p = ""foo bar baz
;
}",
                                BlockType.Statement, SpanKind.Code,
                                new RazorError(RazorResources.ParseError_Unterminated_String_Literal, new SourceLocation(23, 1, 12)));
        }

        [TestMethod]
        public void ParseBlockTerminatesNormalStringAtEndOfFile() {
            SingleSpanBlockTest("if(foo) { var foo = \"blah blah blah blah blah", BlockType.Statement, SpanKind.Code,
                                new RazorError(RazorResources.ParseError_Unterminated_String_Literal, new SourceLocation(20, 0, 20)),
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "if", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockTerminatesVerbatimStringAtEndOfFile() {
            SingleSpanBlockTest(@"if(foo) { var foo = @""blah 
blah; 
<p>Foo</p>
blah 
blah",
                                BlockType.Statement, SpanKind.Code,
                                new RazorError(RazorResources.ParseError_Unterminated_String_Literal, new SourceLocation(21, 0, 21)),
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "if", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesMarkupIncorrectyAssumedToBeWithinAStatement() {
            ParseBlockTest(@"if(foo) {
    var foo = ""foo bar baz
    <p>Foo is @foo</p>
}",
                            new StatementBlock(
                                new CodeSpan(@"if(foo) {
    var foo = ""foo bar baz
    "),
                                new MarkupBlock(
                                    new MarkupSpan("<p>Foo is "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                                new MarkupSpan(@"</p>
", hidden:false, acceptedCharacters: AcceptedCharacters.None)),
                                new CodeSpan("}")
                            ), new RazorError(RazorResources.ParseError_Unterminated_String_Literal, new SourceLocation(25, 1, 14)));
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesAtSignInDelimitedBlock() {
            ParseBlockTest("(Request[\"description\"] ?? @photo.Description)",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("Request[\"description\"] ?? @photo.Description"),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        private void RunUnterminatedSimpleKeywordBlock(string keyword) {
            SingleSpanBlockTest(keyword + " (foo) { var foo = bar; if(foo != null) { bar(); } ", BlockType.Statement, SpanKind.Code,
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, keyword, '}', '{'), SourceLocation.Zero));
        }
    }
}

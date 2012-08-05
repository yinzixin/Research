using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBBlockTest : VBHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockMethodThrowsArgNullExceptionOnNullContext() {
            // Arrange
            VBCodeParser parser = new VBCodeParser();

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => parser.ParseBlock(), RazorResources.Parser_Context_Not_Set);
        }

        [TestMethod]
        public void ParseBlockAcceptsImplicitExpression() {
            ParseBlockTest(@"If True Then
    @foo
End If",
                            new StatementBlock(
                                new CodeSpan(@"If True Then
    "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo", VBCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new CodeSpan(@"
End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockAcceptsIfStatementWithinCodeBlockIfInDesignTimeMode() {
            ParseBlockTest(@"If True Then
    @If True Then
    End If
End If",
                            new StatementBlock(
                                new CodeSpan(@"If True Then
    "),
                                new StatementBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CodeSpan(@"If True Then
    End If
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"End If", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           new RazorError(String.Format(RazorResources.ParseError_Unexpected_Keyword_After_At, "If"),
                                          new SourceLocation(19, 1, 5)));
        }

        [TestMethod]
        public void ParseBlockSupportsSpacesInStrings() {
            ParseBlockTest(@"for each p in db.Query(""SELECT * FROM PRODUCTS"")
    @<p>@p.Name</p>
next",
                            new StatementBlock(
                                new CodeSpan(@"for each p in db.Query(""SELECT * FROM PRODUCTS"")
"),
                                new MarkupBlock(
                                    new MarkupSpan("    "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan("<p>"),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("p.Name", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(@"</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                new CodeSpan("next", hidden: false, acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleCodeBlock() {
            ParseBlockTest(@"Code
    If foo IsNot Nothing
        Bar(foo)
    End If
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If foo IsNot Nothing
        Bar(foo)
    End If
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockRejectsNewlineBetweenEndAndCodeIfNotPrefixedWithUnderscore() {
            ParseBlockTest(@"Code
    If foo IsNot Nothing
        Bar(foo)
    End If
End
Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If foo IsNot Nothing
        Bar(foo)
    End If
End
Code")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Code", "End Code"),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockAcceptsNewlineBetweenEndAndCodeIfPrefixedWithUnderscore() {
            ParseBlockTest(@"Code
    If foo IsNot Nothing
        Bar(foo)
    End If
End _
_
 _
Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If foo IsNot Nothing
        Bar(foo)
    End If
"),
                                new MetaCodeSpan(@"End _
_
 _
Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleFunctionsBlock() {
            ParseBlockTest(@"Functions
    Public Sub Foo()
        Bar()
    End Sub

    Private Function Bar() As Object
        Return Nothing
    End Function
End Functions",
                            new FunctionsBlock(
                                new MetaCodeSpan("Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    Public Sub Foo()
        Bar()
    End Sub

    Private Function Bar() As Object
        Return Nothing
    End Function
"),
                                new MetaCodeSpan("End Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockRejectsNewlineBetweenEndAndFunctionsIfNotPrefixedWithUnderscore() {
            ParseBlockTest(@"Functions
    If foo IsNot Nothing
        Bar(foo)
    End If
End
Functions",
                            new FunctionsBlock(
                                new MetaCodeSpan("Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If foo IsNot Nothing
        Bar(foo)
    End If
End
Functions")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Functions", "End Functions"),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockAcceptsNewlineBetweenEndAndFunctionsIfPrefixedWithUnderscore() {
            ParseBlockTest(@"Functions
    If foo IsNot Nothing
        Bar(foo)
    End If
End _
_
 _
Functions",
                            new FunctionsBlock(
                                new MetaCodeSpan("Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If foo IsNot Nothing
        Bar(foo)
    End If
"),
                                new MetaCodeSpan(@"End _
_
 _
Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockCorrectlyHandlesExtraEndsInEndCode() {
            ParseBlockTest(@"Code
    Bar End
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    Bar End
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockCorrectlyHandlesExtraEndsInEndFunctions() {
            ParseBlockTest(@"Functions
    Bar End
End Functions",
                            new FunctionsBlock(
                                new MetaCodeSpan("Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    Bar End
"),
                                new MetaCodeSpan("End Functions", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsExitWithinWhileBlock() {
            RunKeywordWithExitOrContinueTest("While True", "Exit While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsExitWithinDoBlock() {
            RunKeywordWithExitOrContinueTest("Do", "Exit Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsExitWithinForBlock() {
            RunKeywordWithExitOrContinueTest("For Each p in Products", "Exit For", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsContinueWithinWhileBlock() {
            RunKeywordWithExitOrContinueTest("While True", "Continue While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsContinueWithinDoBlock() {
            RunKeywordWithExitOrContinueTest("Do", "Continue Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsContinueWithinForBlock() {
            RunKeywordWithExitOrContinueTest("For Each p in Products", "Continue For", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleWhileBlock() {
            RunSimpleKeywordBlockTest("While True", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedWhileBlock() {
            RunNestedKeywordBlockTest("While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleDoBlock() {
            RunSimpleKeywordBlockTest("Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsNestedDoBlock() {
            RunNestedKeywordBlockTest("Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleIfBlock() {
            RunSimpleKeywordBlockTest("If foo IsNot Nothing", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedIfBlock() {
            RunNestedKeywordBlockTest("If", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleSelectCaseBlock() {
            RunSimpleKeywordBlockTest("Select Case foo", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedSelectCaseBlock() {
            RunNestedKeywordBlockTest("Select", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleForBlock() {
            RunSimpleKeywordBlockTest("For i as Integer = 1 to 10 Step 2", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsNestedForBlock() {
            RunNestedKeywordBlockTest("For", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleForEachBlock() {
            RunSimpleKeywordBlockTest("For Each p in Products", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleTryBlock() {
            RunSimpleKeywordBlockTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedTryBlock() {
            RunNestedKeywordBlockTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleWithBlock() {
            RunSimpleKeywordBlockTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedWithBlock() {
            RunNestedKeywordBlockTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleSyncLockBlock() {
            RunSimpleKeywordBlockTest("SyncLock", "End SyncLock");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedSyncLockBlock() {
            RunNestedKeywordBlockTest("SyncLock", "End SyncLock");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleUsingBlock() {
            RunSimpleKeywordBlockTest("Using", "End Using");
        }

        [TestMethod]
        public void ParseBlockSupportsNestedUsingBlock() {
            RunNestedKeywordBlockTest("Using", "End Using");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInWhileBlock() {
            RunCommentedEndSequenceTest("While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInDoBlock() {
            RunCommentedEndSequenceTest("Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInIfBlock() {
            RunCommentedEndSequenceTest("If", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInSelectCaseBlock() {
            RunCommentedEndSequenceTest("Select", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInForBlock() {
            RunCommentedEndSequenceTest("For", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInTryBlock() {
            RunCommentedEndSequenceTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInWithBlock() {
            RunCommentedEndSequenceTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsCommentedEndSequenceInUsingBlock() {
            RunCommentedEndSequenceTest("Using", "End Using");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInWhileBlock() {
            RunEndSequenceInStringTest("While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInDoBlock() {
            RunEndSequenceInStringTest("Do", "Loop", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInIfBlock() {
            RunEndSequenceInStringTest("If", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInSelectCaseBlock() {
            RunEndSequenceInStringTest("Select", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInForBlock() {
            RunEndSequenceInStringTest("For", "Next", acceptToEndOfLine: true);
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInTryBlock() {
            RunEndSequenceInStringTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInWithBlock() {
            RunEndSequenceInStringTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsEndSequenceInStringInUsingBlock() {
            RunEndSequenceInStringTest("Using", "End Using");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForWhileBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("While", "EndWhile", "End While");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForIfBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("If", "EndIf", "End If");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForSelectCaseBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("Select", "EndSelect", "End Select");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForTryBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("Try", "EndTry", "End Try");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForWithBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("With", "EndWith", "End With");
        }

        [TestMethod]
        public void ParseBlockRequiresSpaceInEndSequenceForUsingBlock() {
            RunKeywordRequiresSpaceBetweenKeywordsTest("Using", "EndUsing", "End Using");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForWhileBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("While", "End", "While");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForIfBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("If", "End", "If");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForSelectCaseBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("Select", "End", "Select");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForTryBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("Try", "End", "Try");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForWithBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("With", "End", "With");
        }

        [TestMethod]
        public void ParseBlockAllowsNewlineInEndSequenceForUsingBlockIfPrefixedByUnderscore() {
            RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest("Using", "End", "Using");
        }

        private void RunKeywordAllowsNewlinesIfPrefixedByUnderscoreTest(string startKeyword, string endKeyword1, string endKeyword2, bool acceptToEndOfLine = false) {
            string code = startKeyword + @"
    ' In the block
" + endKeyword1 + @" _
_
_
_
_
_
  " + endKeyword2 + @"
";

            SingleSpanBlockTest(code + "foo bar baz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunKeywordRequiresSpaceBetweenKeywordsTest(string startKeyword, string wrongEndKeyword, string endKeyword, bool acceptToEndOfLine = false) {
            string code = startKeyword + @"
    ' This should not end the code
    " + wrongEndKeyword + @"
    ' But this should
" + endKeyword;
            SingleSpanBlockTest(code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunKeywordWithExitOrContinueTest(string startKeyword, string exitKeyword, string endKeyword, bool acceptToEndOfLine = false) {
            string code = startKeyword + @"
    ' This is before the exit
    " + exitKeyword + @"
    ' This is after the exit
" + endKeyword + @"
";
            SingleSpanBlockTest(code + "foo bar baz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunCommentedEndSequenceTest(string keyword, string endSequence, bool acceptToEndOfLine = false) {
            string code = keyword + @"
    '" + endSequence + @"
" + endSequence + (acceptToEndOfLine ? @" foo bar baz
" : @" 
");
            SingleSpanBlockTest(code + "biz boz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunEndSequenceInStringTest(string keyword, string endSequence, bool acceptToEndOfLine = false) {
            string code = keyword + @"
    """ + endSequence + @"""
" + endSequence + (acceptToEndOfLine ? @" foo bar baz
" : @"
");
            SingleSpanBlockTest(code + "biz boz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunNestedKeywordBlockTest(string keyword, string endSequence, bool acceptToEndOfLine = false) {
            string code = keyword + @"
    " + keyword + @"
        Bar(foo)
    " + endSequence + @"
" + endSequence + (acceptToEndOfLine ? @" foo bar baz
" : @" 
");
            SingleSpanBlockTest(code + "biz boz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private void RunSimpleKeywordBlockTest(string keyword, string endSequence, bool acceptToEndOfLine = false) {
            string code = keyword + @"
    Bar(foo)
" + endSequence + (acceptToEndOfLine ? @" foo bar baz
" : @" 
");
            SingleSpanBlockTest(code + "biz boz", code, BlockType.Statement, SpanKind.Code, acceptedCharacters: GetAcceptedCharacters(acceptToEndOfLine));
        }

        private AcceptedCharacters GetAcceptedCharacters(bool acceptToEndOfLine) {
            return acceptToEndOfLine ? AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace :
                                       AcceptedCharacters.None;
        }
    }
}

using System.Web.Razor.Test.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBToMarkupSwitchTest : VBHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockSwitchesToMarkupWhenAtSignFollowedByLessThanInStatementBlock() {
            ParseBlockTest(@"Code
    If True Then
        @<p>It's True!</p>
    End If
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    If True Then
"),
                                new MarkupBlock(
                                    new MarkupSpan("        "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(@"<p>It's True!</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    End If
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInWhileBlock() {
            RunSimpleMarkupSwitchTest("While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInIfBlock() {
            RunSimpleMarkupSwitchTest("If", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInSelectCaseBlock() {
            RunSimpleMarkupSwitchTest("Select", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInForBlock() {
            RunSimpleMarkupSwitchTest("For", "Next", acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace);
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInTryBlock() {
            RunSimpleMarkupSwitchTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInWithBlock() {
            RunSimpleMarkupSwitchTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInUsingBlock() {
            RunSimpleMarkupSwitchTest("Using", "End Using");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInWhileBlock() {
            RunSingleLineMarkupSwitchTest("While", "End While");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInIfBlock() {
            RunSingleLineMarkupSwitchTest("If", "End If");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInSelectCaseBlock() {
            RunSingleLineMarkupSwitchTest("Select", "End Select");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInForBlock() {
            RunSingleLineMarkupSwitchTest("For", "Next", acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace);
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInTryBlock() {
            RunSingleLineMarkupSwitchTest("Try", "End Try");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInWithBlock() {
            RunSingleLineMarkupSwitchTest("With", "End With");
        }

        [TestMethod]
        public void ParseBlockSupportsSingleLineMarkupInUsingBlock() {
            RunSingleLineMarkupSwitchTest("Using", "End Using");
        }

        private void RunSingleLineMarkupSwitchTest(string keyword, string endSequence, AcceptedCharacters acceptedCharacters = AcceptedCharacters.None) {
            ParseBlockTest(keyword + @"
    If True Then
        @:<p>It's True!</p>
        This is code!
    End If
" + endSequence,
                            new StatementBlock(
                                new CodeSpan(keyword + @"
    If True Then
"),
                                new MarkupBlock(
                                    new MarkupSpan("        "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"<p>It's True!</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        This is code!
    End If
" + endSequence, hidden: false, acceptedCharacters: acceptedCharacters)
                            ));
        }

        private void RunSimpleMarkupSwitchTest(string keyword, string endSequence, AcceptedCharacters acceptedCharacters = AcceptedCharacters.None) {
            ParseBlockTest(keyword + @"
    If True Then
        @<p>It's True!</p>
    End If
" + endSequence,
                            new StatementBlock(
                                new CodeSpan(keyword + @"
    If True Then
"),
                                new MarkupBlock(
                                    new MarkupSpan("        "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(@"<p>It's True!</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    End If
" + endSequence, hidden: false, acceptedCharacters: acceptedCharacters)
                            ));
        }
    }
}

using System.Collections.Generic;
using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBErrorTest : VBHtmlCodeParserTestBase {
        [TestMethod]
        public void ParserOutputsErrorAndRecoversToEndOfLineIfExplicitExpressionUnterminated() {
            ParseBlockTest(@"(foo
bar",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("foo")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                          RazorResources.BlockName_ExplicitExpression,
                                                          ")", "("),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParserOutputsErrorIfFunctionsBlockNotTerminated() {
            RunUnterminatedBlockTest("Functions", "End Functions", blockType: BlockType.Functions, keywordIsMetaCode: true);
        }

        [TestMethod]
        public void ParserOutputsErrorIfCodeBlockNotTerminated() {
            RunUnterminatedBlockTest("Code", "End Code", keywordIsMetaCode: true);
        }

        [TestMethod]
        public void ParserOutputsErrorIfDoBlockNotTerminated() {
            RunUnterminatedBlockTest("Do", "Loop");
        }

        [TestMethod]
        public void ParserOutputsErrorIfWhileBlockNotTerminated() {
            RunUnterminatedBlockTest("While", "End While");
        }

        [TestMethod]
        public void ParserOutputsErrorIfIfBlockNotTerminated() {
            RunUnterminatedBlockTest("If", "End If");
        }

        [TestMethod]
        public void ParserOutputsErrorIfSelectCaseBlockNotTerminated() {
            RunUnterminatedBlockTest("Select Case", "End Select");
        }

        [TestMethod]
        public void ParserOutputsErrorIfForBlockNotTerminated() {
            RunUnterminatedBlockTest("For", "Next");
        }

        [TestMethod]
        public void ParserOutputsErrorIfTryBlockNotTerminated() {
            RunUnterminatedBlockTest("Try", "End Try");
        }

        [TestMethod]
        public void ParserOutputsErrorIfWithBlockNotTerminated() {
            RunUnterminatedBlockTest("With", "End With");
        }

        [TestMethod]
        public void ParserOutputsErrorIfUsingBlockNotTerminated() {
            RunUnterminatedBlockTest("Using", "End Using");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterFunctions() {
            RunEofBlockTest("Functions", "End Functions", blockType: BlockType.Functions, keywordIsMetaCode: true);
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterCode() {
            RunEofBlockTest("Code", "End Code", keywordIsMetaCode: true);
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterDo() {
            RunEofBlockTest("Do", "Loop");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterWhile() {
            RunEofBlockTest("While", "End While");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterIf() {
            RunEofBlockTest("If", "End If");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterSelectCase() {
            RunEofBlockTest("Select Case", "End Select");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterFor() {
            RunEofBlockTest("For", "Next");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterTry() {
            RunEofBlockTest("Try", "End Try");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterWith() {
            RunEofBlockTest("With", "End With");
        }

        [TestMethod]
        public void ParserOutputsErrorIfEOFReachedImmediatelyAfterUsing() {
            RunEofBlockTest("Using", "End Using");
        }

        [TestMethod]
        public void ParserOutputsZeroLengthCodeSpanIfEofReachedAfterStartOfExplicitExpression() {
            ParseBlockTest("(",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "explicit expression", ")", "("),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParserOutputsZeroLengthCodeSpanIfEofReachedAfterAtSign() {
            SingleSpanBlockTest(String.Empty, BlockType.Expression, SpanKind.Code,
                                new RazorError(RazorResources.ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock, SourceLocation.Zero));
        }

        [TestMethod]
        public void ParserOutputsZeroLengthCodeSpanIfOnlyWhitespaceFoundAfterAtSign() {
            SingleSpanBlockTest(" ", String.Empty, BlockType.Expression, SpanKind.Code,
                                new RazorError(RazorResources.ParseError_Unexpected_WhiteSpace_At_Start_Of_CodeBlock_VB, SourceLocation.Zero));
        }

        [TestMethod]
        public void ParserOutputsZeroLengthCodeSpanIfInvalidCharacterFoundAfterAtSign() {
            SingleSpanBlockTest("!!!", String.Empty, BlockType.Expression, SpanKind.Code,
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_VB, "!"),
                                                SourceLocation.Zero));
        }

        private void RunEofBlockTest(string keyword, string expectedTerminator, BlockType blockType = BlockType.Statement, bool keywordIsMetaCode = false) {
            IEnumerable<SyntaxTreeNode> expectedNodes = null;
            if (keywordIsMetaCode) {
                expectedNodes = new SyntaxTreeNode[] {
                    new MetaCodeSpan(keyword, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan(String.Empty)
                };
            }
            else {
                expectedNodes = new SyntaxTreeNode[] {
                    new CodeSpan(keyword)
                };
            }

            ParseBlockTest(keyword,
                            new Block(blockType, expectedNodes),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, keyword, expectedTerminator),
                                            SourceLocation.Zero));
        }

        private void RunUnterminatedBlockTest(string keyword, string expectedTerminator, BlockType blockType = BlockType.Statement, bool keywordIsMetaCode = false) {
            const string blockBody = @"
    ' This block is not correctly terminated!";

            IEnumerable<SyntaxTreeNode> expectedNodes = null;
            if (keywordIsMetaCode) {
                expectedNodes = new SyntaxTreeNode[] {
                    new MetaCodeSpan(keyword, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan(blockBody)
                };
            }
            else {
                expectedNodes = new SyntaxTreeNode[] {
                    new CodeSpan(keyword + blockBody)
                };
            }

            ParseBlockTest(keyword + blockBody,
                            new Block(blockType, expectedNodes),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, keyword, expectedTerminator),
                                            SourceLocation.Zero));
        }
    }
}

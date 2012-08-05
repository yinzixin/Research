using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpExplicitExpressionTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockShouldOutputZeroLengthCodeSpanIfExplicitExpressionIsEmpty() {
            ParseBlockTest("()",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldOutputZeroLengthCodeSpanIfEOFOccursAfterStartOfExplicitExpression() {
            ParseBlockTest("(",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                          RazorResources.BlockName_ExplicitExpression,
                                                          ")", "("),
                                            SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptEscapedQuoteInNonVerbatimStrings() {
            ParseBlockTest("(\"\\\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("\"\\\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptEscapedQuoteInVerbatimStrings() {
            ParseBlockTest("(@\"\"\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("@\"\"\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptMultipleRepeatedEscapedQuoteInVerbatimStrings() {
            ParseBlockTest("(@\"\"\"\"\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("@\"\"\"\"\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }
        [TestMethod]
        public void ParseBlockShouldAcceptMultiLineVerbatimStrings() {
            ParseBlockTest(@"(@""
Foo
Bar
Baz
"")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"@""
Foo
Bar
Baz
"""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptMultipleEscapedQuotesInNonVerbatimStrings() {
            ParseBlockTest("(\"\\\"hello, world\\\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("\"\\\"hello, world\\\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptMultipleEscapedQuotesInVerbatimStrings() {
            ParseBlockTest("(@\"\"\"hello, world\"\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("@\"\"\"hello, world\"\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptConsecutiveEscapedQuotesInNonVerbatimStrings() {
            ParseBlockTest("(\"\\\"\\\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("\"\\\"\\\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockShouldAcceptConsecutiveEscapedQuotesInVerbatimStrings() {
            ParseBlockTest("(@\"\"\"\"\"\")",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("@\"\"\"\"\"\""),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }
    }
}

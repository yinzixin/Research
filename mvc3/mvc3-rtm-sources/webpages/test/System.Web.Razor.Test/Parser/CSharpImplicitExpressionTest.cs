using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Collections.Generic;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpImplicitExpressionTest : CsHtmlCodeParserTestBase {
        private const string TestExtraKeyword = "model";
        private ISet<string> KeywordSet = null;

        public override ParserBase CreateCodeParser() {
            CSharpCodeParser parser = new CSharpCodeParser();
            parser.RazorKeywords.Add(TestExtraKeyword, _ => { Assert.Fail("Should never be called!"); return true; });
            KeywordSet = parser.TopLevelKeywords;
            return parser;
        }

        [TestMethod]
        public void AddingKeywordToRazorKeywordsListBeforeAccessingTopLevelKeywordsAddsItToTopLevelKeywords() {
            CSharpCodeParser parser = new CSharpCodeParser();
            parser.RazorKeywords.Add(TestExtraKeyword, _ => { Assert.Fail("Should never be called!"); return true; });
            Assert.IsTrue(parser.TopLevelKeywords.Contains(TestExtraKeyword));
        }

        [TestMethod]
        public void InnerImplicitExpressionWithOnlySingleAtOutputsZeroLengthCodeSpan() {
            ParseBlockTest(@"{@}",
                           new StatementBlock(
                               new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(String.Empty),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, KeywordSet, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                               ),
                               new CodeSpan(String.Empty),
                               new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           designTimeParser: true);
        }

        [TestMethod]
        public void InnerImplicitExpressionDoesNotAcceptDotAfterAt() {
            ParseBlockTest(@"{@.}",
                           new StatementBlock(
                               new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(String.Empty),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, KeywordSet, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                               ),
                               new CodeSpan("."),
                               new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           designTimeParser: true);
        }

        [TestMethod]
        public void InnerImplicitExpressionWithOnlySingleAtAcceptsSingleSpaceOrNewlineAtDesignTime() {
            ParseBlockTest(@"{
    @
}",
                           new StatementBlock(
                               new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(@"
    "),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, KeywordSet, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                               ),
                               new CodeSpan(@"
"),
                               new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                           designTimeParser: true);
        }

        [TestMethod]
        public void InnerImplicitExpressionDoesNotAcceptTrailingNewlineInRunTimeMode() {
            ParseBlockTest(@"{@foo.
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan(@"foo.", KeywordSet, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                                new CodeSpan(@"
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void InnerImplicitExpressionAcceptsTrailingNewlineInDesignTimeMode() {
            ParseBlockTest(@"{@foo.
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(String.Empty),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan(@"foo.", KeywordSet, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                                new CodeSpan(@"
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                            designTimeParser: true);
        }
        
        [TestMethod]
        public void ParseBlockAcceptsNonEnglishCharactersThatAreValidIdentifiers() {
            ImplicitExpressionTest("हळूँजद॔.", "हळूँजद॔");
        }

        [TestMethod]
        public void ParseBlockOutputsZeroLengthCodeSpanIfInvalidCharacterFollowsTransition() {
            ImplicitExpressionTest("/", String.Empty, 
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "/"), 
                                                SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockOutputsZeroLengthCodeSpanIfEOFOccursAfterTransition() {
            ImplicitExpressionTest(String.Empty,
                                new RazorError(RazorResources.ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock,
                                                SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockSupportsSlashesWithinComplexImplicitExpressions() {
            ImplicitExpressionTest("DataGridColumn.Template(\"Years of Service\", e => (int)Math.Round((DateTime.Now - dt).TotalDays / 365))");
        }

        [TestMethod]
        public void ParseBlockMethodParsesSingleIdentifierAsImplicitExpression() {
            ImplicitExpressionTest("foo");
        }

        [TestMethod]
        public void ParseBlockMethodDoesNotAcceptSemicolonIfExpressionTerminatedByWhitespace() {
            ImplicitExpressionTest("foo ;", "foo");
        }

        [TestMethod]
        public void ParseBlockMethodIgnoresSemicolonAtEndOfSimpleImplicitExpression() {
            RunTrailingSemicolonTest("foo");
        }

        [TestMethod]
        public void ParseBlockMethodParsesDottedIdentifiersAsImplicitExpression() {
            ImplicitExpressionTest("foo.bar.baz");
        }

        [TestMethod]
        public void ParseBlockMethodIgnoresSemicolonAtEndOfDottedIdentifiers() {
            RunTrailingSemicolonTest("foo.bar.baz");
        }

        [TestMethod]
        public void ParseBlockMethodDoesNotIncludeDotAtEOFInImplicitExpression() {
            ImplicitExpressionTest("foo.bar.", "foo.bar");
        }

        [TestMethod]
        public void ParseBlockMethodDoesNotIncludeDotFollowedByInvalidIdentifierCharacterInImplicitExpression() {
            ImplicitExpressionTest("foo.bar.0", "foo.bar");
            ImplicitExpressionTest("foo.bar.</p>", "foo.bar");
        }

        [TestMethod]
        public void ParseBlockMethodDoesNotIncludeSemicolonAfterDot() {
            ImplicitExpressionTest("foo.bar.;", "foo.bar");
        }

        [TestMethod]
        public void ParseBlockMethodTerminatesAfterIdentifierUnlessFollowedByDotOrParenInImplicitExpression() {
            ImplicitExpressionTest("foo.bar</p>", "foo.bar");
        }

        [TestMethod]
        public void ParseBlockProperlyParsesParenthesesAndBalancesThemInImplicitExpression() {
            ImplicitExpressionTest(@"foo().bar(""bi\""z"", 4)(""chained method; call"").baz(@""bo""""z"", '\'', () => { return 4; }, (4+5+new { foo = bar[4] }))");
        }

        [TestMethod]
        public void ParseBlockProperlyParsesBracketsAndBalancesThemInImplicitExpression() {
            ImplicitExpressionTest(@"foo.bar[4 * (8 + 7)][""fo\""o""].baz");
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionAtHtmlEndTag() {
            ImplicitExpressionTest("foo().bar.baz</p>zoop", "foo().bar.baz");
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionAtHtmlStartTag() {
            ImplicitExpressionTest("foo().bar.baz<p>zoop", "foo().bar.baz");
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionBeforeDotIfDotNotFollowedByIdentifierStartCharacter() {
            ImplicitExpressionTest("foo().bar.baz.42", "foo().bar.baz");
        }

        [TestMethod]
        public void ParseBlockStopsBalancingParenthesesAtEOF() {
            ImplicitExpressionTest("foo((((()()()((()()(((((", "foo((((()()()((()()(((((", 
                                   acceptedCharacters: AcceptedCharacters.Any,
                                   errors: new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(3, 0, 3)));
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionIfCloseParenFollowedByAnyWhiteSpace() {
            ImplicitExpressionTest("foo.bar() (baz)", "foo.bar()");
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionIfIdentifierFollowedByAnyWhiteSpace() {
            ImplicitExpressionTest("foo .bar() (baz)", "foo");
        }

        [TestMethod]
        public void ParseBlockTerminatesImplicitExpressionAtLastValidPointIfDotFollowedByWhitespace() {
            ImplicitExpressionTest("foo. bar() (baz)", "foo");
        }

        [TestMethod]
        public void ParseBlockOutputExpressionIfModuleTokenNotFollowedByBrace() {
            ImplicitExpressionTest("module.foo()");
        }

        private void RunTrailingSemicolonTest(string expr) {
            ParseBlockTest(expr + ";",
                           new ExpressionBlock(
                               new ImplicitExpressionSpan(expr, KeywordSet, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                           ));
        }

    }
}

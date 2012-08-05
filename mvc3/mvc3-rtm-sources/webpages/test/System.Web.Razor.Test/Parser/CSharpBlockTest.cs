using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpBlockTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockMethodThrowsArgNullExceptionOnNullContext() {
            // Arrange
            CSharpCodeParser parser = new CSharpCodeParser();

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => parser.ParseBlock(), RazorResources.Parser_Context_Not_Set);
        }

        [TestMethod]
        public void BalancingBracketsIgnoresStringLiteralCharactersAndBracketsInsideSingleLineComments() {
            SingleSpanBlockTest(@"if(foo) {
    // bar } "" baz '
    zoop();
}", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockAcceptsImplicitExpression() {
            ParseBlockTest("if (true) { @foo }",
                            new StatementBlock(
                                new CodeSpan("if (true) { "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new CodeSpan(" }")));
        }

        [TestMethod]
        public void ParseBlockAcceptsIfStatementWithinCodeBlockIfInDesignTimeMode() {
            ParseBlockTest("if (true) { @if(false) { } }",
                           new StatementBlock(
                               new CodeSpan("if (true) { "),
                               new StatementBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new CodeSpan("if(false) { }")
                               ),
                               new CodeSpan(" }")),
                           expectedErrors: new RazorError(String.Format(RazorResources.ParseError_Unexpected_Keyword_After_At,
                                                                        "if"),
                                                          new SourceLocation(13, 0, 13)));
        }

        [TestMethod]
        public void BalancingBracketsIgnoresStringLiteralCharactersAndBracketsInsideBlockComments() {
            SingleSpanBlockTest(
@"if(foo) {
    /* bar } "" */ ' baz } '
    zoop();
}", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsForKeyword() {
            SingleSpanBlockTest("for(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsForeachKeyword() {
            SingleSpanBlockTest("foreach(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsWhileKeyword() {
            SingleSpanBlockTest("while(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsUsingKeywordFollowedByParen() {
            SingleSpanBlockTest("using(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsUsingsNestedWithinOtherBlocks() {
            SingleSpanBlockTest("if(foo) { using(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); } }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsIfKeywordWithNoElseBranches() {
            SingleSpanBlockTest("if(int i = 0; i < 10; new Foo { Bar = \"baz\" }) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockAllowsEmptyBlockStatement() {
            SingleSpanBlockTest("if(false) { }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockTerminatesParenBalancingAtEOF() {
            ImplicitExpressionTest("Html.En(code()", "Html.En(code()",
                                   acceptedCharacters: AcceptedCharacters.Any,
                                   errors: new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF,
                                                                        "(", ")"),
                                                          new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenIfAndElseClause() {
            SingleSpanBlockTest("if(foo) { bar(); } /* Foo */ /* Bar */ else { baz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenIfAndElseClause() {
            RunRazorCommentBetweenClausesTest("if(foo) { bar(); } ", " else { baz(); }", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenElseIfAndElseClause() {
            SingleSpanBlockTest("if(foo) { bar(); } else if(bar) { baz(); } /* Foo */ /* Bar */ else { biz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenElseIfAndElseClause() {
            RunRazorCommentBetweenClausesTest("if(foo) { bar(); } else if(bar) { baz(); } ", " else { baz(); }", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenIfAndElseIfClause() {
            SingleSpanBlockTest("if(foo) { bar(); } /* Foo */ /* Bar */ else if(bar) { baz(); }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenIfAndElseIfClause() {
            RunRazorCommentBetweenClausesTest("if(foo) { bar(); } ", " else if(bar) { baz(); }");
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenIfAndElseClause() {
            SingleSpanBlockTest(@"if(foo) { bar(); } 
// Foo
// Bar
else { baz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenElseIfAndElseClause() {
            SingleSpanBlockTest(@"if(foo) { bar(); } else if(bar) { baz(); }
// Foo
// Bar
else { biz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenIfAndElseIfClause() {
            SingleSpanBlockTest(@"if(foo) { bar(); }
// Foo
// Bar
else if(bar) { baz(); }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockParsesElseIfBranchesOfIfStatement() {
            const string ifStatement = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";
            const string elseIfBranch = @" else if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) { 
    Debug.WriteLine(@""bar } baz""); 
}";
            const string document = ifStatement + elseIfBranch;

            SingleSpanBlockTest(document, BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockParsesMultipleElseIfBranchesOfIfStatement() {
            const string ifStatement = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";
            const string elseIfBranch = @" else if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) { 
    Debug.WriteLine(@""bar } baz""); 
}";
            const string document = ifStatement + elseIfBranch + elseIfBranch + elseIfBranch + elseIfBranch;
            SingleSpanBlockTest(document, BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockParsesMultipleElseIfBranchesOfIfStatementFollowedByOneElseBranch() {
            const string ifStatement = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";
            const string elseIfBranch = @" else if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) { 
    Debug.WriteLine(@""bar } baz""); 
}";
            const string elseBranch = @" else { Debug.WriteLine(@""bar } baz""); }";
            const string document = ifStatement + elseIfBranch + elseIfBranch + elseBranch;

            SingleSpanBlockTest(document, BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockStopsParsingCodeAfterElseBranch() {
            const string ifStatement = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";
            const string elseIfBranch = @" else if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) { 
    Debug.WriteLine(@""bar } baz""); 
}";
            const string elseBranch = @" else { Debug.WriteLine(@""bar } baz""); }";
            const string document = ifStatement + elseIfBranch + elseBranch + elseIfBranch;
            const string expected = ifStatement + elseIfBranch + elseBranch;

            ParseBlockTest(document, new StatementBlock(new CodeSpan(expected, hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockStopsParsingIfIfStatementNotFollowedByElse() {
            const string document = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";

            SingleSpanBlockTest(document, BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockAcceptsElseIfWithNoCondition() {
            // We don't want to be a full C# parser - If the else if is missing it's condition, the C# compiler can handle that, we have all the info we need to keep parsing
            const string ifBranch = @"if(int i = 0; i < 10; new Foo { Bar = ""baz"" }) {
    Debug.WriteLine(@""foo } bar""); 
}";
            const string elseIfBranch = @" else if { foo(); }";
            const string document = ifBranch + elseIfBranch;

            SingleSpanBlockTest(document, BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesDoWhileBlock() {
            SingleSpanBlockTest("do { var foo = bar; } while(foo != bar);", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesDoWhileBlockMissingSemicolon() {
            SingleSpanBlockTest("do { var foo = bar; } while(foo != bar)", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesDoWhileBlockMissingWhileCondition() {
            SingleSpanBlockTest("do { var foo = bar; } while", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesDoWhileBlockMissingWhileConditionWithSemicolon() {
            SingleSpanBlockTest("do { var foo = bar; } while;", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesDoWhileBlockMissingWhileClauseEntirely() {
            SingleSpanBlockTest("do { var foo = bar; } narf;", "do { var foo = bar; }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenDoAndWhileClause() {
            SingleSpanBlockTest("do { var foo = bar; } /* Foo */ /* Bar */ while(true);", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenDoAndWhileClause() {
            SingleSpanBlockTest(@"do { var foo = bar; } 
// Foo
// Bar
while(true);", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenDoAndWhileClause() {
            RunRazorCommentBetweenClausesTest("do { var foo = bar; } ", " while(true);", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockCorrectlyParsesMarkupInDoWhileBlock() {
            ParseBlockTest("do { var foo = bar; <p>Foo</p> foo++; } while (foo<bar>);",
                            new StatementBlock(
                                new CodeSpan("do { var foo = bar;"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Foo</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("foo++; } while (foo<bar>);", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsSwitchKeyword() {
            SingleSpanBlockTest(@"switch(foo) {
    case 0:
        break;
    case 1:
        {
            break;
        }
    case 2:
        return;
    default:
        return;
}", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSkipsParenthesisedExpressionAndThenBalancesBracesIfFirstIdentifierIsLockKeyword() {
            SingleSpanBlockTest("lock(foo) { Debug.WriteLine(@\"foo } bar\"); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockHasErrorsIfNamespaceImportMissingSemicolon() {
            NamespaceImportTest("using Foo.Bar.Baz", " Foo.Bar.Baz", acceptedCharacters: AcceptedCharacters.NonWhiteSpace | AcceptedCharacters.WhiteSpace, location: new SourceLocation(17, 0, 17));
        }

        [TestMethod]
        public void ParseBlockHasErrorsIfNamespaceAliasMissingSemicolon() {
            NamespaceImportTest("using Foo.Bar.Baz = FooBarBaz", " Foo.Bar.Baz = FooBarBaz", acceptedCharacters: AcceptedCharacters.NonWhiteSpace | AcceptedCharacters.WhiteSpace, location: new SourceLocation(29, 0, 29));
        }

        [TestMethod]
        public void ParseBlockParsesNamespaceImportWithSemicolonForUsingKeywordIfIsInValidFormat() {
            NamespaceImportTest("using Foo.Bar.Baz;", " Foo.Bar.Baz", AcceptedCharacters.NonWhiteSpace | AcceptedCharacters.WhiteSpace);
        }

        [TestMethod]
        public void ParseBlockDoesntCaptureWhitespaceAfterUsing() {
            ParseBlockTest("using Foo   ",
                            new DirectiveBlock(
                                new NamespaceImportSpan(SpanKind.Code,
                                                        "using Foo",
                                                        AcceptedCharacters.NonWhiteSpace | AcceptedCharacters.WhiteSpace,
                                                        " Foo",
                                                        CSharpCodeParser.UsingKeywordLength)));
        }

        [TestMethod]
        public void ParseBlockParsesNamespaceAliasWithSemicolonForUsingKeywordIfIsInValidFormat() {
            NamespaceImportTest("using FooBarBaz = FooBarBaz;", " FooBarBaz = FooBarBaz", AcceptedCharacters.NonWhiteSpace | AcceptedCharacters.WhiteSpace);
        }

        [TestMethod]
        public void ParseBlockTerminatesUsingKeywordAtEOFAndOutputsFileCodeBlock() {
            SingleSpanBlockTest("using                    ", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockTerminatesSingleLineCommentAtEndOfFile() {
            const string document = "foreach(var f in Foo) { // foo bar baz";
            SingleSpanBlockTest(document, document, BlockType.Statement, SpanKind.Code,
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "foreach", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockTerminatesBlockCommentAtEndOfFile() {
            const string document = "foreach(var f in Foo) { /* foo bar baz";
            SingleSpanBlockTest(document, document, BlockType.Statement, SpanKind.Code,
                                new RazorError(String.Format(RazorResources.ParseError_BlockComment_Not_Terminated), new SourceLocation(24, 0, 24)),
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "foreach", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockTerminatesSingleSlashAtEndOfFile() {
            const string document = "foreach(var f in Foo) { / foo bar baz";
            SingleSpanBlockTest(document, document, BlockType.Statement, SpanKind.Code,
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "foreach", '}', '{'), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenTryAndFinallyClause() {
            SingleSpanBlockTest("try { bar(); } /* Foo */ /* Bar */ finally { baz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenTryAndFinallyClause() {
            RunRazorCommentBetweenClausesTest("try { bar(); } ", " finally { biz(); }", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenCatchAndFinallyClause() {
            SingleSpanBlockTest("try { bar(); } catch(bar) { baz(); } /* Foo */ /* Bar */ finally { biz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenCatchAndFinallyClause() {
            RunRazorCommentBetweenClausesTest("try { bar(); } catch(bar) { baz(); } ", " finally { biz(); }", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsBlockCommentBetweenTryAndCatchClause() {
            SingleSpanBlockTest("try { bar(); } /* Foo */ /* Bar */ catch(bar) { baz(); }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsRazorCommentBetweenTryAndCatchClause() {
            RunRazorCommentBetweenClausesTest("try { bar(); }", " catch(bar) { baz(); }");
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenTryAndFinallyClause() {
            SingleSpanBlockTest(@"try { bar(); } 
// Foo
// Bar
finally { baz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenCatchAndFinallyClause() {
            SingleSpanBlockTest(@"try { bar(); } catch(bar) { baz(); }
// Foo
// Bar
finally { biz(); }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsLineCommentBetweenTryAndCatchClause() {
            SingleSpanBlockTest(@"try { bar(); }
// Foo
// Bar
catch(bar) { baz(); }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsTryStatementWithNoAdditionalClauses() {
            SingleSpanBlockTest("try { var foo = new { } }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupWithinTryClause() {
            RunSimpleWrappedMarkupTest("try {", " <p>Foo</p> ", "}");
        }

        [TestMethod]
        public void ParseBlockSupportsTryStatementWithOneCatchClause() {
            SingleSpanBlockTest("try { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupWithinCatchClause() {
            RunSimpleWrappedMarkupTest("try { var foo = new { } } catch(Foo Bar Baz) {", " <p>Foo</p> ", "}");
        }

        [TestMethod]
        public void ParseBlockSupportsTryStatementWithMultipleCatchClause() {
            SingleSpanBlockTest("try { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsExceptionLessCatchClauses() {
            SingleSpanBlockTest("try { var foo = new { } } catch { var foo = new { } }", BlockType.Statement, SpanKind.Code);
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupWithinAdditionalCatchClauses() {
            RunSimpleWrappedMarkupTest("try { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } } catch(Foo Bar Baz) { var foo = new { } } catch(Foo Bar Baz) {", " <p>Foo</p> ", "}");
        }

        [TestMethod]
        public void ParseBlockSupportsTryStatementWithFinallyClause() {
            SingleSpanBlockTest("try { var foo = new { } } finally { var foo = new { } }", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupWithinFinallyClause() {
            RunSimpleWrappedMarkupTest("try { var foo = new { } } finally {", " <p>Foo</p> ", "}", acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockStopsParsingCatchClausesAfterFinallyBlock() {
            string expectedContent = "try { var foo = new { } } finally { var foo = new { } }";
            SingleSpanBlockTest(expectedContent + " catch(Foo Bar Baz) { }", expectedContent, BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockDoesNotAllowMultipleFinallyBlocks() {
            string expectedContent = "try { var foo = new { } } finally { var foo = new { } }";
            SingleSpanBlockTest(expectedContent + " finally { }", expectedContent, BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockAcceptsTrailingDotIntoImplicitExpressionWhenEmbeddedInCode() {
            // Arrange
            ParseBlockTest(@"if(foo) { @foo. }",
                            new StatementBlock(
                                new CodeSpan("if(foo) { "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new CodeSpan(" }")
                            ));
        }

        [TestMethod]
        public void ParseBlockParsesExpressionOnSwitchCharacterFollowedByOpenParen() {
            // Arrange
            ParseBlockTest(@"if(foo) { @(foo + bar) }",
                            new StatementBlock(
                                new CodeSpan("if(foo) { "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CodeSpan("foo + bar"),
                                    new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(" }")
                            ));
        }

        [TestMethod]
        public void ParseBlockParsesExpressionOnSwitchCharacterFollowedByIdentifierStart() {
            // Arrange
            ParseBlockTest(@"if(foo) { @foo[4].bar() }",
                            new StatementBlock(
                                new CodeSpan("if(foo) { "),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("foo[4].bar()", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new CodeSpan(" }")
                            ));
        }

        [TestMethod]
        public void ParseBlockTreatsDoubleAtSignAsEscapeSequenceIfAtStatementStart() {
            // Arrange
            ParseBlockTest(@"if(foo) { @@class.Foo() }",
                            new StatementBlock(
                                new CodeSpan("if(foo) { "),
                                new CodeSpan("@", hidden: true),
                                new CodeSpan("@class.Foo() }")
                            ));
        }

        [TestMethod]
        public void ParseBlockTreatsAtSignsAfterFirstPairAsPartOfCSharpStatement() {
            // Arrange
            ParseBlockTest(@"if(foo) { @@@@class.Foo() }",
                            new StatementBlock(
                                new CodeSpan("if(foo) { "),
                                new CodeSpan("@", hidden: true),
                                new CodeSpan("@@@class.Foo() }")
                            ));
        }

        [TestMethod]
        public void ParseBlockDoesNotParseMarkupStatementOrExpressionOnSwitchCharacterNotFollowedByOpenAngleOrColon() {
            // Arrange
            ParseBlockTest("if(foo) { @\"Foo\".ToString(); }",
                           new StatementBlock(
                               new CodeSpan("if(foo) { "),
                               new ExpressionBlock(
                                   new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                   new ImplicitExpressionSpan(String.Empty, CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                              new CodeSpan("\"Foo\".ToString(); }")));
        }

        [TestMethod]
        public void ParsersCanNestRecursively() {
            // Arrange
            ParseBlockTest(@"foreach(var c in db.Categories) {
            <div class=""category"">
                <h1>@c.Name</h1>
                <ul>
                    @foreach(var p in c.Products) {
                        <li><a href=""@Html.ActionUrl(""Products"", ""Detail"", new { id = p.Id })"">@p.Name</a></li>
                    }
                </ul>
            </div>
        }",
                            new StatementBlock(
                                new CodeSpan(@"foreach(var c in db.Categories) {
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"            <div class=""category"">
                <h1>"),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan(@"c.Name", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(@"</h1>
                <ul>
"),
                                    new StatementBlock(
                                        new CodeSpan(@"                    "),
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(@"foreach(var p in c.Products) {
"),
                                        new MarkupBlock(
                                            new MarkupSpan(@"                        <li><a href="""),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan(@"Html.ActionUrl(""Products"", ""Detail"", new { id = p.Id })", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(@""">"),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("p.Name", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(@"</a></li>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(@"                    }
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(@"                </ul>
            </div>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        }", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        private void RunRazorCommentBetweenClausesTest(string preComment, string postComment, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any) {
            ParseBlockTest(preComment + "@* Foo *@ @* Bar *@" + postComment,
                            new StatementBlock(
                                new CodeSpan(preComment),
                                new CommentBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CommentSpan(" Foo "),
                                    new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(" "),
                                new CommentBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new CommentSpan(" Bar "),
                                    new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(postComment, hidden: false, acceptedCharacters: acceptedCharacters)));
        }

        private void RunSimpleWrappedMarkupTest(string prefix, string markup, string suffix, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any) {
            ParseBlockTest(prefix + markup + suffix,
                            new StatementBlock(
                                new CodeSpan(prefix),
                                new MarkupBlock(
                                    new MarkupSpan(markup, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(suffix, hidden: false, acceptedCharacters: acceptedCharacters)
                            ));
        }

        private void NamespaceImportTest(string content, string expectedNS, AcceptedCharacters acceptedCharacters = AcceptedCharacters.None, string errorMessage = null, SourceLocation? location = null) {
            var errors = new RazorError[0];
            if (!String.IsNullOrEmpty(errorMessage) && location.HasValue) {
                errors = new RazorError[] { 
                    new RazorError(errorMessage, location.Value) 
                };
            }
            ParseBlockTest(content,
                            new DirectiveBlock(
                                new NamespaceImportSpan(SpanKind.Code,
                                                        content,
                                                        acceptedCharacters,
                                                        expectedNS,
                                                        CSharpCodeParser.UsingKeywordLength)), errors);
        }
    }
}

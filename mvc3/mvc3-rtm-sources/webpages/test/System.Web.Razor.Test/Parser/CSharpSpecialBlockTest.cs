using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpSpecialBlockTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseInheritsStatementMarksInheritsSpanAsCanGrowIfMissingTrailingSpace() {
            ParseBlockTest("inherits",
                            new DirectiveBlock(
                                new MetaCodeSpan("inherits", hidden: false, acceptedCharacters: AcceptedCharacters.Any)
                            ),
                            new RazorError(RazorResources.ParseError_InheritsKeyword_Must_Be_Followed_By_TypeName,
                                           new SourceLocation(8, 0, 8)));
        }

        [TestMethod]
        public void InheritsBlockAcceptsMultipleGenericArguments() {
            ParseBlockTest("inherits Foo.Bar<Biz<Qux>, string, int>.Baz",
                            new DirectiveBlock(
                                new MetaCodeSpan("inherits ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new InheritsSpan("Foo.Bar<Biz<Qux>, string, int>.Baz")
                            ));
        }

        [TestMethod]
        public void InheritsBlockOutputsErrorIfInheritsNotFollowedByTypeButAcceptsEntireLineAsCode() {
            ParseBlockTest(@"inherits                
foo",
                            new DirectiveBlock(
                                new MetaCodeSpan("inherits ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new InheritsSpan(@"               ", baseClass: String.Empty)
                            ),
                            new RazorError(RazorResources.ParseError_InheritsKeyword_Must_Be_Followed_By_TypeName, new SourceLocation(8,0,8)));
        }

        [TestMethod]
        public void NamespaceImportInsideCodeBlockCausesError() {
                           
            ParseBlockTest("{ using Foo.Bar.Baz; var foo = bar; }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" using Foo.Bar.Baz; var foo = bar; "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ), 
                            new RazorError(RazorResources.ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock,
                                            new SourceLocation(2, 0, 2))
                           );
        }

        [TestMethod]
        public void TypeAliasInsideCodeBlockIsNotHandledSpecially() {
            ParseBlockTest("{ using Foo = Bar.Baz; var foo = bar; }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" using Foo = Bar.Baz; var foo = bar; "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            new RazorError(RazorResources.ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock,
                                            new SourceLocation(2, 0, 2))
                           );
        }

        [TestMethod]
        public void Plan9FunctionsKeywordInsideCodeBlockIsNotHandledSpecially() {
            ParseBlockTest("{ functions Foo; }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" functions Foo; "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void NonKeywordStatementInCodeBlockIsHandledCorrectly() {
            ParseBlockTest(@"{
    List<dynamic> photos = gallery.Photo.ToList();
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    List<dynamic> photos = gallery.Photo.ToList();
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockBalancesBracesOutsideStringsIfFirstCharacterIsBraceAndReturnsSpanOfTypeCode() {
            // Arrange
            const string code = "foo\"b}ar\" if(condition) { String.Format(\"{0}\"); } ";

            // Act/Assert
            ParseBlockTest("{" + code + "}",
                           new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(code),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockBalancesParensOutsideStringsIfFirstCharacterIsParenAndReturnsSpanOfTypeExpression() {
            // Arrange
            const string code = "foo\"b)ar\" if(condition) { String.Format(\"{0}\"); } ";

            // Act/Assert
            ParseBlockTest("(" + code + ")",
                           new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(code),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockBalancesBracesAndOutputsContentAsClassLevelCodeSpanIfFirstIdentifierIsFunctionsKeyword() {
            const string code = " foo(); \"bar}baz\" ";
            ParseBlockTest("functions {" + code + "} zoop",
                           new FunctionsBlock(
                                new MetaCodeSpan("functions {", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(code),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockDoesNoErrorRecoveryForFunctionsBlock() {
            ParseBlockTest("functions { { { { { } zoop",
                           new FunctionsBlock(
                                new MetaCodeSpan("functions {", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" { { { { } zoop")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "functions", "}", "{"),
                                           SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockIgnoresFunctionsUnlessAllLowerCase() {
            ParseBlockTest("Functions { foo() }",
                            new ExpressionBlock(
                                    new ImplicitExpressionSpan("Functions", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                )
                            );
        }

        [TestMethod]
        public void ParseBlockIgnoresSingleSlashAtStart() {
            ParseBlockTest("/ foo",
                           new ExpressionBlock(
                               new ImplicitExpressionSpan(String.Empty,
                                                          CSharpCodeParser.DefaultKeywords,
                                                          acceptTrailingDot: false,
                                                          acceptedCharacters: AcceptedCharacters.NonWhiteSpace)),
                           new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "/"), 
                                           SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockTerminatesSingleLineCommentAtEndOfLine() {
            ParseBlockTest(@"if(!false) {
    // Foo
	<p>A real tag!</p>
}",
                            new StatementBlock(
                                new CodeSpan(@"if(!false) {
    // Foo
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"	<p>A real tag!</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}")
                            ));
                                
        }

    }
}

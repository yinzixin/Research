using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBSpecialKeywordsTest : VBHtmlCodeParserTestBase {
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
            ParseBlockTest("inherits Foo.Bar(Of Biz(Of Qux), String, Integer).Baz",
                            new DirectiveBlock(
                                new MetaCodeSpan("inherits ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new InheritsSpan("Foo.Bar(Of Biz(Of Qux), String, Integer).Baz")
                            ));
        }

        [TestMethod]
        public void InheritsBlockOutputsErrorIfInheritsNotFollowedByTypeButAcceptsEntireLineAsCode() {
            ParseBlockTest(@"inherits                
foo",
                            new DirectiveBlock(
                                new MetaCodeSpan("inherits ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new InheritsSpan(@"               ")
                            ),
                            new RazorError(RazorResources.ParseError_InheritsKeyword_Must_Be_Followed_By_TypeName, new SourceLocation(8, 0, 8)));
        }

        [TestMethod]
        public void ParseBlockShouldSupportNamespaceImports() {
            ParseBlockTest("Imports Foo.Bar.Baz.Biz.Boz Bar",
                            new DirectiveBlock(
                                new NamespaceImportSpan(SpanKind.MetaCode,
                                                        "Imports Foo.Bar.Baz.Biz.Boz ",
                                                        acceptedCharacters: AcceptedCharacters.Any,
                                                        ns: " Foo.Bar.Baz.Biz.Boz",
                                                        namespaceKeywordLength: VBCodeParser.ImportsKeywordLength)
                            ));

        }

        [TestMethod]
        public void ParseBlockShowsErrorIfNamespaceNotOnSameLineAsImportsKeyword() {
            ParseBlockTest(@"Imports
Foo",
                            new DirectiveBlock(
                                new NamespaceImportSpan(SpanKind.MetaCode, 
                                                        @"Imports
", 
                                                        acceptedCharacters: AcceptedCharacters.Any, 
                                                        ns: String.Empty, 
                                                        namespaceKeywordLength: VBCodeParser.ImportsKeywordLength)
                            ),
                            new RazorError(RazorResources.ParseError_NamespaceOrTypeAliasExpected, new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockShowsErrorIfTypeBeingAliasedNotOnSameLineAsImportsKeyword() {
            ParseBlockTest(@"Imports Foo =
System.Bar",
                           new DirectiveBlock(
                               new NamespaceImportSpan(SpanKind.MetaCode, 
                                                       @"Imports Foo =
",
                                                       acceptedCharacters: AcceptedCharacters.Any,
                                                       ns: " Foo =", 
                                                       namespaceKeywordLength: VBCodeParser.ImportsKeywordLength)
                           ),
                           new RazorError(RazorResources.ParseError_NamespaceOrTypeAliasExpected, new SourceLocation(13, 0, 13)));
        }

        [TestMethod]
        public void ParseBlockShouldSupportTypeAliases() {
            ParseBlockTest("Imports Foo = Bar.Baz.Biz.Boz Bar",
                            new DirectiveBlock(
                                new NamespaceImportSpan(SpanKind.MetaCode,
                                                        "Imports Foo = Bar.Baz.Biz.Boz ",
                                                        acceptedCharacters: AcceptedCharacters.Any,
                                                        ns: " Foo = Bar.Baz.Biz.Boz",
                                                        namespaceKeywordLength: VBCodeParser.ImportsKeywordLength)
                            ));
        }

        [TestMethod]
        public void ParseBlockThrowsErrorIfOptionIsNotFollowedByStrictOrExplicit() {
            ParseBlockTest("Option FizzBuzz On",
                           new DirectiveBlock(
                                new VBOptionSpan("Option FizzBuzz", null, true)
                           ),
                           new RazorError(String.Format(RazorResources.ParseError_UnknownOption, "FizzBuzz"), 
                                          new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockThrowsErrorIfOptionStrictIsNotFollowedByOnOrOff() {
            ParseBlockTest("Option Strict Yes",
                           new DirectiveBlock(
                                new VBOptionSpan("Option Strict Yes", "AllowLateBound", false)
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_InvalidOptionValue, "Strict", "Yes"),
                                           new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseBlockReadsToAfterOnKeywordIfOptionStrictBlock() {
            ParseBlockTest("Option Strict On Foo Bar Baz",
                            new DirectiveBlock(
                                new VBOptionSpan("Option Strict On", "AllowLateBound", false)
                            ));
        }

        [TestMethod]
        public void ParseBlockReadsToAfterOffKeywordIfOptionStrictBlock() {
            ParseBlockTest("Option Strict Off Foo Bar Baz",
                            new DirectiveBlock(
                                new VBOptionSpan("Option Strict Off", "AllowLateBound", true)
                            ));
        }

        [TestMethod]
        public void ParseBlockReadsToAfterOnKeywordIfOptionExplicitBlock() {
            ParseBlockTest("Option Explicit On Foo Bar Baz",
                            new DirectiveBlock(
                                new VBOptionSpan("Option Explicit On", "RequireVariableDeclaration", true)
                            ));
        }

        [TestMethod]
        public void ParseBlockReadsToAfterOffKeywordIfOptionExplicitBlock() {
            ParseBlockTest("Option Explicit Off Foo Bar Baz",
                            new DirectiveBlock(
                                new VBOptionSpan("Option Explicit Off", "RequireVariableDeclaration", false)
                            ));
        }
    }
}

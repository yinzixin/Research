using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpSectionTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseSectionBlockCapturesNewlineImmediatelyFollowing() {
            ParseDocumentTest(@"@section
",
                            new MarkupBlock(
                                new SectionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new SectionHeaderSpan(@"section
", String.Empty, acceptedCharacters: AcceptedCharacters.Any)
                                )
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                         RazorResources.ErrorComponent_EndOfFile), 
                                           new SourceLocation(10, 1, 0)));

        }

        [TestMethod]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingOpenBrace() {
            ParseDocumentTest(@"@section Foo         
    ",
                            new MarkupBlock(
                                new SectionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new SectionHeaderSpan("section Foo         ", "Foo", acceptedCharacters: AcceptedCharacters.Any)
                                ),
                                new MarkupSpan(@"
    ")
                            ),
                            new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, new SourceLocation(21, 0, 21)));

        }

        [TestMethod]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingName() {
            ParseDocumentTest(@"@section         
    ",
                            new MarkupBlock(
                                new SectionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new SectionHeaderSpan("section         ", String.Empty, acceptedCharacters: AcceptedCharacters.Any)
                                ),
                                new MarkupSpan(@"
    ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                          RazorResources.ErrorComponent_EndOfFile),
                                            new SourceLocation(23, 1, 4)));

        }

        [TestMethod]
        public void ParseSectionBlockIgnoresSectionUnlessAllLowerCase() {
            ParseDocumentTest("@Section foo",
                                new MarkupBlock(
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("Section", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" foo")
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfKeywordNotFollowedByIdentifierStartCharacter() {
            ParseDocumentTest("@section 9 { <p>Foo</p> }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section ", String.Empty, acceptedCharacters: AcceptedCharacters.Any)
                                    ),
                                    new MarkupSpan("9 { <p>Foo</p> }")
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                              String.Format(RazorResources.ErrorComponent_Character, "9")),
                                                new SourceLocation(9, 0, 9)));
        }

        [TestMethod]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfNameNotFollowedByOpenBrace() {
            ParseDocumentTest("@section foo-bar { <p>Foo</p> }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo", "foo", acceptedCharacters: AcceptedCharacters.Any)
                                    ),
                                    new MarkupSpan("-bar { <p>Foo</p> }")
                                ),
                                new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, new SourceLocation(12, 0, 12)));
        }

        [TestMethod]
        public void ParserOutputsErrorOnNestedSections() {
            ParseDocumentTest("@section foo { @section bar { <p>Foo</p> } }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(" "),
                                            new SectionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new SectionHeaderSpan("section bar {", "bar", acceptedCharacters: AcceptedCharacters.Any),
                                                new MarkupBlock(
                                                    new MarkupSpan(" <p>Foo</p> ")
                                                ),
                                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                            ),
                                            new MarkupSpan(" ")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_Sections_Cannot_Be_Nested, RazorResources.SectionExample_CS), new SourceLocation(23, 0, 23)));
        }

        [TestMethod]
        public void ParseSectionBlockHandlesEOFAfterOpenBrace() {
            ParseDocumentTest("@section foo {",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any) { AutoCompleteString = "}" },
                                        new MarkupBlock()
                                    )
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockHandlesUnterminatedSection() {
            ParseDocumentTest("@section foo { <p>Foo{}</p>",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any) { AutoCompleteString = "}" },
                                        new MarkupBlock(
                                            new MarkupSpan(" <p>Foo{}</p>")
                                        )
                                    )
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockReportsErrorAndAcceptsWhitespaceToEndOfLineIfSectionNotFollowedByOpenBrace() {
            ParseDocumentTest(@"@section foo      
",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo      ", "foo", acceptedCharacters: AcceptedCharacters.Any)
                                    ),
                                    new MarkupSpan(@"
")
                                ),
                                new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, new SourceLocation(18, 0, 18)));
        }

        [TestMethod]
        public void ParseSectionBlockAcceptsOpenBraceMultipleLinesBelowSectionName() {
            ParseDocumentTest(@"@section foo      





{
<p>Foo</p>
}",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan(@"section foo      





{", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
<p>Foo</p>
")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockParsesNamedSectionCorrectly() {
            ParseDocumentTest("@section foo { <p>Foo</p> }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(" <p>Foo</p> ")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockDoesNotRequireSpaceBetweenSectionNameAndOpenBrace() {
            ParseDocumentTest("@section foo{ <p>Foo</p> }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo{", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(" <p>Foo</p> ")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockBalancesBraces() {
            ParseDocumentTest("@section foo { <p>Foo{}</p> }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(" <p>Foo{}</p> ")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionBlockAllowsBracesInCSharpExpression() {
            ParseDocumentTest("@section foo { I really want to render a close brace, so here I go: @(\"}\") }",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("section foo {", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(" I really want to render a close brace, so here I go: "),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new CodeSpan("\"}\""),
                                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                            ),
                                            new MarkupSpan(" ")
                                        ),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }
    }
}

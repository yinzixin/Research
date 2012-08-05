using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBSectionTest : VBHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseSectionBlockCapturesNewlineImmediatelyFollowing() {
            ParseDocumentTest(@"@Section
",
                            new MarkupBlock(
                                new SectionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new SectionHeaderSpan(@"Section
", String.Empty, acceptedCharacters: AcceptedCharacters.Any),
                                    new MarkupBlock()
                                )
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                         RazorResources.ErrorComponent_EndOfFile),
                                           new SourceLocation(10, 1, 0)),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Section", "End Section"),
                                           new SourceLocation(1, 0, 1)));

        }

        [TestMethod]
        public void ParseSectionRequiresNameBeOnSameLineAsSectionKeyword() {
            ParseDocumentTest(@"@Section 
Foo
    <p>Body</p>
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section ", String.Empty, acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
Foo
    <p>Body</p>
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                              RazorResources.ErrorComponent_Whitespace),
                                                new SourceLocation(9, 0, 9)));
        }

        [TestMethod]
        public void ParseSectionAllowsNameToBeOnDifferentLineAsSectionKeywordIfUnderscoresUsed() {
            ParseDocumentTest(@"@Section _
_
Foo
    <p>Body</p>
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan(@"Section _
_
Foo", "Foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    <p>Body</p>
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionReportsErrorAndTerminatesSectionBlockIfKeywordNotFollowedByIdentifierStartCharacter() {
            ParseDocumentTest(@"@Section 9
    <p>Foo</p>
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section ", String.Empty, acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"9
    <p>Foo</p>
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                                                              String.Format(RazorResources.ErrorComponent_Character, "9")), new SourceLocation(9, 0, 9)));
        }

        [TestMethod]
        public void ParserOutputsErrorOnNestedSections() {
            ParseDocumentTest(@"@Section foo
    @Section bar
        <p>Foo</p>
    End Section
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    "),
                                            new SectionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new SectionHeaderSpan("Section bar", "bar", acceptedCharacters: AcceptedCharacters.Any),
                                                new MarkupBlock(
                                                    new MarkupSpan(@"
        <p>Foo</p>
    ")
                                                ),
                                                new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                            ),
                                            new MarkupSpan(@"
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_Sections_Cannot_Be_Nested, RazorResources.SectionExample_VB), new SourceLocation(26, 1, 12)));
        }

        [TestMethod]
        public void ParseSectionHandlesEOFAfterIdentifier() {
            ParseDocumentTest("@Section foo",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any) { AutoCompleteString = VBCodeParser.EndSectionKeyword },
                                        new MarkupBlock()
                                    )
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Section", "End Section"),
                                                new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseSectionHandlesUnterminatedSection() {
            ParseDocumentTest(@"@Section foo
    <p>Foo</p>",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any) { AutoCompleteString = VBCodeParser.EndSectionKeyword },
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    <p>Foo</p>")
                                        )
                                    )
                                ),
                                new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Section", "End Section"),
                                                new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseDocumentParsesNamedSectionCorrectly() {
            ParseDocumentTest(@"@Section foo
    <p>Foo</p>
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    <p>Foo</p>
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }

        [TestMethod]
        public void ParseSectionTerminatesOnFirstEndSection() {
            ParseDocumentTest(@"@Section foo
    <p>End Section</p>",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    <p>")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan("</p>")
                                ));
        }

        [TestMethod]
        public void ParseSectionAllowsEndSectionInVBExpression() {
            ParseDocumentTest(@"@Section foo
    I really want to render the word @(""End Section""), so this is how I do it
End Section",
                                new MarkupBlock(
                                    new SectionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new SectionHeaderSpan("Section foo", "foo", acceptedCharacters: AcceptedCharacters.Any),
                                        new MarkupBlock(
                                            new MarkupSpan(@"
    I really want to render the word "),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new CodeSpan("\"End Section\""),
                                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                            ),
                                            new MarkupSpan(@", so this is how I do it
")
                                        ),
                                        new MetaCodeSpan("End Section", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    ),
                                    new MarkupSpan(String.Empty)
                                ));
        }
    }
}

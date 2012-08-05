using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBTemplateTest : VBHtmlCodeParserTestBase {
        private const string TestTemplateCode = " @@<p>Foo #@item</p>";
        private static TemplateBlock TestTemplate() {
            return new TemplateBlock(
                new MarkupBlock(
                    new MarkupSpan(" "),
                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new MetaCodeSpan("@"),
                    new MarkupSpan("<p>Foo #"),
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan("item", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan("</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                )
            );
        }

        private const string TestNestedTemplateCode = " @@<p>Foo #@Html.Repeat(10, @@<p>@item</p>)</p>";
        private static TemplateBlock TestNestedTemplate() {
            return new TemplateBlock(
                new MarkupBlock(
                    new MarkupSpan(" "),
                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new MetaCodeSpan("@"),
                    new MarkupSpan("<p>Foo #"),
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan("Html.Repeat(10,", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                        new TemplateBlock(
                            new MarkupBlock(
                                new MarkupSpan(" "),
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MetaCodeSpan("@"),
                                new MarkupSpan("<p>"),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("item", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan("</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            )
                        ),
                        new ImplicitExpressionSpan(")", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan("</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                )
            );
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInExplicitExpressionParens() {
            ParseBlockTest("(Html.Repeat(10," + TestTemplateCode + "))",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan("Html.Repeat(10,"),
                                TestTemplate(),
                                new CodeSpan(")"),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInImplicitExpressionParens() {
            ParseBlockTest("Html.Repeat(10," + TestTemplateCode + ")",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan("Html.Repeat(10,", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                TestTemplate(),
                                new ImplicitExpressionSpan(")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlesTwoAnonymousSectionsInImplicitExpressionParens() {
            ParseBlockTest("Html.Repeat(10," + TestTemplateCode + "," + TestTemplateCode + ")",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan("Html.Repeat(10,", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                TestTemplate(),
                                new ImplicitExpressionSpan(",", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                TestTemplate(),
                                new ImplicitExpressionSpan(")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ));
        }

        [TestMethod]
        public void ParseBlockProducesErrorButCorrectlyParsesNestedAnonymousSectionInImplicitExpressionParens() {
            ParseBlockTest("Html.Repeat(10," + TestNestedTemplateCode + ")",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan("Html.Repeat(10,", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                TestNestedTemplate(),
                                new ImplicitExpressionSpan(")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ),
                            GetNestedSectionError(42, 0, 42));
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInStatementWithinCodeBlock() {
            ParseBlockTest(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo," + TestTemplateCode + @")
Next foo",
                            new StatementBlock(
                                new CodeSpan(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(@")
Next foo", hidden: false, acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlesTwoAnonymousSectionsInStatementWithinCodeBlock() {
            ParseBlockTest(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo," + TestTemplateCode + "," + TestTemplateCode + @")
Next foo",
                            new StatementBlock(
                                new CodeSpan(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(","),
                                TestTemplate(),
                                new CodeSpan(@")
Next foo", hidden: false, acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace)
                            ));
        }

        [TestMethod]
        public void ParseBlockProducesErrorButCorrectlyParsesNestedAnonymousSectionInStatementWithinCodeBlock() {
            ParseBlockTest(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo," + TestNestedTemplateCode + @")
Next foo",
                            new StatementBlock(
                                new CodeSpan(@"For Each foo in Bar 
    Html.ExecuteTemplate(foo,"),
                                TestNestedTemplate(),
                                new CodeSpan(@")
Next foo", hidden: false, acceptedCharacters: AcceptedCharacters.WhiteSpace | AcceptedCharacters.NonWhiteSpace)
                            ),
                            GetNestedSectionError(78, 1, 56));
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInStatementWithinStatementBlock() {
            ParseBlockTest(@"Code 
    Dim foo = bar
    Html.ExecuteTemplate(foo," + TestTemplateCode + @")
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@" 
    Dim foo = bar
    Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(@")
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlessTwoAnonymousSectionsInStatementWithinStatementBlock() {
            ParseBlockTest(@"Code
    Dim foo = bar
    Html.ExecuteTemplate(foo," + TestTemplateCode + "," + TestTemplateCode + @")
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    Dim foo = bar
    Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(","),
                                TestTemplate(),
                                new CodeSpan(@")
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockProducesErrorButCorrectlyParsesNestedAnonymousSectionInStatementWithinStatementBlock() {
            ParseBlockTest(@"Code
    Dim foo = bar
    Html.ExecuteTemplate(foo," + TestNestedTemplateCode + @")
End Code",
                            new StatementBlock(
                                new MetaCodeSpan("Code", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    Dim foo = bar
    Html.ExecuteTemplate(foo,"),
                                TestNestedTemplate(),
                                new CodeSpan(@")
"),
                                new MetaCodeSpan("End Code", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            GetNestedSectionError(81, 2, 56));
        }

        private static RazorError GetNestedSectionError(int absoluteIndex, int lineIndex, int characterIndex) {
            return new RazorError(RazorResources.ParseError_InlineMarkup_Blocks_Cannot_Be_Nested, new SourceLocation(absoluteIndex, lineIndex, characterIndex));
        }
    }
}

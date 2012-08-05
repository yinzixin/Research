using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpTemplateTest : CsHtmlCodeParserTestBase {
        private const string TestTemplateCode = " @<p>Foo #@item</p>";
        private static TemplateBlock TestTemplate() {
            return new TemplateBlock(
                new MarkupBlock(
                    new MarkupSpan(" "),
                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new MarkupSpan("<p>Foo #"),
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan("item", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan("</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                )
            );
        }

        private const string TestNestedTemplateCode = " @<p>Foo #@Html.Repeat(10, @<p>@item</p>)</p>";
        private static TemplateBlock TestNestedTemplate() {
            return new TemplateBlock(
                new MarkupBlock(
                    new MarkupSpan(" "),
                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new MarkupSpan("<p>Foo #"),
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan("Html.Repeat(10,", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                        new TemplateBlock(
                            new MarkupBlock(
                                new MarkupSpan(" "),
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new MarkupSpan("<p>"),
                                new ExpressionBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new ImplicitExpressionSpan("item", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                ),
                                new MarkupSpan("</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            )
                        ),
                        new ImplicitExpressionSpan(")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
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
                            GetNestedSectionError(41));
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInStatementWithinCodeBlock() {
            ParseBlockTest("foreach(foo in Bar) { Html.ExecuteTemplate(foo," + TestTemplateCode + "); }",
                            new StatementBlock(
                                new CodeSpan("foreach(foo in Bar) { Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan("); }", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlesTwoAnonymousSectionsInStatementWithinCodeBlock() {
            ParseBlockTest("foreach(foo in Bar) { Html.ExecuteTemplate(foo," + TestTemplateCode + "," + TestTemplateCode + "); }",
                            new StatementBlock(
                                new CodeSpan("foreach(foo in Bar) { Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(","),
                                TestTemplate(),
                                new CodeSpan("); }", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockProducesErrorButCorrectlyParsesNestedAnonymousSectionInStatementWithinCodeBlock() {
            ParseBlockTest("foreach(foo in Bar) { Html.ExecuteTemplate(foo," + TestNestedTemplateCode + "); }",
                            new StatementBlock(
                                new CodeSpan("foreach(foo in Bar) { Html.ExecuteTemplate(foo,"),
                                TestNestedTemplate(),
                                new CodeSpan("); }", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            GetNestedSectionError(73));
        }

        [TestMethod]
        public void ParseBlockHandlesSimpleAnonymousSectionInStatementWithinStatementBlock() {
            ParseBlockTest("{ var foo = bar; Html.ExecuteTemplate(foo," + TestTemplateCode + "); }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" var foo = bar; Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan("); "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockHandlessTwoAnonymousSectionsInStatementWithinStatementBlock() {
            ParseBlockTest("{ var foo = bar; Html.ExecuteTemplate(foo," + TestTemplateCode + "," + TestTemplateCode + "); }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" var foo = bar; Html.ExecuteTemplate(foo,"),
                                TestTemplate(),
                                new CodeSpan(","),
                                TestTemplate(),
                                new CodeSpan("); "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockProducesErrorButCorrectlyParsesNestedAnonymousSectionInStatementWithinStatementBlock() {
            ParseBlockTest("{ var foo = bar; Html.ExecuteTemplate(foo," + TestNestedTemplateCode + "); }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" var foo = bar; Html.ExecuteTemplate(foo,"),
                                TestNestedTemplate(),
                                new CodeSpan("); "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ),
                            GetNestedSectionError(68));
        }

        private static RazorError GetNestedSectionError(int characterIndex) {
            return new RazorError(RazorResources.ParseError_InlineMarkup_Blocks_Cannot_Be_Nested, new SourceLocation(characterIndex, 0, characterIndex));
        }
    }
}

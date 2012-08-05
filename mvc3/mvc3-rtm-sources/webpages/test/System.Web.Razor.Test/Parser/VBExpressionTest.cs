using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Collections.Generic;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBExpressionTest : VBHtmlCodeParserTestBase {
        private const string TestExtraKeyword = "ModelType";
        private ISet<string> KeywordSet = null;

        public override ParserBase CreateCodeParser() {
            VBCodeParser parser = new VBCodeParser();
            parser.KeywordHandlers.Add(TestExtraKeyword, _ => { Assert.Fail("Should never be called!"); return true; });
            KeywordSet = parser.TopLevelKeywords;
            return parser;
        }

        [TestMethod]
        public void AddingKeywordToRazorKeywordsListBeforeAccessingTopLevelKeywordsAddsItToTopLevelKeywords() {
            VBCodeParser parser = new VBCodeParser();
            parser.KeywordHandlers.Add(TestExtraKeyword, _ => { Assert.Fail("Should never be called!"); return true; });
            Assert.IsTrue(parser.TopLevelKeywords.Contains(TestExtraKeyword));
        }

        [TestMethod]
        public void ParseBlockCorrectlyHandlesCodeBlockInBodyOfExplicitExpressionDueToUnclosedExpression() {
            ParseBlockTest(@"(
@Code
    Dim foo = bar
End Code",
                           new ExpressionBlock(
                               new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                               new CodeSpan(String.Empty)),
                           new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                         RazorResources.BlockName_ExplicitExpression,
                                                         ")", "("), SourceLocation.Zero));
        }

        [TestMethod]
        public void ParseBlockAcceptsNonEnglishCharactersThatAreValidIdentifiers() {
            ImplicitExpressionTest("हळूँजद॔.", "हळूँजद॔");
        }

        [TestMethod]
        public void ParseBlockDoesNotTreatTypeSuffixAsTransitionToMarkup() {
            SingleSpanBlockTest(@"If foo Is Nothing Then
    Dim bar@
End If", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockDoesNotTreatXmlAxisPropertyAsTransitionToMarkup() {
            SingleSpanBlockTest(@"If foo Is Nothing Then
    Dim bar As XElement
    Dim foo = bar.<foo>
End If", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockDoesNotTreatXmlAttributePropertyAsTransitionToMarkup() {
            SingleSpanBlockTest(@"If foo Is Nothing Then
    Dim bar As XElement
    Dim foo = bar.@foo
End If", BlockType.Statement, SpanKind.Code, acceptedCharacters: AcceptedCharacters.None);
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleImplicitExpression() {
            ImplicitExpressionTest("Foo");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithDots() {
            ImplicitExpressionTest("Foo.Bar.Baz");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithParens() {
            ImplicitExpressionTest("Foo().Bar().Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithStuffInParens() {
            ImplicitExpressionTest("Foo().Bar(sdfkhj sdfksdfjs \")\" sjdfkjsdf).Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsImplicitExpressionWithCommentInParens() {
            ImplicitExpressionTest(@"Foo().Bar(sdfkhj sdfksdfjs "")"" '))))))))
sjdfkjsdf).Baz()");
        }

        [TestMethod]
        public void ParseBlockSupportsSimpleExplicitExpression() {
            ParseBlockTest("(Foo)",
                new ExpressionBlock(
                    new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan("Foo"),
                    new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void ParseBlockSupportsExplicitExpressionWithMethodCalls() {
            ParseBlockTest("(Foo(Of String).Bar(1, 2, 3).Biz)",
                new ExpressionBlock(
                    new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan("Foo(Of String).Bar(1, 2, 3).Biz"),
                    new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInStrings() {
            ParseBlockTest("(Foo(Of String).Bar(\")\").Biz)",
                new ExpressionBlock(
                    new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan("Foo(Of String).Bar(\")\").Biz"),
                    new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void ParseBlockCorrectlyTerminatesStringsWithEscapedQuotes() {
            ParseBlockTest("(Foo(Of String).Bar(\"Foo\"\"Bar)\"\"Baz\").Biz)",
                new ExpressionBlock(
                    new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                    new CodeSpan("Foo(Of String).Bar(\"Foo\"\"Bar)\"\"Baz\").Biz"),
                    new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void ParseBlockTerminatesUnterminatedStringLiteralAtNewline() {
            ParseBlockTest(@"(""foo
bar)",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"""foo
bar"),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));

        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInREMComment() {
            ParseBlockTest(@"(Foo.Bar. _
REM )
Baz()
)",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"Foo.Bar. _
REM )
Baz()
"),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void ParseBlockIgnoresBracketsInTickComment() {
            ParseBlockTest(@"(Foo.Bar. _
' )
Baz()
)",
                            new ExpressionBlock(
                                new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"Foo.Bar. _
' )
Baz()
"),
                                new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }
    }
}

using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpReservedWordsTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void NamespaceIsReservedWord() {
            RunReservedWordTest("namespace");
        }

        [TestMethod]
        public void ClassIsReservedWord() {
            RunReservedWordTest("class");
        }

        [TestMethod]
        public void LayoutIsReservedWord() {
            RunReservedWordTest("layout");
        }

        [TestMethod]
        public void NamespaceIsCaseSensitiveReservedWord() {
            RunNonReservedWordTest("Namespace");
        }

        [TestMethod]
        public void ClassIsCaseSensitiveReservedWord() {
            RunNonReservedWordTest("Class");
        }

        [TestMethod]
        public void LayoutIsCaseSensitiveReservedWord() {
            RunNonReservedWordTest("Layout");
        }

        private void RunReservedWordTest(string word) {
            ParseBlockTest(word,
                           new DirectiveBlock(
                               new MetaCodeSpan(word, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                           ),
                           new RazorError(String.Format(RazorResources.ParseError_ReservedWord, word), SourceLocation.Zero));
        }

        private void RunNonReservedWordTest(string word) {
            ParseBlockTest(word,
                           new ExpressionBlock(
                               new ImplicitExpressionSpan(word, CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false)
                           ));
        }
    }
}

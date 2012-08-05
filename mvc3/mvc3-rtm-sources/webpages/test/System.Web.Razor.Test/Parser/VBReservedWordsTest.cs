using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBReservedWordsTest : VBHtmlCodeParserTestBase {
        [TestMethod]
        public void NamespaceIsReservedWord() {
            RunReservedWordTest("Namespace");
        }

        [TestMethod]
        public void ClassIsReservedWord() {
            RunReservedWordTest("Class");
        }

        [TestMethod]
        public void LayoutIsReservedWord() {
            RunReservedWordTest("Layout");
        }

        [TestMethod]
        public void NamespaceIsCaseInsensitiveReservedWord() {
            RunReservedWordTest("nAmEsPaCe");
        }

        [TestMethod]
        public void ClassIsCaseInsensitiveReservedWord() {
            RunReservedWordTest("cLaSs");
        }

        [TestMethod]
        public void LayoutIsCaseInsensitiveReservedWord() {
            RunReservedWordTest("lAyOuT");
        }

        private void RunReservedWordTest(string word) {
            ParseBlockTest(word,
                           new DirectiveBlock(
                               new MetaCodeSpan(word, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                           ),
                           new RazorError(String.Format(RazorResources.ParseError_ReservedWord, word), SourceLocation.Zero));
        }
    }
}

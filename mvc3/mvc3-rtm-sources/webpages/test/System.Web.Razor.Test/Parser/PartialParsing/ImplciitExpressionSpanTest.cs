using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Parser.PartialParsing {
    [TestClass]
    public class ImplicitExpressionSpanTest {
        [TestMethod]
        public void SpanWithAcceptTrailingDotOffProvisionallyAcceptsEndReplacementWithTrailingDot() {
            // Arrange
            var span = new ImplicitExpressionSpan("abcd.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.None);
            var newBuffer = new StringTextBuffer("abcdef.");
            var oldBuffer = new StringTextBuffer("abcd.");
            var textChange = new TextChange(0, 5, oldBuffer, 7, newBuffer);

            // Act
            PartialParseResult result = span.ApplyChange(textChange);            

            // Assert
            Assert.AreEqual(PartialParseResult.Accepted | PartialParseResult.Provisional, result);
        }

        [TestMethod]
        public void SpanWithAcceptTrailingDotOnAcceptsEndReplacementWithTrailingDot() {
            // Arrange
            var span = new ImplicitExpressionSpan(@"abcd.
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.None);
            var newBuffer = new StringTextBuffer(@"abcdef.
");
            var oldBuffer = new StringTextBuffer(@"abcd.
");
            var textChange = new TextChange(0, 5, oldBuffer, 7, newBuffer);

            // Act
            PartialParseResult result = span.ApplyChange(textChange);

            // Assert
            Assert.AreEqual(PartialParseResult.Accepted, result);
        }

        [TestMethod]
        public void SpanWithAcceptTrailingDotOffProvisionallyAcceptsIntelliSenseReplaceWhichActuallyInsertsDot() {
            // Arrange
            var span = new ImplicitExpressionSpan("abcd", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.None);
            var newBuffer = new StringTextBuffer("abcd.");
            var oldBuffer = new StringTextBuffer("abcd");
            var textChange = new TextChange(0, 4, oldBuffer, 5, newBuffer);

            // Act
            PartialParseResult result = span.ApplyChange(textChange);

            // Assert
            Assert.AreEqual(PartialParseResult.Accepted | PartialParseResult.Provisional, result);
        }

        [TestMethod]
        public void SpanWithAcceptTrailingDotOnAcceptsIntelliSenseReplaceWhichActuallyInsertsDot() {
            // Arrange
            var span = new ImplicitExpressionSpan("abcd", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.None);
            var newBuffer = new StringTextBuffer("abcd.");
            var oldBuffer = new StringTextBuffer("abcd");
            var textChange = new TextChange(0, 4, oldBuffer, 5, newBuffer);

            // Act
            PartialParseResult result = span.ApplyChange(textChange);

            // Assert
            Assert.AreEqual(PartialParseResult.Accepted, result);
        }
    }
}

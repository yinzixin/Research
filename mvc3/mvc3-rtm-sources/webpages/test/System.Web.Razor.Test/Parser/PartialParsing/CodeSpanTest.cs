using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Parser.PartialParsing {
    [TestClass]
    public class CodeSpanTest {
        [TestMethod]
        public void ApplyChangeReturnsRejectedIfTerminatorStringNull() {
            // Arrange
            var span = new CodeSpan(new SourceLocation(0, 0, 0), "foo");
            var change = new TextChange(0, 0, new StringTextBuffer("foo"), 1, new StringTextBuffer("bfoo"));

            // Act
            PartialParseResult result = span.ApplyChange(change);

            // Assert
            Assert.AreEqual(PartialParseResult.Rejected, result);
        }

        [TestMethod]
        public void ApplyChangeReturnsRejectedWithAutoCompleteBlockIfTerminatorStringNonNullAndEditIsNewlineInsertOnFirstLine() {
            // Arrange
            var span = new CodeSpan(new SourceLocation(0, 0, 0), "foo\r\nbar\r\nbaz") {
                AutoCompleteString = "}"
            };
            var change = new TextChange(1, 0, new StringTextBuffer("foo\r\nbar\r\nbaz"), 2, new StringTextBuffer("f\r\noo\r\nbar\r\nbaz"));

            // Act
            PartialParseResult result = span.ApplyChange(change);

            // Assert
            Assert.AreEqual(PartialParseResult.Rejected | PartialParseResult.AutoCompleteBlock, result);
        }

        [TestMethod]
        public void ApplyChangeReturnsRejectedWithoutAutoCompleteBlockIfTerminatorStringNonNullAndEditIsNewlineInsertOnSecondLine() {
            // Arrange
            var span = new CodeSpan(new SourceLocation(0, 0, 0), "foo\r\nbar\r\nbaz") {
                AutoCompleteString = "}"
            };
            var change = new TextChange(8, 0, new StringTextBuffer("foo\r\nbar\r\nbaz"), 2, new StringTextBuffer("foo\r\nb\r\nar\r\nbaz"));

            // Act
            PartialParseResult result = span.ApplyChange(change);

            // Assert
            Assert.AreEqual(PartialParseResult.Rejected, result);
        }

        [TestMethod]
        public void GetAutoCompleteStringReturnsAutoCompleteString() {
            // Arrange
            var span = new CodeSpan(new SourceLocation(0, 0, 0), "foo\r\nbar\r\nbaz") {
                AutoCompleteString = "Foo"
            };
            
            // Act
            string actual = span.AutoCompleteString;

            // Assert
            Assert.AreEqual("Foo", actual);
        }
    }
}

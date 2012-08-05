using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.Razor.Test.Text {
    [TestClass]
    public class TextChangeTest {
        [TestMethod]
        public void ConstructorRequiresNonNegativeOldPosition() {
            ExceptionAssert.ThrowsArgOutOfRange(() => new TextChange(-1, 0, new Mock<ITextBuffer>().Object, 0, 0, new Mock<ITextBuffer>().Object), "oldPosition", 0, null, true);
        }

        [TestMethod]
        public void ConstructorRequiresNonNegativeNewPosition() {
            ExceptionAssert.ThrowsArgOutOfRange(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, -1, 0, new Mock<ITextBuffer>().Object), "newPosition", 0, null, true);
        }

        [TestMethod]
        public void ConstructorRequiresNonNegativeOldLength() {
            ExceptionAssert.ThrowsArgOutOfRange(() => new TextChange(0, -1, new Mock<ITextBuffer>().Object, 0, 0, new Mock<ITextBuffer>().Object), "oldLength", 0, null, true);
        }

        [TestMethod]
        public void ConstructorRequiresNonNegativeNewLength() {
            ExceptionAssert.ThrowsArgOutOfRange(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, 0, -1, new Mock<ITextBuffer>().Object), "newLength", 0, null, true);
        }

        [TestMethod]
        public void ConstructorRequiresNonNullOldBuffer() {
            ExceptionAssert.ThrowsArgNull(() => new TextChange(0, 0, null, 0, 0, new Mock<ITextBuffer>().Object), "oldBuffer");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullNewBuffer() {
            ExceptionAssert.ThrowsArgNull(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, 0, 0, null), "newBuffer");
        }

        [TestMethod]
        public void ConstructorInitializesProperties() {
            // Act
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(42, 24, oldBuffer, 1337, newBuffer);

            // Assert
            Assert.AreEqual(42, change.OldPosition);
            Assert.AreEqual(24, change.OldLength);
            Assert.AreEqual(1337, change.NewLength);
            Assert.AreSame(newBuffer, change.NewBuffer);
            Assert.AreSame(oldBuffer, change.OldBuffer);
        }

        [TestMethod]
        public void TestIsDelete() {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 1, oldBuffer, 0, newBuffer);

            // Assert
            Assert.IsTrue(change.IsDelete);
        }

        [TestMethod]
        public void TestIsInsert() {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 0, oldBuffer, 35, newBuffer);

            // Assert
            Assert.IsTrue(change.IsInsert);
        }

        [TestMethod]
        public void TestIsReplace() {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 5, oldBuffer, 10, newBuffer);

            // Assert
            Assert.IsTrue(change.IsReplace);
        }

        [TestMethod]
        public void OldTextReturnsOldSpanFromOldBuffer() {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("text");
            var textChange = new TextChange(2, 1, oldBuffer, 1, newBuffer);

            // Act
            string text = textChange.OldText;

            // Assert
            Assert.AreEqual("x", text);
        }

        [TestMethod]
        public void NewTextWithInsertReturnsChangedTextFromBuffer() {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(0, 0, oldBuffer, 3, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.AreEqual("tes", text);
        }

        [TestMethod]
        public void NewTextWithDeleteReturnsEmptyString() {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 0, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.AreEqual(String.Empty, text);
        }

        [TestMethod]
        public void NewTextWithReplaceReturnsChangedTextFromBuffer() {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(2, 2, oldBuffer, 1, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.AreEqual("s", text);
        }

        [TestMethod]
        public void ApplyChangeWithInsertedTextReturnsNewContentWithChangeApplied() {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(0, 0, oldBuffer, 3, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcd", 0);

            // Assert
            Assert.AreEqual("tesabcd", text);
        }

        [TestMethod]
        public void ApplyChangeWithRemovedTextReturnsNewContentWithChangeApplied() {
            // Arrange
            var newBuffer = new StringTextBuffer("abcdefg");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 0, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcdefg", 1);

            // Assert
            Assert.AreEqual("bcdefg", text);
        }

        [TestMethod]
        public void ApplyChangeWithReplacedTextReturnsNewContentWithChangeApplied() {
            // Arrange
            var newBuffer = new StringTextBuffer("abcdefg");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 2, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcdefg", 1);

            // Assert
            Assert.AreEqual("bcbcdefg", text);
        }

        [TestMethod]
        public void NormalizeFixesUpIntelliSenseStyleReplacements() {
            // Arrange
            var newBuffer = new StringTextBuffer("Date.");
            var oldBuffer = new StringTextBuffer("Date");
            var original = new TextChange(0, 4, oldBuffer, 5, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.AreEqual(new TextChange(4, 0, oldBuffer, 1, newBuffer), normalized);
        }

        [TestMethod]
        public void NormalizeDoesntAffectChangesWithoutCommonPrefixes() {
            // Arrange
            var newBuffer = new StringTextBuffer("DateTime.");
            var oldBuffer = new StringTextBuffer("Date.");
            var original = new TextChange(0, 5, oldBuffer, 9, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.AreEqual(original, normalized);
        }

        [TestMethod]
        public void NormalizeDoesntAffectShrinkingReplacements() {
            // Arrange
            var newBuffer = new StringTextBuffer("D");
            var oldBuffer = new StringTextBuffer("DateTime");
            var original = new TextChange(0, 8, oldBuffer, 1, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.AreEqual(original, normalized);
        }
    }
}

using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class SpanTest {
        private static readonly SourceLocation TestLocation = new SourceLocation(0, 42, 24);

        [TestMethod]
        public void ConstructorWithoutHiddenParameterInitializesHiddenPropertyToFalse() {
            // Act
            Span tok = new TransitionSpan(TestLocation, "foo");

            // Assert
            Assert.AreEqual(SpanKind.Transition, tok.Kind);
            Assert.AreEqual("foo", tok.Content);
            Assert.AreEqual(TestLocation, tok.Start);
            Assert.IsFalse(tok.Hidden);
        }

        [TestMethod]
        public void ConstructorInitializesProperties() {
            // Act
            Span tok = new TransitionSpan(TestLocation, "foo", true);

            // Assert
            Assert.AreEqual(SpanKind.Transition, tok.Kind);
            Assert.AreEqual("foo", tok.Content);
            Assert.AreEqual(TestLocation, tok.Start);
            Assert.IsTrue(tok.Hidden);
        }

        [TestMethod]
        public void OwnsChangeReturnsTrueIfSpanContentEntirelyContainsOldSpan() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(45, 3, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsTrue(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsFalseIfChangeStartsBeforeSpanStarts() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(41, 3, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsFalse(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsFalseIfChangeStartsAfterSpanEnds() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(52, 3, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsFalse(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsTrueIfChangeIsInsertionAtSpanEndAndCanGrowIsTrue() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(51, 0, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsTrue(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsFalseIfChangeIsReplacementOrDeleteAtSpanEnd() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(51, 2, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsFalse(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsFalseIfChangeIsInsertionAtSpanEndAndCanGrowIsFalse() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz", hidden: false, acceptedCharacters: AcceptedCharacters.None);
            TextChange change = new TextChange(51, 0, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsFalse(span.OwnsChange(change));
        }

        [TestMethod]
        public void OwnsChangeReturnsFalseIfOldSpanOverlapsNeighbouringSpan() {
            // Arrange
            Span span = new CodeSpan(new SourceLocation(42, 0, 42), "FooBarBaz");
            TextChange change = new TextChange(44, 50, new StringTextBuffer("BooBarBaz"), 3, new StringTextBuffer("Foo"));

            // Act/Assert
            Assert.IsFalse(span.OwnsChange(change)); 
        }

        [TestMethod]
        public void TryMergeWithReturnsFalseWhenMergingLeftIntoRightIfSpansNotAdjacent() {
            // Arrange
            Span left = new CodeSpan(SourceLocation.Zero, "Foo");
            Span right = new CodeSpan(new SourceLocation(100, 0, 0), "Bar");

            // Act/Assert
            Assert.IsFalse(left.TryMergeWith(right));
        }

        [TestMethod]
        public void TryMergeWithReturnsFalseWhenMergingRightIntoLeftIfSpansNotAdjacent() {
            // Arrange
            Span left = new CodeSpan(new SourceLocation(100, 0, 0), "Foo");
            Span right = new CodeSpan(SourceLocation.Zero, "Bar");

            // Act/Assert
            Assert.IsFalse(left.TryMergeWith(right));
        }

        [TestMethod]
        public void TryMergeWithReturnsTrueAndCorrectlyMergesWhenMergingLeftIntoRightIfSpansAreAdjacent() {
            // Arrange
            Span left = new CodeSpan(SourceLocation.Zero, "Foo");
            Span right = new CodeSpan(new SourceLocation(3, 0, 0), "Bar");

            // Act
            bool success = left.TryMergeWith(right);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("FooBar", left.Content);
            Assert.AreEqual(SourceLocation.Zero, left.Start);
        }

        [TestMethod]
        public void TryMergeWithLeavesTypeVisibilityAndTrackingModeUnchanged() {
            // Arrange
            Span left = new CodeSpan(SourceLocation.Zero, "Foo", hidden: true, acceptedCharacters: AcceptedCharacters.None);
            Span right = new MetaCodeSpan(new SourceLocation(3, 0, 0), "Bar");

            // Act
            bool success = left.TryMergeWith(right);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(SpanKind.Code, left.Kind);
            Assert.IsTrue(left.Hidden);
        }

        [TestMethod]
        public void TryMergeWithReturnsTrueAndCorrectlyMergesWhenMergingRightIntoLeftIfSpansAreAdjacent() {
            // Arrange
            Span left = new CodeSpan(new SourceLocation(3, 0, 0), "Foo");
            Span right = new CodeSpan(SourceLocation.Zero, "Bar");

            // Act
            bool success = left.TryMergeWith(right);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("BarFoo", left.Content);
            Assert.AreEqual(SourceLocation.Zero, left.Start);
        }
    }
}

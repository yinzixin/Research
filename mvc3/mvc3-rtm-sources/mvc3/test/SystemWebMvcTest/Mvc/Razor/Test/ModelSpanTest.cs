namespace System.Web.Mvc.Razor.Test {
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ModelSpanTest {
        [TestMethod]
        public void Constructor() {
            // Arrange
            var start = new SourceLocation(1, 2, 3);

            // Act
            var result = new ModelSpan(start, "content", "typeName");

            // Assert
            Assert.AreEqual("typeName", result.ModelTypeName);
            Assert.IsFalse(result.Hidden);
            Assert.AreEqual(SpanKind.Code, result.Kind);
            Assert.AreEqual(start, result.Start);
            Assert.AreEqual("content", result.Content);
            Assert.IsFalse(result.IsBlock);
        }

        [TestMethod]
        public void Equals_SameInstance_ReturnsTrue() {
            // Arrange
            var span = new ModelSpan(SourceLocation.Zero, " TypeName   ", "TypeName");

            // Act + Assert
            Assert.IsTrue(span.Equals(span));
        }

        [TestMethod]
        public void Equals_IdenticalSpan_ReturnsTrue() {
            // Arrange
            var span1 = new ModelSpan(SourceLocation.Zero, " TypeName   ", "TypeName");
            var span2 = new ModelSpan(SourceLocation.Zero, " TypeName   ", "TypeName");

            // Act + Assert
            Assert.IsTrue(span1.Equals(span2));
        }

        [TestMethod]
        public void Equals_NullValue_ReturnsFalse() {
            // Arrange
            var span = new ModelSpan(SourceLocation.Zero, "A", "A");

            // Act + Assert
            Assert.IsFalse(span.Equals(null));
        }

        [TestMethod]
        public void Equals_DifferentType_ReturnsFalse() {
            // Arrange
            var span = new ModelSpan(SourceLocation.Zero, "A", "A");

            // Act + Assert
            Assert.IsFalse(span.Equals(new object()));
        }
    }
}

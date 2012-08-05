namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IndexExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Index;
            Type expectedType = typeof(char);
            PropertyInfo expectedIndexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint = new IndexExpressionFingerprint(expectedNodeType, expectedType, expectedIndexer);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
            Assert.AreEqual(expectedIndexer, fingerprint.Indexer);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, type, indexer);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_Indexer() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, type, null /* indexer */);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Indexer).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, typeof(object), indexer);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

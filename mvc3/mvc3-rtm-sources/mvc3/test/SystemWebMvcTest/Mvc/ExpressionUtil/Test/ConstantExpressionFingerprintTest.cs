namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConstantExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Constant;
            Type expectedType = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint = new ConstantExpressionFingerprint(expectedNodeType, expectedType);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            ConstantExpressionFingerprint fingerprint2 = new ConstantExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            ConstantExpressionFingerprint fingerprint2 = new ConstantExpressionFingerprint(nodeType, typeof(string));

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

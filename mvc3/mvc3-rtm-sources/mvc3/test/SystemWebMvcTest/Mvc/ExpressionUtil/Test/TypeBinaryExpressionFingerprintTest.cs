namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TypeBinaryExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.TypeIs;
            Type expectedType = typeof(bool);
            Type expectedTypeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint = new TypeBinaryExpressionFingerprint(expectedNodeType, expectedType, expectedTypeOperand);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
            Assert.AreEqual(expectedTypeOperand, fingerprint.TypeOperand);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_TypeOperand() {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, type, typeof(string) /* typeOperand */);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by TypeOperand).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, typeof(object), typeOperand);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

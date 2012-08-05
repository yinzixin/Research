namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParameterExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Parameter;
            Type expectedType = typeof(object);
            int expectedParameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint = new ParameterExpressionFingerprint(expectedNodeType, expectedType, expectedParameterIndex);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
            Assert.AreEqual(expectedParameterIndex, fingerprint.ParameterIndex);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_Method() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, type, -1 /* parameterIndex */);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by ParameterIndex).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, typeof(string), parameterIndex);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

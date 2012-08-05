namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodCallExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Call;
            Type expectedType = typeof(string);
            MethodInfo expectedMethod = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint = new MethodCallExpressionFingerprint(expectedNodeType, expectedType, expectedMethod);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
            Assert.AreEqual(expectedMethod, fingerprint.Method);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, type, method);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_Method() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, type, null /* method */);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Method).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, typeof(object), method);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

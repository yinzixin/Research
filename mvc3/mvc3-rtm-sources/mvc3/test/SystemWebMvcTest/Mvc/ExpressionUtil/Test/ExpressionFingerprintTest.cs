namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpressionFingerprintTest {

        [TestMethod]
        public void Comparison_Equality() {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_NodeType() {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Parameter, typeof(object));

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by NodeType).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(string));

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

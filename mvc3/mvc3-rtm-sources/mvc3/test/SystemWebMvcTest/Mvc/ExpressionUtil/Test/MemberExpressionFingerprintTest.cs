namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MemberExpressionFingerprintTest {

        [TestMethod]
        public void Properties() {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.MemberAccess;
            Type expectedType = typeof(int);
            MemberInfo expectedMember = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint = new MemberExpressionFingerprint(expectedNodeType, expectedType, expectedMember);

            // Assert
            Assert.AreEqual(expectedNodeType, fingerprint.NodeType);
            Assert.AreEqual(expectedType, fingerprint.Type);
            Assert.AreEqual(expectedMember, fingerprint.Member);
        }

        [TestMethod]
        public void Comparison_Equality() {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, type, member);

            // Assert
            Assert.AreEqual(fingerprint1, fingerprint2, "Fingerprints should have been equal.");
            Assert.AreEqual(fingerprint1.GetHashCode(), fingerprint2.GetHashCode(), "Fingerprints should have been different.");
        }

        [TestMethod]
        public void Comparison_Inequality_FingerprintType() {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal ('other' is wrong type).");
        }

        [TestMethod]
        public void Comparison_Inequality_Member() {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, type, null /* member */);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Member).");
        }

        [TestMethod]
        public void Comparison_Inequality_Type() {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, typeof(object), member);

            // Assert
            Assert.AreNotEqual(fingerprint1, fingerprint2, "Fingerprints should not have been equal (differ by Type).");
        }

    }
}

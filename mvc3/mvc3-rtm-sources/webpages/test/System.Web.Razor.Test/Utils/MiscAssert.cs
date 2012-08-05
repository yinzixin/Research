using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test.Utils {
    public static class MiscAssert {
        public static void AssertBothNullOrPropertyEqual<T>(T expected, T actual, Expression<Func<T, object>> propertyExpr, string objectName) {
            // Unpack convert expressions
            Expression expr = propertyExpr.Body;
            while (expr.NodeType == ExpressionType.Convert) {
                expr = ((UnaryExpression)expr).Operand;
            }

            string propertyName = ((MemberExpression)expr).Member.Name;
            Func<T, object> property = propertyExpr.Compile();

            if (expected == null) {
                Assert.IsNull(actual, "The actual {0} was expected to be null", objectName);
            }
            else {
                Assert.IsNotNull(actual, "The actual {0} was expected to be non-null", objectName);
                Assert.AreEqual(property(expected), property(actual), "The {0} property on the expected and actual {1} were expected to be equal", propertyName, objectName);
            }
        }
    }
}

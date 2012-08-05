namespace System.Web.Mvc.ExpressionUtil.Test {
    using System;
    using System.Linq.Expressions;

    // Represents an ExpressionFingerprint that is of the wrong type.

    internal sealed class DummyExpressionFingerprint : ExpressionFingerprint {

        public DummyExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type) {
        }

    }
}

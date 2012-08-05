using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework {
    // Span.Equals will work with this
    public class TestSimpleSpan : Span {
        protected override string GetSpanTypeName() {
            return Kind.ToString();
        }

        public TestSimpleSpan(SpanKind kind, string content) : base(kind, content) { }
        public TestSimpleSpan(SpanKind kind, string content, AcceptedCharacters acceptedCharacters) : base(kind, content, hidden: false, acceptedCharacters: acceptedCharacters) { }
    }
}

using System.Collections.Generic;
using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Utils {
    public static class EventAssert {
        public static void NoMoreSpans(IEnumerator<Span> enumerator) {
            IList<Span> tokens = new List<Span>();
            while (enumerator.MoveNext()) {
                tokens.Add(enumerator.Current);
            }

            Assert.IsFalse(tokens.Count > 0, @"There are more tokens available from the source: 

{0}", FormatList(tokens));
        }

        private static string FormatList<T>(IList<T> items) {
            StringBuilder tokenString = new StringBuilder();
            foreach (T item in items) {
                tokenString.AppendLine(item.ToString()); 
            }
            return tokenString.ToString();
        }

        public static void NextSpanIs(IEnumerator<Span> enumerator, SpanKind type, string content, SourceLocation location) {
            Assert.IsTrue(enumerator.MoveNext(), "There is no next token!");
            IsSpan(enumerator.Current, type, content, location);
        }

        public static void NextSpanIs(IEnumerator<Span> enumerator, SpanKind type, string content, int actualIndex, int lineIndex, int charIndex) {
            NextSpanIs(enumerator, type, content, new SourceLocation(actualIndex, lineIndex, charIndex));
        }

        public static void IsSpan(Span tok, SpanKind type, string content, int actualIndex, int lineIndex, int charIndex) {
            IsSpan(tok, type, content, new SourceLocation(actualIndex, lineIndex, charIndex));
        }

        public static void IsSpan(Span tok, SpanKind type, string content, SourceLocation location) {
            Assert.AreEqual(content, tok.Content);
            Assert.AreEqual(type, tok.Kind);
            Assert.AreEqual(location, tok.Start);
        }
    }
}

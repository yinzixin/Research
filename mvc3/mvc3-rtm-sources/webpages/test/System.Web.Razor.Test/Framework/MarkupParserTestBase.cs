using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework {
    public abstract class MarkupParserTestBase : CodeParserTestBase {
        protected override ParserBase SelectActiveParser(ParserBase codeParser, MarkupParser markupParser) {
            return markupParser;
        }

        internal virtual void SingleSpanDocumentTest(string document, BlockType blockType, SpanKind spanType) {
            ParseDocumentTest(document, new Block(blockType, new[] { new TestSimpleSpan(spanType, document) }));
        }

        internal virtual void ParseDocumentTest(string document) {
            ParseDocumentTest(document, null, false);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot) {
            ParseDocumentTest(document, expectedRoot, false, null);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot, params RazorError[] expectedErrors) {
            ParseDocumentTest(document, expectedRoot, false, expectedErrors);
        }

        internal virtual void ParseDocumentTest(string document, bool designTimeParser) {
            ParseDocumentTest(document, null, designTimeParser);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser) {
            ParseDocumentTest(document, expectedRoot, designTimeParser, null);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser, params RazorError[] expectedErrors) {
            RunParseTest(document, parser => ((MarkupParser)parser).ParseDocument, expectedRoot, expectedErrors, designTimeParser);
        }
    }
}

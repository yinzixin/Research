using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Framework {
    public abstract class VBHtmlMarkupParserTestBase : MarkupParserTestBase {
        public override MarkupParser CreateMarkupParser() {
            return new HtmlMarkupParser();
        }

        public override ParserBase CreateCodeParser() {
            return new VBCodeParser();
        }
    }
}

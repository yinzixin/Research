using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Framework {
    public abstract class VBHtmlCodeParserTestBase : CodeParserTestBase {
        public override MarkupParser CreateMarkupParser() {
            return new HtmlMarkupParser();
        }

        public override ParserBase CreateCodeParser() {
            return new VBCodeParser();
        }
    }
}

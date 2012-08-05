using System.Web.Razor.Test.Framework;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Parser {
    internal class HtmlParserTestUtils {
        public static void RunSingleAtEscapeTest(Action<string, Block> testMethod, AcceptedCharacters lastSpanAcceptedCharacters = AcceptedCharacters.None) {
            testMethod("<foo>@@bar</foo>",
                        new MarkupBlock(
                            new MarkupSpan("<foo>"),
                            new MarkupSpan("@", hidden: true),
                            new MarkupSpan("@bar</foo>", hidden: false, acceptedCharacters: lastSpanAcceptedCharacters)
                        )
                      );
        }

        public static void RunMultiAtEscapeTest(Action<string, Block> testMethod, AcceptedCharacters lastSpanAcceptedCharacters = AcceptedCharacters.None) {
            testMethod("<foo>@@@@@bar</foo>", 
                        new MarkupBlock(
                            new MarkupSpan("<foo>"),
                            new MarkupSpan("@", hidden: true),
                            new MarkupSpan("@"),
                            new MarkupSpan("@", hidden: true),
                            new MarkupSpan("@"),
                            new ExpressionBlock(
                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ),
                            new MarkupSpan("</foo>", hidden: false, acceptedCharacters: lastSpanAcceptedCharacters)
                        ));
        }
    }
}

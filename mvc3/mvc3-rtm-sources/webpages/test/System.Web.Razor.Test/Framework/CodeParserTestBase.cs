using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework {
    public abstract class CodeParserTestBase : ParserTestBase {
        protected override ParserBase SelectActiveParser(ParserBase codeParser, MarkupParser markupParser) {
            return codeParser;
        }

        protected void ImplicitExpressionTest(string input, params RazorError[] errors) {
            ImplicitExpressionTest(input, AcceptedCharacters.NonWhiteSpace, errors);
        }

        protected void ImplicitExpressionTest(string input, AcceptedCharacters acceptedCharacters, params RazorError[] errors) {
            ImplicitExpressionTest(input, input, acceptedCharacters, errors);
        }

        protected void ImplicitExpressionTest(string input, string expected, params RazorError[] errors) {
            ImplicitExpressionTest(input, expected, AcceptedCharacters.NonWhiteSpace, errors);
        }

        protected void ImplicitExpressionTest(string input, string expected, AcceptedCharacters acceptedCharacters, params RazorError[] errors) {
            ParseBlockTest(input,
                           new ExpressionBlock(
                              new ImplicitExpressionSpan(expected, 
                                                         CSharpCodeParser.DefaultKeywords, 
                                                         acceptTrailingDot: false,
                                                         acceptedCharacters: acceptedCharacters)),
                           errors);
        }
    }
}

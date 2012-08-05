using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpToMarkupSwitchTest : CsHtmlCodeParserTestBase {
        [TestMethod]
        public void ParseBlockGivesSpacesToCodeOnAtTagTemplateTransitionInDesignTimeMode() {
            ParseBlockTest(@"Foo(    @<p>Foo</p>    )",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo(    ", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                new TemplateBlock(
                                    new MarkupBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MarkupSpan("<p>Foo</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    )
                                ),
                                new ImplicitExpressionSpan(@"    )", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ), designTimeParser: true);
        }

        [TestMethod]
        public void ParseBlockGivesSpacesToCodeOnAtColonTemplateTransitionInDesignTimeMode() {
            ParseBlockTest(@"Foo(    
@:<p>Foo</p>    
)",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Foo(    
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                new TemplateBlock(
                                    new MarkupBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan(":"),
                                        new SingleLineMarkupSpan(@"<p>Foo</p>    
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    )
                                ),
                                new ImplicitExpressionSpan(@")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ), designTimeParser: true);
        }

        [TestMethod]
        public void ParseBlockGivesSpacesToCodeOnTagTransitionInDesignTimeMode() {
            ParseBlockTest(@"{
    <p>Foo</p>    
}", 
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    "),
                                new MarkupBlock(
                                    new MarkupSpan("<p>Foo</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ), designTimeParser: true);
        }

        [TestMethod]
        public void ParseBlockGivesSpacesToCodeOnInvalidAtTagTransitionInDesignTimeMode() {
            ParseBlockTest(@"{
    @<p>Foo</p>    
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    "),
                                new MarkupBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan("<p>Foo</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ), true,
                            new RazorError(RazorResources.ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start, new SourceLocation(8, 1, 5)));
        }

        [TestMethod]
        public void ParseBlockGivesSpacesToCodeOnAtColonTransitionInDesignTimeMode() {
            ParseBlockTest(@"{
    @:<p>Foo</p>    
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    "),
                                new MarkupBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"<p>Foo</p>    
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(String.Empty),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ), designTimeParser: true);
        }

        [TestMethod]
        public void ParseBlockShouldSupportSingleLineMarkupContainingStatementBlock() {
            ParseBlockTest(@"Repeat(10,
    @: @{}
)",
                            new ExpressionBlock(
                                new ImplicitExpressionSpan(@"Repeat(10,
", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                                new TemplateBlock(
                                    new MarkupBlock(
                                        new MarkupSpan("    "),
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan(":"),
                                        new SingleLineMarkupSpan(" "),
                                        new StatementBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new CodeSpan(String.Empty),
                                            new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new SingleLineMarkupSpan(@"
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    )
                                ),
                                new ImplicitExpressionSpan(")", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                            ));
        }
        
        [TestMethod]
        public void ParseBlockShouldSupportMarkupWithoutPreceedingWhitespace() {
            ParseBlockTest(@"foreach(var file in files){


@:Baz
<br/>
<a>Foo</a>
@:Bar
}",
                            new StatementBlock(
                                new CodeSpan(@"foreach(var file in files){


"),
                                new MarkupBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Baz
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new MarkupSpan(@"<br/>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new MarkupSpan(@"<a>Foo</a>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Bar
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }
        [TestMethod]
        public void ParseBlockGivesAllWhitespaceOnSameLineExcludingPreceedingNewlineButIncludingTrailingNewLineToMarkup() {
            ParseBlockTest(@"if(foo) {
    var foo = ""After this statement there are 10 spaces"";          
    <p>
        Foo
        @bar
    </p>
    @:Hello!
    var biz = boz;
}",
                            new StatementBlock(
                                new CodeSpan(@"if(foo) {
    var foo = ""After this statement there are 10 spaces"";          
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"    <p>
        Foo
        "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("bar", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(@"
    </p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new MarkupSpan(@"    "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Hello!
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    var biz = boz;
}")));

        }

        [TestMethod]
        public void ParseBlockAllowsMarkupInIfBodyWithBraces() {
            ParseBlockTest("if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> }",
                            new StatementBlock(
                                new CodeSpan("if(foo) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Bar</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} else if(bar) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Baz</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} else {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Boz</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockAllowsMarkupInIfBodyWithBracesWithinCodeBlock() {
            ParseBlockTest("{ if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> } }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" if(foo) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Bar</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} else if(bar) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Baz</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} else {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Boz</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitch() {
            // Arrange
            ParseBlockTest(@"switch(foo) {
    case 0:
        <p>Foo</p>
        break;
    case 1:
        <p>Bar</p>
        return;
    case 2:
        {
            <p>Baz</p>
            <p>Boz</p>
        }
    default:
        <p>Biz</p>
}",
                            new StatementBlock(
                                new CodeSpan(@"switch(foo) {
    case 0:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Foo</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        break;
    case 1:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Bar</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        return;
    case 2:
        {
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"            <p>Baz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new MarkupSpan(@"            <p>Boz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        }
    default:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Biz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitchInCodeBlock() {
            // Arrange
            ParseBlockTest(@"{ switch(foo) {
    case 0:
        <p>Foo</p>
        break;
    case 1:
        <p>Bar</p>
        return;
    case 2:
        {
            <p>Baz</p>
            <p>Boz</p>
        }
    default:
        <p>Biz</p>
} }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@" switch(foo) {
    case 0:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Foo</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        break;
    case 1:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Bar</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        return;
    case 2:
        {
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"            <p>Baz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new MarkupBlock(
                                    new MarkupSpan(@"            <p>Boz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"        }
    default:
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>Biz</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracket() {
            ParseBlockTest("for(int i = 0; i < 10; i++) { <p>Foo</p> }",
                            new StatementBlock(
                                new CodeSpan("for(int i = 0; i < 10; i++) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Foo</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                            ));
        }

        [TestMethod]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracketInCodeBlock() {
            ParseBlockTest("{ for(int i = 0; i < 10; i++) { <p>Foo</p> } }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" for(int i = 0; i < 10; i++) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" <p>Foo</p> ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColon() {
            // Arrange
            ParseBlockTest(@"if(foo) { @:Bar
} zoop",
                            new StatementBlock(
                                new CodeSpan("if(foo) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Bar
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("}")));
        }

        [TestMethod]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColonInCodeBlock() {
            // Arrange
            ParseBlockTest(@"{ if(foo) { @:Bar
} } zoop",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(" if(foo) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Bar
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan("} "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTag() {
            ParseBlockTest(@"if (i > 0) { <text>;</text> }",
                            new StatementBlock(
                                new CodeSpan(@"if (i > 0) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" "),
                                    new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(";"),
                                    new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(" ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"}")));
        }

        [TestMethod]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTagInCodeBlock() {
            ParseBlockTest(@"{ if (i > 0) { <text>;</text> } }",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@" if (i > 0) {"),
                                new MarkupBlock(
                                    new MarkupSpan(" "),
                                    new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(";"),
                                    new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(" ", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"} "),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }

        [TestMethod]
        public void ParseBlockSupportsAllKindsOfImplicitMarkupInCodeBlock() {
            ParseBlockTest(@"{
    if(true) {
        @:Single Line Markup
    }
    foreach (var p in Enumerable.Range(1, 10)) {
        <text>The number is @p</text>
    }
    if(!false) {
        <p>A real tag!</p>
    }
}",
                            new StatementBlock(
                                new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                new CodeSpan(@"
    if(true) {
"),
                                new MarkupBlock(
                                    new MarkupSpan("        "),
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan(":"),
                                    new SingleLineMarkupSpan(@"Single Line Markup
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    }
    foreach (var p in Enumerable.Range(1, 10)) {
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        "),
                                    new TransitionSpan("<text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan("The number is "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("p", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new TransitionSpan("</text>", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MarkupSpan(@"
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    }
    if(!false) {
"),
                                new MarkupBlock(
                                    new MarkupSpan(@"        <p>A real tag!</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                ),
                                new CodeSpan(@"    }
"),
                                new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)));
        }
    }
}
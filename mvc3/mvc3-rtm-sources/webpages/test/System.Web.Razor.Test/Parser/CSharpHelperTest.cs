using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CSharpHelperTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseHelperCorrectlyParsesHelperWithNoSpaceInBody() {
            ParseDocumentTest("@helper Foo(){@Bar()}",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new HelperHeaderSpan("Foo(){", complete: true) { AcceptedCharacters = AcceptedCharacters.None },
                                      new StatementBlock(
                                          new CodeSpan(String.Empty),
                                          new ExpressionBlock(
                                              new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                              new ImplicitExpressionSpan("Bar()", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                          ),
                                          new CodeSpan(String.Empty)
                                      ),
                                      new HelperFooterSpan("}") { AcceptedCharacters = AcceptedCharacters.None }
                                  ),
                                  new MarkupSpan(String.Empty)
                              ));
        }

        [TestMethod]
        public void ParseHelperSupportsNoSpaceBeforeSignature() {
            ParseDocumentTest("@helper{",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("helper", hidden: false, acceptedCharacters: AcceptedCharacters.Any),
                                      new HelperHeaderSpan("{", complete: false) {
                                          AcceptedCharacters = AcceptedCharacters.Any,
                                          AutoCompleteString = "}"
                                      },
                                      new StatementBlock()
                                  )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, 
                                                           String.Format(RazorResources.ErrorComponent_Character, "{")),
                                             new SourceLocation(7, 0, 7)),
                              new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF,
                                                           "helper", "}", "{"),
                                             new SourceLocation(1, 0, 1))
                              );
        }

        [TestMethod]
        public void ParseHelperOutputsErrorButContinuesIfLParenFoundAfterHelperKeyword() {
            ParseDocumentTest("@helper () {",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new HelperHeaderSpan("() {", complete: false) {
                                          AcceptedCharacters = AcceptedCharacters.Any,
                                          AutoCompleteString = "}"
                                      },
                                      new StatementBlock()
                                  )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start,
                                                           String.Format(RazorResources.ErrorComponent_Character, "(")),
                                             new SourceLocation(7, 0, 7)),
                              new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "helper", "}", "{"),
                                             new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseHelperStatementOutputsMarkerHelperHeaderSpanOnceKeywordComplete() {
            ParseDocumentTest("@helper ",
                              new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(String.Empty, complete: false)
                                )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                             new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseHelperStatementMarksHelperSpanAsCanGrowIfMissingTrailingSpace() {
            ParseDocumentTest("@helper",
                              new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper", hidden: false, acceptedCharacters: AcceptedCharacters.Any)
                                )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                             new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseHelperStatementCapturesWhitespaceToEndOfLineIfHelperStatementMissingName() {
            ParseDocumentTest(@"@helper                       
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"                      ", complete: false)
                                ),
                                new MarkupSpan(@"
    ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start,
                                                          RazorResources.ErrorComponent_EndOfFile), new SourceLocation(7, 0, 7)));

        }

        [TestMethod]
        public void ParseHelperStatementCapturesWhitespaceToEndOfLineIfHelperStatementMissingOpenParen() {
            ParseDocumentTest(@"@helper Foo    
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo    ", complete: false)
                                ),
                                new MarkupSpan(@"
    ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_MissingCharAfterHelperName, "("), new SourceLocation(11, 0, 11)));

        }

        [TestMethod]
        public void ParseHelperStatementCapturesAllContentToEndOfFileIfHelperStatementMissingCloseParenInParameterList() {
            ParseDocumentTest(@"@helper Foo(Foo Bar
Biz
Boz",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(Foo Bar
Biz
Boz", complete: false)
                                )
                            ),
                            new RazorError(RazorResources.ParseError_UnterminatedHelperParameterList, new SourceLocation(12, 0, 12)));

        }

        [TestMethod]
        public void ParseHelperStatementCapturesWhitespaceToEndOfLineIfHelperStatementMissingOpenBraceAfterParameterList() {
            ParseDocumentTest(@"@helper Foo(string foo)    
",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo)    ", complete: false)
                                ),
                                new MarkupSpan(@"
")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_MissingCharAfterHelperParameters, "{"),
                                            new SourceLocation(29, 1, 0)));

        }

        [TestMethod]
        public void ParseHelperStatementContinuesParsingHelperUntilEOF() {
            ParseDocumentTest(@"@helper Foo(string foo) {    
    <p>Foo</p>",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo) {", complete: true) { 
                                        AcceptedCharacters = AcceptedCharacters.Any,
                                        AutoCompleteString = "}"
                                    },
                                    new StatementBlock(
                                        new CodeSpan(@"    
"),
                                        new MarkupBlock(
                                            new MarkupSpan("    <p>Foo</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(String.Empty)
                                    )
                                )
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "helper", "}", "{"),
                                           new SourceLocation(1, 0, 1)));

        }

        [TestMethod]
        public void ParseHelperStatementCorrectlyParsesHelperWithEmbeddedCode() {
            ParseDocumentTest(@"@helper Foo(string foo) {    
    <p>@foo</p>
}",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo) {", complete: true) { AcceptedCharacters = AcceptedCharacters.None },
                                    new StatementBlock(
                                        new CodeSpan(@"    
"),
                                        new MarkupBlock(
                                            new MarkupSpan(@"    <p>"),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(@"</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(String.Empty)
                                    ),
                                    new HelperFooterSpan("}") { AcceptedCharacters = AcceptedCharacters.None }
                                ),
                                new MarkupSpan(String.Empty)
                            ));

        }

        [TestMethod]
        public void ParseHelperStatementCorrectlyParsesHelperWithNewlinesBetweenCloseParenAndOpenBrace() {
            ParseDocumentTest(@"@helper Foo(string foo)



{    
    <p>@foo</p>
}",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo)



{", complete: true) { AcceptedCharacters = AcceptedCharacters.None },
                                    new StatementBlock(
                                        new CodeSpan(@"    
"),
                                        new MarkupBlock(
                                            new MarkupSpan(@"    <p>"),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(@"</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(String.Empty)
                                    ),
                                    new HelperFooterSpan("}") { AcceptedCharacters = AcceptedCharacters.None }
                                ),
                                new MarkupSpan(String.Empty)
                            ));

        }

        [TestMethod]
        public void ParseHelperStatementGivesWhitespaceAfterOpenBraceToMarkupInDesignMode() {
            ParseDocumentTest(@"@helper Foo(string foo) {    
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo) {", complete: true) {
                                        AcceptedCharacters = AcceptedCharacters.Any,
                                        AutoCompleteString = "}"
                                    },
                                    new StatementBlock(
                                        new CodeSpan(@"    
    ")
                                    )
                                )
                            ),
                            designTimeParser: true,
                            expectedErrors: new[] {
                                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, "helper", "}", "{"),
                                               new SourceLocation(1, 0, 1))
                            });

        }
        [TestMethod]
        public void ParseHelperAcceptsNestedHelpersButOutputsError() {
            ParseDocumentTest(@"@helper Foo(string foo) {
    @helper Bar(string baz) {
    }
}",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo) {", complete: true) { AcceptedCharacters = AcceptedCharacters.None },
                                    new StatementBlock(
                                        new CodeSpan(@"
    "),
                                        new HelperBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new MetaCodeSpan("helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new HelperHeaderSpan(@"Bar(string baz) {", complete: true) { AcceptedCharacters = AcceptedCharacters.None },
                                            new StatementBlock(
                                                new CodeSpan(@"
    ")
                                            ),
                                            new HelperFooterSpan("}") { AcceptedCharacters = AcceptedCharacters.None }
                                        ),
                                        new CodeSpan(@"
")
                                    ),
                                    new HelperFooterSpan("}") { AcceptedCharacters = AcceptedCharacters.None }
                                ),
                                new MarkupSpan(String.Empty)
                            ),
                            designTimeParser: true,
                            expectedErrors: new[] {
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Keyword_After_At, "helper"),
                                               new SourceLocation(32, 1, 5)),
                                new RazorError(RazorResources.ParseError_Helpers_Cannot_Be_Nested,
                                               new SourceLocation(38, 1, 11))
                            });

        }
    }
}

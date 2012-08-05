using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class VBHelperTest : VBHtmlMarkupParserTestBase {
        [TestMethod]
        public void ParseHelperOutputsErrorButContinuesIfLParenFoundAfterHelperKeyword() {
            ParseDocumentTest("@Helper ()",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new HelperHeaderSpan("()", complete: false) { 
                                          AcceptedCharacters = AcceptedCharacters.Any,
                                          AutoCompleteString = VBCodeParser.EndHelperKeyword
                                      },
                                      new StatementBlock()
                                  )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start,
                                                           String.Format(RazorResources.ErrorComponent_Character, "(")),
                                             new SourceLocation(7, 0, 7)),
                              new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Helper", "End Helper"),
                                             new SourceLocation(1, 0, 1)));
        }

        [TestMethod]
        public void ParseHelperStatementOutputsMarkerHelperHeaderSpanOnceKeywordComplete() {
            ParseDocumentTest("@Helper ",
                              new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(String.Empty, complete: false)
                                )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                             new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseHelperStatementMarksHelperSpanAsCanGrowIfMissingTrailingSpace() {
            ParseDocumentTest("@Helper",
                              new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper", hidden: false, acceptedCharacters: AcceptedCharacters.Any)
                                )
                              ),
                              new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                             new SourceLocation(7, 0, 7)));
        }

        [TestMethod]
        public void ParseHelperStatementTerminatesEarlyIfHeaderNotComplete() {
            ParseDocumentTest(@"@Helper
@Helper",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan(@"Helper
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                  ),
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("Helper", hidden: false, acceptedCharacters: AcceptedCharacters.Any)
                                  )),
                              designTimeParser: true,
                              expectedErrors: new[] {
                                  new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                                 new SourceLocation(16, 1, 7))
                                  });
        }

        [TestMethod]
        public void ParseHelperStatementTerminatesEarlyIfHeaderNotCompleteWithSpace() {
            ParseDocumentTest(@"@Helper @Helper",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan(@"Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new HelperHeaderSpan(String.Empty, complete:false)
                                  ),
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("Helper", hidden: false, acceptedCharacters: AcceptedCharacters.Any)
                                  )),
                              designTimeParser: true,
                              expectedErrors: new[] {
                                  new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, 
                                                               String.Format(RazorResources.ErrorComponent_Character, "@")),
                                                 new SourceLocation(7, 0, 7)),
                                  new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Helper_Name_Start, RazorResources.ErrorComponent_EndOfFile),
                                                 new SourceLocation(15, 0, 15))
                                  });
        }

        [TestMethod]
        public void ParseHelperStatementAllowsDifferentlyCasedEndHelperKeyword() {
            ParseDocumentTest(@"@Helper Foo()
end helper",
                              new MarkupBlock(
                                  new HelperBlock(
                                      new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                      new HelperHeaderSpan("Foo()", complete: true) { AcceptedCharacters = AcceptedCharacters.Any },
                                      new StatementBlock(
                                          new CodeSpan(@"
"),
                                          new MetaCodeSpan("end helper", hidden: false, acceptedCharacters: AcceptedCharacters.None)

                                      )
                                  ),
                                  new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ParseHelperStatementCapturesWhitespaceToEndOfLineIfHelperStatementMissingName() {
            ParseDocumentTest(@"@Helper                       
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
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
            ParseDocumentTest(@"@Helper Foo    
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo    ", complete: false)
                                ),
                                new MarkupSpan(@"
    ")
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_MissingCharAfterHelperName, "("), new SourceLocation(11, 0, 11)));

        }

        [TestMethod]
        public void ParseHelperStatementCapturesAllContentToEndOfFileIfHelperStatementMissingCloseParenInParameterList() {
            ParseDocumentTest(@"@Helper Foo(Foo Bar
Biz
Boz",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(Foo Bar
Biz
Boz", complete: false)
                                )
                            ),
                            new RazorError(RazorResources.ParseError_UnterminatedHelperParameterList, new SourceLocation(12, 0, 12)));

        }

        [TestMethod]
        public void ParseHelperStatementCapturesWhitespaceToEndOfLineIfHelperStatementMissingOpenBraceAfterParameterList() {
            ParseDocumentTest(@"@Helper Foo(foo as String)    
",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(foo as String)", complete: true) { 
                                        AcceptedCharacters = AcceptedCharacters.Any,
                                        AutoCompleteString = VBCodeParser.EndHelperKeyword
                                    },
                                    new StatementBlock(
                                        new CodeSpan(@"    
")
                                    )
                                )
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Helper", "End Helper"),
                                            new SourceLocation(1, 0, 1)));

        }

        [TestMethod]
        public void ParseHelperStatementContinuesParsingHelperUntilEOF() {
            ParseDocumentTest(@"@Helper Foo(foo as String)
    @<p>Foo</p>",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(foo as String)", complete: true) { 
                                        AcceptedCharacters = AcceptedCharacters.Any,
                                        AutoCompleteString = VBCodeParser.EndHelperKeyword
                                    },
                                    new StatementBlock(
                                        new CodeSpan(@"
"),
                                        new MarkupBlock(
                                            new MarkupSpan("    "),
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new MarkupSpan("<p>Foo</p>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(String.Empty)
                                    )
                                )
                            ),
                            new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated, "Helper", "End Helper"),
                                            new SourceLocation(1, 0, 1)));

        }

        [TestMethod]
        public void ParseHelperStatementCorrectlyParsesHelperWithEmbeddedCode() {
            ParseDocumentTest(@"@Helper Foo(foo as String, bar as String)
    @<p>@foo</p>
End Helper",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(foo as String, bar as String)", complete: true) { AcceptedCharacters = AcceptedCharacters.Any },
                                    new StatementBlock(
                                        new CodeSpan(@"
"),
                                        new MarkupBlock(
                                            new MarkupSpan("    "),
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new MarkupSpan("<p>"),
                                            new ExpressionBlock(
                                                new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                new ImplicitExpressionSpan("foo", VBCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                            ),
                                            new MarkupSpan(@"</p>
", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                        ),
                                        new CodeSpan(String.Empty),
                                        new MetaCodeSpan("End Helper", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    )
                                ),
                                new MarkupSpan(String.Empty)
                            ));

        }

        [TestMethod]
        public void ParseHelperStatementGivesWhitespaceAfterCloseParenToMarkup() {
            ParseDocumentTest(@"@Helper Foo(string foo)     
    ",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo)", complete: true) { 
                                        AcceptedCharacters = AcceptedCharacters.Any,
                                        AutoCompleteString = VBCodeParser.EndHelperKeyword
                                    },
                                    new StatementBlock(
                                        new CodeSpan(@"     
    ")
                                    )
                                )
                            ),
                            designTimeParser: true,
                            expectedErrors: new RazorError(String.Format(RazorResources.ParseError_BlockNotTerminated,
                                                                          "Helper", "End Helper"),
                                                            new SourceLocation(1, 0, 1)));

        }
        [TestMethod]
        public void ParseHelperAcceptsNestedHelpersButOutputsError() {
            ParseDocumentTest(@"@Helper Foo(string foo)
    @Helper Bar(string baz)
    End Helper
End Helper",
                            new MarkupBlock(
                                new HelperBlock(
                                    new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                    new HelperHeaderSpan(@"Foo(string foo)", complete: true) { AcceptedCharacters = AcceptedCharacters.Any },
                                    new StatementBlock(
                                        new CodeSpan(@"
    "),
                                        new HelperBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new MetaCodeSpan("Helper ", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new HelperHeaderSpan(@"Bar(string baz)", complete: true) { AcceptedCharacters = AcceptedCharacters.Any },
                                            new StatementBlock(
                                                new CodeSpan(@"
    "),
                                                new MetaCodeSpan("End Helper", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                            )
                                        ),
                                        new CodeSpan(@"
"),
                                        new MetaCodeSpan("End Helper", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                                    )
                                ),
                                new MarkupSpan(String.Empty)
                            ),
                            designTimeParser: true,
                            expectedErrors: new [] {
                                new RazorError(String.Format(RazorResources.ParseError_Unexpected_Keyword_After_At, "Helper"),
                                               new SourceLocation(30, 1, 5)),
                                new RazorError(RazorResources.ParseError_Helpers_Cannot_Be_Nested,
                                               new SourceLocation(36, 1, 11))
                            });

        }
    }
}

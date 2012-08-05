using System.Collections.Generic;
using System.Linq;
using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework {
    public abstract class ParserTestBase {
        public abstract MarkupParser CreateMarkupParser();
        public abstract ParserBase CreateCodeParser();

        public TestContext TestContext { get; set; }

        protected abstract ParserBase SelectActiveParser(ParserBase codeParser, MarkupParser markupParser);

        public virtual ParserContext CreateParserRun(LookaheadTextReader input, ParserBase codeParser, MarkupParser markupParser, ParserVisitor listener) {
            return new ParserContext(input, codeParser, markupParser, SelectActiveParser(codeParser, markupParser), listener);
        }

        internal virtual void ParseBlockTest(string document) {
            ParseBlockTest(document, null, false, new RazorError[0]);
        }

        internal virtual void ParseBlockTest(string document, bool designTimeParser) {
            ParseBlockTest(document, null, designTimeParser, new RazorError[0]);
        }

        internal virtual void ParseBlockTest(string document, params RazorError[] expectedErrors) {
            ParseBlockTest(document, false, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, bool designTimeParser, params RazorError[] expectedErrors) {
            ParseBlockTest(document, null, designTimeParser, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot) {
            ParseBlockTest(document, expectedRoot, false, null);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot, bool designTimeParser) {
            ParseBlockTest(document, expectedRoot, designTimeParser, null);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot, params RazorError[] expectedErrors) {
            ParseBlockTest(document, expectedRoot, false, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot, bool designTimeParser, params RazorError[] expectedErrors) {
            RunParseTest(document, parser => parser.ParseBlock, expectedRoot, (expectedErrors ?? new RazorError[0]).ToList(), designTimeParser);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any) {
            SingleSpanBlockTest(document, blockType, spanType, acceptedCharacters, expectedError: null);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any) {
            SingleSpanBlockTest(document, spanContent, blockType, spanType, acceptedCharacters, expectedErrors: null);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, params RazorError[] expectedError) {
            SingleSpanBlockTest(document, document, blockType, spanType, expectedError);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, params RazorError[] expectedErrors) {
            SingleSpanBlockTest(document, spanContent, blockType, spanType, AcceptedCharacters.Any, expectedErrors ?? new RazorError[0]);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters, params RazorError[] expectedError) {
            SingleSpanBlockTest(document, document, blockType, spanType, acceptedCharacters, expectedError);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters, params RazorError[] expectedErrors) {
            ParseBlockTest(document, new Block(blockType, new[] { new TestSimpleSpan(spanType, spanContent, acceptedCharacters: acceptedCharacters) }), expectedErrors ?? new RazorError[0]);
        }

        internal virtual void RunParseTest(string document, Func<ParserBase, Action> parserActionSelector, Block expectedRoot, IList<RazorError> expectedErrors, bool designTimeParser) {
            // Create the source
            using (StringTextBuffer reader = new StringTextBuffer(document)) {
                ParserResults results = null;
                try {
                    ParserBase codeParser = CreateCodeParser();
                    MarkupParser markupParser = CreateMarkupParser();
                    SyntaxTreeBuilderVisitor listener = new SyntaxTreeBuilderVisitor();
                    ParserContext context = CreateParserRun(new TextBufferReader(reader), codeParser, markupParser, listener);
                    context.DesignTimeMode = designTimeParser;

                    codeParser.Context = context;
                    markupParser.Context = context;

                    // Run the parser
                    parserActionSelector(context.ActiveParser)();
                    context.OnComplete();

                    // Collect the results
                    results = listener.Results;
                    EvaluateResults(TestContext, results, expectedRoot, expectedErrors);
                }
                finally {
                    if (TestContext != null && results != null && results.Document != null) {
                        TestContext.WriteLine(String.Empty);
                        TestContext.WriteLine("Actual Parse Tree:");
                        WriteNode(0, TestContext, results.Document);
                    }
                }
            }
        }

        private void WriteNode(int indent, TestContext testContext, SyntaxTreeNode node) {
            string content = node.ToString().Replace("\r", "\\r")
                                            .Replace("\n", "\\n")
                                            .Replace("{", "{{")
                                            .Replace("}", "}}");
            if (indent > 0) {
                content = new String('.', indent * 2) + content;
            }
            testContext.WriteLine(content);
            Block block = node as Block;
            if (block != null) {
                foreach (SyntaxTreeNode child in block.Children) {
                    WriteNode(indent + 1, testContext, child);
                }
            }
        }

        public static void EvaluateResults(TestContext context, ParserResults results, Block expectedRoot) {
            EvaluateResults(context, results, expectedRoot, null);
        }

        public static void EvaluateResults(ParserResults results, Block expectedRoot) {
            EvaluateResults(results, expectedRoot, null);
        }

        public static void EvaluateResults(ParserResults results, Block expectedRoot, IList<RazorError> expectedErrors) {
            EvaluateResults(null, results, expectedRoot, expectedErrors);
        }

        public static void EvaluateResults(TestContext context, ParserResults results, Block expectedRoot, IList<RazorError> expectedErrors) {
            EvaluateParseTree(context, results.Document, expectedRoot);
            EvaluateRazorErrors(context, results.ParserErrors, expectedErrors);
        }

        public static void EvaluateParseTree(Block actualRoot, Block expectedRoot) {
            EvaluateParseTree(null, actualRoot, expectedRoot);
        }

        public static void EvaluateParseTree(TestContext context, Block actualRoot, Block expectedRoot) {
            // Evaluate the result
            ErrorCollector collector = new ErrorCollector();
            SourceLocationTracker tracker = new SourceLocationTracker();

            // Link all the nodes
            Span first = null;
            Span previous = null;
            foreach (Span span in expectedRoot.Flatten()) {
                if(first == null) {
                    first = span;
                }
                span.Previous = previous;
                if (previous != null) {
                    previous.Next = span;
                }
                previous = span;
            }
            Span.ClearCachedStartPoints(first);

            if (expectedRoot == null) {
                Assert.IsNull(actualRoot, "Expected an empty document.  Actual: {0}", actualRoot);
            }
            else {
                Assert.IsNotNull(actualRoot, "Expected a valid document, but it was empty");
                EvaluateSyntaxTreeNode(collector, tracker, actualRoot, expectedRoot);
                if (collector.Success) {
                    if (context != null) {
                        context.WriteLine("Parse Tree Validation Succeeded:\r\n{0}", collector.Message);
                    }
                }
                else {
                    Assert.Fail("\r\n{0}", collector.Message);
                }
            }
        }

        private static void EvaluateSyntaxTreeNode(ErrorCollector collector, SourceLocationTracker tracker, SyntaxTreeNode actual, SyntaxTreeNode expected) {
            if (actual == null) {
                AddNullActualError(collector, tracker, actual, expected);
            }

            if (actual.IsBlock != expected.IsBlock) {
                AddMismatchError(collector, tracker, actual, expected);
            }
            else {
                if (expected.IsBlock) {
                    EvaluateBlock(collector, tracker, (Block)actual, (Block)expected);
                }
                else {
                    EvaluateSpan(collector, tracker, (Span)actual, (Span)expected);
                }
            }
        }

        private static void EvaluateSpan(ErrorCollector collector, SourceLocationTracker tracker, Span actual, Span expected) {
            if (!actual.Equals(expected)) {
                AddMismatchError(collector, tracker, actual, expected);
            }
            else {
                AddPassedMessage(collector, tracker, expected);
            }
        }

        private static void EvaluateBlock(ErrorCollector collector, SourceLocationTracker tracker, Block actual, Block expected) {
            if (actual.Type != expected.Type) {
                AddMismatchError(collector, tracker, actual, expected);
            }
            else {
                AddPassedMessage(collector, tracker, expected);
                using (collector.Indent()) {
                    IEnumerator<SyntaxTreeNode> expectedNodes = expected.Children.GetEnumerator();
                    IEnumerator<SyntaxTreeNode> actualNodes = actual.Children.GetEnumerator();
                    while (expectedNodes.MoveNext()) {
                        if (!actualNodes.MoveNext()) {
                            collector.AddError("{0} - FAILED :: No more elements at this node", expectedNodes.Current);
                        }
                        else {
                            EvaluateSyntaxTreeNode(collector, tracker, actualNodes.Current, expectedNodes.Current);
                        }
                    }
                    while (actualNodes.MoveNext()) {
                        collector.AddError("End of Node - FAILED :: Found Node: {0}", actualNodes.Current);
                    }
                }
            }
        }

        private static void AddPassedMessage(ErrorCollector collector, SourceLocationTracker tracker, SyntaxTreeNode expected) {
            collector.AddMessage("{0} - PASSED", expected);
        }

        private static void AddMismatchError(ErrorCollector collector, SourceLocationTracker tracker, SyntaxTreeNode actual, SyntaxTreeNode expected) {
            collector.AddError("{0} - FAILED :: Actual: {1}", expected, actual);
        }

        private static void AddNullActualError(ErrorCollector collector, SourceLocationTracker tracker, SyntaxTreeNode actual, SyntaxTreeNode expected) {
            collector.AddError("{0} - FAILED :: Actual: << Null >>", expected);
        }

        public static void EvaluateRazorErrors(IList<RazorError> actualErrors, IList<RazorError> expectedErrors) {
            EvaluateRazorErrors(null, actualErrors, expectedErrors);
        }

        public static void EvaluateRazorErrors(TestContext context, IList<RazorError> actualErrors, IList<RazorError> expectedErrors) {
            // Evaluate the errors
            if (expectedErrors == null || expectedErrors.Count == 0) {
                Assert.AreEqual(0,
                                actualErrors.Count,
                                "Expected that no errors would be raised, but the following errors were:\r\n{0}",
                                FormatErrors(actualErrors));
            }
            else {
                Assert.AreEqual(expectedErrors.Count,
                                actualErrors.Count,
                                "Expected that {0} errors would be raised, but {1} errors were.\r\nExpected Errors: \r\n{2}\r\nActual Errors: \r\n{3}",
                                expectedErrors.Count,
                                actualErrors.Count,
                                FormatErrors(expectedErrors),
                                FormatErrors(actualErrors));
                Enumerable.Zip(actualErrors, expectedErrors, (actual, expected) => {
                    Assert.AreEqual(expected, actual);
                    return String.Empty;
                }).ToList();
            }
            if (context != null) {
                context.WriteLine("Expected Errors were raised:\r\n{0}", FormatErrors(expectedErrors));
            }
        }

        public static string FormatErrors(IList<RazorError> errors) {
            if (errors == null) {
                return "\t<< No Errors >>";
            }

            StringBuilder builder = new StringBuilder();
            foreach (RazorError err in errors) {
                builder.AppendFormat("\t{0}", err);
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}

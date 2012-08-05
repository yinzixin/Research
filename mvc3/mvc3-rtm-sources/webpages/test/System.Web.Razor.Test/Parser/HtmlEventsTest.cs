using System.Web.Razor.Test.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class HtmlEventsTest : CsHtmlMarkupParserTestBase {
//        private const string SimpleDocument = "<p>Foo</p>";
//        private const string DocumentWithEmptyTag = "<p>Foo<br/>Bar</p>";

//        [TestMethod]
//        public void ParseBlockFiresTagStartedEventWhenEmptyTagRead() {
//            RunTagStartedEventTest(ParseBlock, DocumentWithEmptyTag, 1, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseBlockFiresTagFinishedEventWhenEndOfEmptyTagRead() {
//            RunTagFinishedEventTest(ParseBlock, DocumentWithEmptyTag, 1, new SourceLocation(10, 0, 10), "br", false, true, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseBlockFiresTagStartedEventWhenStartTagRead() {
//            RunTagStartedEventTest(ParseBlock, SimpleDocument, 0, SourceLocation.Zero);
//        }

//        [TestMethod]
//        public void ParseBlockFiresTagStartedEventWhenEndTagRead() {
//            RunTagStartedEventTest(ParseBlock, SimpleDocument, 1, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseBlockFiresTagFinishedEventWhenEndOfStartTagRead() {
//            RunTagFinishedEventTest(ParseBlock, SimpleDocument, 0, new SourceLocation(2, 0, 2), "p", false, false, SourceLocation.Zero);
//        }

//        [TestMethod]
//        public void ParseBlockFiresTagFinishedEventWhenEndOfEndTagRead() {
//            RunTagFinishedEventTest(ParseBlock, SimpleDocument, 1, new SourceLocation(9, 0, 9), "p", true, false, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagStartedEventWhenEmptyTagRead() {
//            RunTagStartedEventTest(ParseDocument, DocumentWithEmptyTag, 1, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagFinishedEventWhenEndOfEmptyTagRead() {
//            RunTagFinishedEventTest(ParseDocument, DocumentWithEmptyTag, 1, new SourceLocation(10, 0, 10), "br", false, true, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagStartedEventWhenStartTagRead() {
//            RunTagStartedEventTest(ParseDocument, SimpleDocument, 0, SourceLocation.Zero);
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagStartedEventWhenEndTagRead() {
//            RunTagStartedEventTest(ParseDocument, SimpleDocument, 1, new SourceLocation(6, 0, 6));
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagFinishedEventWhenEndOfStartTagRead() {
//            RunTagFinishedEventTest(ParseDocument, SimpleDocument, 0, new SourceLocation(2, 0, 2), "p", false, false, SourceLocation.Zero);
//        }

//        [TestMethod]
//        public void ParseDocumentFiresTagFinishedEventWhenEndOfEndTagRead() {
//            RunTagFinishedEventTest(ParseDocument, SimpleDocument, 1, new SourceLocation(9, 0, 9), "p", true, false, new SourceLocation(6, 0, 6));
//        }

//        private static void RunTagFinishedEventTest(Func<MarkupParser, ParserRun, IEnumerable<ParserEvent>> parseOperation,
//                                                    string document,
//                                                    int eventCount,
//                                                    SourceLocation expectedLocation,
//                                                    string expectedTag,
//                                                    bool expectedIsEndTag,
//                                                    bool expectedIsEmptyTag,
//                                                    SourceLocation expectedTagStart) {
//            Func<TagFinishedEventArgs, bool> argChecker = (args) => args.CurrentParserContext.Source.CurrentLocation == expectedLocation &&
//                                                                    args.IsEndTag == expectedIsEndTag &&
//                                                                    args.IsEmptyTag == expectedIsEmptyTag &&
//                                                                    String.Equals(args.TagName, expectedTag, StringComparison.Ordinal) &&
//                                                                    args.TagStartLocation == expectedTagStart;
//            string message = @"Expected that the following conditions would be true (expected == actual):
//    CurrentLocation: {0} == {1}
//    TagName: {2} == {3}
//    IsEndTag: {4} == {5},
//    IsEmptyTag: {6} == {7},
//    TagStartLocation: {8} == {9}";
//            Func<TagFinishedEventArgs, object[]> messageArgs = (args) => new object[] {
//                expectedLocation, args.CurrentParserContext.Source.CurrentLocation,
//                expectedTag, args.TagName,
//                expectedIsEndTag, args.IsEndTag,
//                expectedIsEmptyTag, args.IsEmptyTag,
//                expectedTagStart, args.TagStartLocation
//            };


//            RunParserEventTest<TagFinishedEventArgs>(parseOperation, document, eventCount, argChecker, message, messageArgs, (m, h) => m.TagFinished += h);
//        }

//        private static void RunTagStartedEventTest(Func<MarkupParser, ParserRun, IEnumerable<ParserEvent>> parseOperation, string document, int eventCount, SourceLocation expectedLocation) {
//            RunParserEventTest<ParserEventArgs>(parseOperation,
//                                                document,
//                                                eventCount,
//                                                (args) => args.CurrentParserContext.Source.CurrentLocation == expectedLocation,
//                                                "Expected that the event would be fired at {0} but it was fired at {1}",
//                                                (args) => new object[] { expectedLocation, args.CurrentParserContext.Source.CurrentLocation },
//                                                (m, h) => m.TagStarted += h);
//        }

//        private static void RunParserEventTest<T>(Func<MarkupParser, ParserRun, IEnumerable<ParserEvent>> parseOperation,
//                                           string document,
//                                           int eventCount,
//                                           Func<T, bool> argChecker,
//                                           string message,
//                                           Func<T, object[]> messageArgs,
//                                           Action<HtmlMarkupParser, EventHandler<T>> attachEventHandler) where T : EventArgs {
//            int counter = 0;
//            BooleanFlag flag = new BooleanFlag();
//            using (var stringReader = new StringReader(document)) {
//                var markupParser = new HtmlMarkupParser();

//                attachEventHandler(markupParser, (sender, args) => {
//                    if (flag.Value == null && counter == eventCount) {
//                        bool checkResult = argChecker(args);
//                        flag.Value = checkResult;
//                        Assert.IsTrue(checkResult, message, messageArgs(args));
//                    }
//                    counter++;
//                });

//                parseOperation(markupParser, new ParserRun(stringReader, new CSharpCodeParser(), markupParser, markupParser, "~/Foo.cshtml")).ToList();
//            }

//            Assert.IsTrue(flag.Value.HasValue, "Expected that the event would be fired but it was not");
//        }

//        private static IEnumerable<ParserEvent> ParseBlock(MarkupParser parser, ParserRun context) {
//            return parser.ParseBlock(context);
//        }

//        private static IEnumerable<ParserEvent> ParseDocument(MarkupParser parser, ParserRun context) {
//            return parser.ParseDocument(context);
//        }
    }
}

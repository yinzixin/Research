using System.IO;
using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Utils;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class ParserContextTest {
        [TestMethod]
        public void ConstructorRequiresNonNullSource() {
            var codeParser = new CSharpCodeParser();
            ExceptionAssert.ThrowsArgNull(() => new ParserContext(null, codeParser, new HtmlMarkupParser(), codeParser, new Mock<ParserVisitor>().Object), "source");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullCodeParser() {
            var codeParser = new CSharpCodeParser();
            ExceptionAssert.ThrowsArgNull(() => new ParserContext(new BufferingTextReader(TextReader.Null), null, new HtmlMarkupParser(), codeParser, new Mock<ParserVisitor>().Object), "codeParser");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullMarkupParser() {
            var codeParser = new CSharpCodeParser();
            ExceptionAssert.ThrowsArgNull(() => new ParserContext(new BufferingTextReader(TextReader.Null), codeParser, null, codeParser, new Mock<ParserVisitor>().Object), "markupParser");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullActiveParser() {
            ExceptionAssert.ThrowsArgNull(() => new ParserContext(new BufferingTextReader(TextReader.Null), new CSharpCodeParser(), new HtmlMarkupParser(), null, new Mock<ParserVisitor>().Object), "activeParser");
        }

        [TestMethod]
        public void ConstructorThrowsIfActiveParserIsNotCodeOrMarkupParser() {
            ExceptionAssert.ThrowsArgumentException(() => new ParserContext(new BufferingTextReader(TextReader.Null), new CSharpCodeParser(), new HtmlMarkupParser(), new CSharpCodeParser(), new Mock<ParserVisitor>().Object),
                                                    "activeParser",
                                                    RazorResources.ActiveParser_Must_Be_Code_Or_Markup_Parser);
        }

        [TestMethod]
        public void ConstructorAcceptsActiveParserIfIsSameAsEitherCodeOrMarkupParser() {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            new ParserContext(new BufferingTextReader(TextReader.Null), codeParser, markupParser, codeParser, new Mock<ParserVisitor>().Object);
            new ParserContext(new BufferingTextReader(TextReader.Null), codeParser, markupParser, markupParser, new Mock<ParserVisitor>().Object);
        }

        [TestMethod]
        public void ConstructorRequiresNonNullListener() {
            var codeParser = new CSharpCodeParser();
            ExceptionAssert.ThrowsArgNull(() => new ParserContext(new BufferingTextReader(TextReader.Null), codeParser, new HtmlMarkupParser(), codeParser, null), "visitor");
        }

        [TestMethod]
        public void ConstructorInitializesProperties() {
            // Arrange
            LookaheadTextReader expectedBuffer = new BufferingTextReader(TextReader.Null);
            CSharpCodeParser expectedCodeParser = new CSharpCodeParser();
            HtmlMarkupParser expectedMarkupParser = new HtmlMarkupParser();
            ParserVisitor expectedListener = new Mock<ParserVisitor>().Object;

            // Act
            ParserContext context = new ParserContext(expectedBuffer, expectedCodeParser, expectedMarkupParser, expectedCodeParser, expectedListener);

            // Assert
            Assert.IsInstanceOfType(context.Source, typeof(BufferingTextReader));
            Assert.AreSame(expectedBuffer, context.Source);
            Assert.AreSame(expectedCodeParser, context.CodeParser);
            Assert.AreSame(expectedMarkupParser, context.MarkupParser);
            Assert.AreSame(expectedCodeParser, context.ActiveParser);
            Assert.AreSame(expectedListener, context.Visitor);
        }

        [TestMethod]
        public void CurrentCharacterReturnsCurrentCharacterInTextBuffer() {
            // Arrange
            ParserContext context = SetupTestRun("bar", b => b.Read());

            // Act
            char actual = context.CurrentCharacter;

            // Assert
            Assert.AreEqual('a', actual);
        }

        [TestMethod]
        public void CurrentCharacterReturnsNulCharacterIfTextBufferAtEOF() {
            // Arrange
            ParserContext context = SetupTestRun("bar", b => b.ReadToEnd());

            // Act
            char actual = context.CurrentCharacter;

            // Assert
            Assert.AreEqual('\0', actual);
        }

        [TestMethod]
        public void EndOfFileReturnsFalseIfTextBufferNotAtEOF() {
            // Arrange
            ParserContext context = SetupTestRun("bar");

            // Act/Assert
            Assert.IsFalse(context.EndOfFile);
        }

        [TestMethod]
        public void EndOfFileReturnsTrueIfTextBufferAtEOF() {
            // Arrange
            ParserContext context = SetupTestRun("bar", b => b.ReadToEnd());

            // Act/Assert
            Assert.IsTrue(context.EndOfFile);
        }

        [TestMethod]
        public void HaveSpanReturnsFalseWhenContentBufferEmpty() {
            // Arrange
            ParserContext context = SetupTestRun("foo");

            // Act/Assert
            Assert.IsFalse(context.HaveContent);
        }

        [TestMethod]
        public void HaveSpanReturnsTrueWhenPrimaryBufferNonEmpty() {
            // Arrange
            ParserContext context = SetupTestRun("foo");

            // Act
            context.ContentBuffer.Append("foo");

            // Assert
            Assert.IsTrue(context.HaveContent);
        }

        [TestMethod]
        public void HaveSpanReturnsTrueWhenInTemporaryAndTemporaryBufferNonEmpty() {
            // Arrange
            ParserContext context = SetupTestRun("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("foo");

                // Assert
                Assert.IsTrue(context.HaveContent);
            }
        }

        [TestMethod]
        public void HaveSpanReturnsFalseIfPrimaryBufferEmptyAndTemporaryBufferRejected() {
            // Arrange
            ParserContext context = SetupTestRun("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("foo");
            }

            // Assert
            Assert.IsFalse(context.HaveContent);
        }

        [TestMethod]
        public void DisposingTemporaryBufferWithoutAcceptingCausesSourceToBacktrackToPointAtWhichTemporaryBufferWasStarted() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            char expected = context.CurrentCharacter;

            // Act
            using (context.StartTemporaryBuffer()) {
                context.Source.ReadToEnd();
            }

            // Assert
            Assert.IsFalse(context.InTemporaryBuffer);
            Assert.AreEqual(expected, context.CurrentCharacter);
        }

        [TestMethod]
        public void CallingRejectTemporaryBufferCausesSourceToBacktrackToPointAtWhichTemporaryBufferWasStarted() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            char expected = context.CurrentCharacter;

            // Act
            using (context.StartTemporaryBuffer()) {
                context.Source.ReadToEnd();
                context.RejectTemporaryBuffer();

                // Assert
                Assert.IsFalse(context.InTemporaryBuffer);
                Assert.AreEqual(expected, context.CurrentCharacter);
            }
        }

        [TestMethod]
        public void CallingRejectTemporaryBufferCausesBacktrackAndFurthurMovementsOccurOutsideLookaheadContext() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            
            // Act
            using (context.StartTemporaryBuffer()) {
                context.Source.ReadToEnd();
                context.RejectTemporaryBuffer();
                context.Source.Read(); // Now outside of lookahead!

                // Assert
                Assert.IsFalse(context.InTemporaryBuffer);
                Assert.AreEqual('r', context.CurrentCharacter);
            }
        }

        [TestMethod]
        public void CallingAcceptTemporaryBufferCausesEndOfLookaheadWithNoBacktrack() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", r=> r.Read());

            // Act
            using (context.StartTemporaryBuffer()) {
                context.Source.Read();
                context.Source.Read();
                context.AcceptTemporaryBuffer();
            }

            // Assert
            Assert.AreEqual('b', context.CurrentCharacter);
        }

        [TestMethod]
        public void TemporaryBufferHidesPrimaryBufferWhileEnabled() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartTemporaryBuffer()) {

                // Assert
                Assert.AreNotEqual("foo", context.ContentBuffer.ToString());
            }
            Assert.AreEqual("foo", context.ContentBuffer.ToString());
        }

        [TestMethod]
        public void ContentAppendedToTemporaryBufferIsLostWhenRejectedByDisposing() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("bar");
                using (context.StartTemporaryBuffer()) {
                    context.ContentBuffer.Append("baz");
                }
                context.AcceptTemporaryBuffer();
            }
            Assert.IsFalse(context.InTemporaryBuffer);
            Assert.AreEqual("foobar", context.ContentBuffer.ToString());
        }

        [TestMethod]
        public void ContentAppendedToTemporaryBufferIsLostWhenRejectedByExplicitMethod() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("bar");
                using (context.StartTemporaryBuffer()) {
                    context.ContentBuffer.Append("baz");
                    context.RejectTemporaryBuffer();
                }
                context.AcceptTemporaryBuffer();
                Assert.IsFalse(context.InTemporaryBuffer);
                Assert.AreEqual("foobar", context.ContentBuffer.ToString());
            }
        }

        [TestMethod]
        public void ContentAppendedToTemporaryBufferIsCopiedToPreviousBufferWhenAccepted() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("bar");
                using (context.StartTemporaryBuffer()) {
                    context.ContentBuffer.Append("baz");
                    context.AcceptTemporaryBuffer();
                }
                context.AcceptTemporaryBuffer();
                Assert.IsFalse(context.InTemporaryBuffer);
                Assert.AreEqual("foobarbaz", context.ContentBuffer.ToString());
            }
        }

        [TestMethod]
        public void ContentFromAcceptedTemporaryBufferIsNotAffectedByDisposalOfTheTemporaryBuffer() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("bar");
                context.AcceptTemporaryBuffer();
            }
            Assert.AreEqual("foobar", context.ContentBuffer.ToString());
        }

        [TestMethod]
        public void StartSpanClearsContentBuffer() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            context.ResetBuffers();

            // Assert
            Assert.AreEqual(0, context.ContentBuffer.Length);
        }

        [TestMethod]
        public void StartSpanSetsCurrentSpanStartToCurrentLocation() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            
            // Act
            context.ResetBuffers();

            // Assert
            Assert.AreEqual(new SourceLocation(1, 0, 1), context.CurrentSpanStart);
        }

        [TestMethod]
        public void StartSpanRejectsTemporaryBuffer() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            using (context.StartTemporaryBuffer()) {
                context.ContentBuffer.Append("bar");

                // Act
                context.ResetBuffers();

                // Assert
                Assert.IsFalse(context.InTemporaryBuffer);
                Assert.AreEqual(0, context.ContentBuffer.Length);
            }
        }

        [TestMethod]
        public void StartBlockSignalsNewBlockWithSpecifiedType() {
            // Arrange
            Mock<ParserVisitor> mockListener = new Mock<ParserVisitor>();
            ParserContext context = SetupTestRun("phoo", mockListener.Object);

            // Act
            context.StartBlock(BlockType.Expression, outputCurrentBufferAsTransition: true);

            // Assert
            mockListener.Verify(l => l.VisitStartBlock(BlockType.Expression));
        }

        [TestMethod]
        public void EndBlockSignalsEndOfBlockLastStartedWithEndBlock() {
            // Arrange
            Mock<ParserVisitor> mockListener = new Mock<ParserVisitor>();
            ParserContext context = SetupTestRun("phoo", mockListener.Object);

            // Act
            context.StartBlock(BlockType.Expression, outputCurrentBufferAsTransition: true);
            context.EndBlock();

            // Assert
            mockListener.Verify(l => l.VisitStartBlock(BlockType.Expression));
            mockListener.Verify(l => l.VisitEndBlock(BlockType.Expression));
        }

        [TestMethod]
        public void OutputSpanSignalsNewSpanWithStartPointAndContentFromBuffersAndSpecifiedSpanType() {
            // Arrange
            Mock<ParserVisitor> mockListener = new Mock<ParserVisitor>();
            ParserContext context = SetupTestRun("phoo", mockListener.Object);
            Span actualSpan = null;

            using (context.StartBlock(BlockType.Functions, outputCurrentBufferAsTransition: true)) {
                context.ContentBuffer.Append(context.Source.ReadToEnd());
            
                mockListener.Setup(l => l.VisitSpan(It.IsAny<Span>()))
                            .Callback<Span>(s => actualSpan = s);

                // Act
                context.OutputSpan(CodeSpan.Create(context));
            }

            // Assert
            EventAssert.IsSpan(actualSpan, SpanKind.Code, "phoo", new SourceLocation(0, 0, 0));
        }

        [TestMethod]
        public void ResetBuffersClearsContentBuffer() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());
            context.ContentBuffer.Append("foo");

            // Act
            using (context.StartBlock(BlockType.Functions, outputCurrentBufferAsTransition: true)) {
                context.ResetBuffers();
            }

            // Assert
            Assert.AreEqual(0, context.ContentBuffer.Length);
        }

        [TestMethod]
        public void ResetBuffersSetsCurrentSpanStartToCurrentLocation() {
            // Arrange
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read());

            // Act
            context.ResetBuffers();

            // Assert
            Assert.AreEqual(new SourceLocation(1, 0, 1), context.CurrentSpanStart);
        }

        [TestMethod]
        public void SwitchActiveParserSetsMarkupParserAsActiveIfCodeParserCurrentlyActive() {
            // Arrange
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read(), codeParser, markupParser, codeParser, new Mock<ParserVisitor>().Object);
            Assert.AreSame(codeParser, context.ActiveParser);

            // Act
            context.SwitchActiveParser();

            // Assert
            Assert.AreSame(markupParser, context.ActiveParser);
        }

        [TestMethod]
        public void SwitchActiveParserSetsCodeParserAsActiveIfMarkupParserCurrentlyActive() {
            // Arrange
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            ParserContext context = SetupTestRun("barbazbiz", b => b.Read(), codeParser, markupParser, markupParser, new Mock<ParserVisitor>().Object);
            Assert.AreSame(markupParser, context.ActiveParser);

            // Act
            context.SwitchActiveParser();

            // Assert
            Assert.AreSame(codeParser, context.ActiveParser);
        }

        private ParserContext SetupTestRun(string document) {
            return SetupTestRun(document, b => { });
        }

        private ParserContext SetupTestRun(string document, ParserVisitor listener) {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            return SetupTestRun(document, b => { }, codeParser, markupParser, codeParser, listener);
        }
        
        private ParserContext SetupTestRun(string document, Action<TextReader> positioningAction) {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            return SetupTestRun(document, positioningAction, codeParser, markupParser, codeParser, new Mock<ParserVisitor>().Object);
        }

        private ParserContext SetupTestRun(string document, Action<TextReader> positioningAction, ParserBase codeParser, MarkupParser markupParser, ParserBase activeParser, ParserVisitor listener) {
            ParserContext context = new ParserContext(new BufferingTextReader(new StringReader(document)), codeParser, markupParser, activeParser, listener);
            positioningAction(context.Source);
            context.ResetBuffers();
            return context;
        }
    }
}

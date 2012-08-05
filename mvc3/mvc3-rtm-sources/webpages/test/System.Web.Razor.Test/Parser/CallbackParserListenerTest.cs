using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class CallbackParserListenerTest {
        [TestMethod]
        public void ListenerConstructedWithSpanCallbackCallsCallbackOnEndSpan() {
            RunOnEndSpanTest(callback => new CallbackVisitor(callback));
        }

        [TestMethod]
        public void ListenerConstructedWithSpanCallbackDoesNotThrowOnStartBlockEndBlockOrError() {
            // Arrange
            Action<Span> spanCallback = _ => { };
            CallbackVisitor listener = new CallbackVisitor(spanCallback);

            // Act/Assert
            listener.VisitStartBlock(BlockType.Functions);
            listener.VisitError(new RazorError("Error", SourceLocation.Zero));
            listener.VisitEndBlock(BlockType.Functions);
        }

        [TestMethod]
        public void ListenerConstructedWithSpanAndErrorCallbackCallsCallbackOnEndSpan() {
            RunOnEndSpanTest(spanCallback => new CallbackVisitor(spanCallback, _ => { }));
        }

        [TestMethod]
        public void ListenerConstructedWithSpanAndErrorCallbackCallsCallbackOnError() {
            RunOnErrorTest(errorCallback => new CallbackVisitor(_ => {}, errorCallback));
        }

        [TestMethod]
        public void ListenerConstructedWithAllCallbacksCallsCallbackOnEndSpan() {
            RunOnEndSpanTest(spanCallback => new CallbackVisitor(spanCallback, _ => { }, _ => { }, _ => { }));
        }

        [TestMethod]
        public void ListenerConstructedWithAllCallbacksCallsCallbackOnError() {
            RunOnErrorTest(errorCallback => new CallbackVisitor(_ => { }, errorCallback, _ => { }, _ => { }));
        }

        [TestMethod]
        public void ListenerConstructedWithAllCallbacksCallsCallbackOnStartBlock() {
            RunOnStartBlockTest(startBlockCallback => new CallbackVisitor(_ => { }, _ => { }, startBlockCallback, _ => { }));
        }

        [TestMethod]
        public void ListenerConstructedWithAllCallbacksCallsCallbackOnEndBlock() {
            RunOnEndBlockTest(endBlockCallback => new CallbackVisitor(_ => { }, _ => { }, _ => { }, endBlockCallback));
        }

        [TestMethod]
        public void ListenerCallsOnEndSpanCallbackUsingSynchronizationContextIfSpecified() {
            RunSyncContextTest((Span)new TransitionSpan(new SourceLocation(42, 42, 42), "@@@@"), 
                               spanCallback => new CallbackVisitor(spanCallback, _ => {}, _ => {}, _ => {}),
                               (listener, expected) => listener.VisitSpan(expected));
        }

        [TestMethod]
        public void ListenerCallsOnStartBlockCallbackUsingSynchronizationContextIfSpecified() {
            RunSyncContextTest(BlockType.Template,
                               startBlockCallback => new CallbackVisitor(_ => { }, _ => { }, startBlockCallback, _ => { }),
                               (listener, expected) => listener.VisitStartBlock(expected));
        }

        [TestMethod]
        public void ListenerCallsOnEndBlockCallbackUsingSynchronizationContextIfSpecified() {
            RunSyncContextTest(BlockType.Template,
                               endBlockCallback => new CallbackVisitor(_ => { }, _ => { }, _ => { }, endBlockCallback),
                               (listener, expected) => listener.VisitEndBlock(expected));
        }

        [TestMethod]
        public void ListenerCallsOnErrorCallbackUsingSynchronizationContextIfSpecified() {
            RunSyncContextTest(new RazorError("Bar", new SourceLocation(42, 42, 42)),
                               errorCallback => new CallbackVisitor(_ => { }, errorCallback, _ => { }, _ => { }),
                               (listener, expected) => listener.VisitError(expected));
        }

        private static void RunSyncContextTest<T>(T expected, Func<Action<T>, CallbackVisitor> ctor, Action<CallbackVisitor, T> call) {
            // Arrange
            Mock<SynchronizationContext> mockContext = new Mock<SynchronizationContext>();
            mockContext.Setup(c => c.Post(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()))
                       .Callback<SendOrPostCallback, object>((callback, state) =>  {
                           callback(expected);
                       });

            // Act/Assert
            RunCallbackTest<T>(default(T), callback => {
                CallbackVisitor listener = ctor(callback);
                listener.SynchronizationContext = mockContext.Object;
                return listener;
            }, call, (original, actual) => {
                Assert.AreNotEqual(original, actual);
                Assert.AreEqual(expected, actual);
            });
        }

        private static void RunOnStartBlockTest(Func<Action<BlockType>, CallbackVisitor> ctor, Action<BlockType, BlockType> verifyResults = null) {
            RunCallbackTest(BlockType.Markup, ctor, (listener, expected) => listener.VisitStartBlock(expected), verifyResults);
        }

        private static void RunOnEndBlockTest(Func<Action<BlockType>, CallbackVisitor> ctor, Action<BlockType, BlockType> verifyResults = null) {
            RunCallbackTest(BlockType.Markup, ctor, (listener, expected) => listener.VisitEndBlock(expected), verifyResults);
        }

        private static void RunOnErrorTest(Func<Action<RazorError>, CallbackVisitor> ctor, Action<RazorError, RazorError> verifyResults = null) {
            RunCallbackTest(new RazorError("Foo", SourceLocation.Zero), ctor, (listener, expected) => listener.VisitError(expected), verifyResults);
        }

        private static void RunOnEndSpanTest(Func<Action<Span>, CallbackVisitor> ctor, Action<Span, Span> verifyResults = null) {
            RunCallbackTest((Span)new CodeSpan(SourceLocation.Zero, "Foo"), ctor, (listener, expected) => listener.VisitSpan(expected), verifyResults);
        }

        private static void RunCallbackTest<T>(T expected, Func<Action<T>, CallbackVisitor> ctor, Action<CallbackVisitor, T> call, Action<T, T> verifyResults = null) {
            // Arrange
            object actual = null;
            Action<T> callback = t => actual = t;

            CallbackVisitor listener = ctor(callback);

            // Act
            call(listener, expected);
            
            // Assert
            if (verifyResults == null) {
                Assert.AreEqual(expected, actual);
            }
            else {
                verifyResults(expected, (T)actual);
            }
        }
    }
}

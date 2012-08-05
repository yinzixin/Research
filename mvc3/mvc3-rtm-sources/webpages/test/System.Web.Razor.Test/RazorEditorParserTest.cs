using System.Web.Razor.Resources;
using System.Web.Razor.Text;
using System.Threading;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;
using System.Web.Razor.Parser;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Test.Utils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test {
    [TestClass]
    public class ClientPageParserTest {
        private static readonly TestFile SimpleCSHTMLDocument = TestFile.Create("DesignTime.Simple.cshtml");
        private static readonly TestFile SimpleCSHTMLDocumentGenerated = TestFile.Create("DesignTime.Simple.txt");
        private const string TestLinePragmaFileName = "C:\\This\\Path\\Is\\Just\\For\\Line\\Pragmas.cshtml";

        [TestMethod]
        public void ConstructorRequiresNonNullHost() {
            ExceptionAssert.ThrowsArgNull(() => new RazorEditorParser(null, TestLinePragmaFileName),
                                          "host");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullPhysicalPath() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new RazorEditorParser(CreateHost(), null),
                                          "sourceFileName");
        }

        [TestMethod]
        public void ConstructorRequiresNonEmptyPhysicalPath() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new RazorEditorParser(CreateHost(), String.Empty),
                                          "sourceFileName");
        }

        [TestMethod]
        public void CheckForStructureChangesRequiresNonNullBufferInChange() {
            TextChange change = new TextChange();
            ExceptionAssert.ThrowsArgumentException(() => new RazorEditorParser(CreateHost(),
                                                                     "C:\\Foo.cshtml").CheckForStructureChanges(change),
                                                    "change",
                                                    String.Format(RazorResources.Structure_Member_CannotBeNull, "Buffer", "TextChange"));

        }

        private static RazorEngineHost CreateHost() {
            return new RazorEngineHost(new CSharpRazorCodeLanguage()) { DesignTimeMode = true };
        }

        [TestMethod]
        public void CheckForStructureChangesStartsReparseAndFiresDocumentParseCompletedEventIfNoAdditionalChangesQueued() {
            // Arrange
            using (RazorEditorParser parser = CreateClientParser()) {
                StringTextBuffer input = new StringTextBuffer(SimpleCSHTMLDocument.ReadAllText());

                DocumentParseCompleteEventArgs capturedArgs = null;
                ManualResetEventSlim parseComplete = new ManualResetEventSlim(false);

                parser.DocumentParseComplete += (sender, args) => {
                    capturedArgs = args;
                    parseComplete.Set();
                };

                // Act
                parser.CheckForStructureChanges(new TextChange(0, 0, new StringTextBuffer(String.Empty), input.Length, input));

                // Assert
                MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait);
                Assert.AreEqual(SimpleCSHTMLDocumentGenerated.ReadAllText(),
                                MiscUtils.StripRuntimeVersion(
                                    capturedArgs.GeneratorResults.GeneratedCode.GenerateCode<CSharpCodeProvider>()
                                ));
            }
        }

        [TestMethod]
        public void CheckForStructureChangesStartsFullReparseIfChangeOverlapsMultipleSpans() {
            // Arrange
            RazorEditorParser parser = new RazorEditorParser(CreateHost(), TestLinePragmaFileName);
            ITextBuffer original = new StringTextBuffer("Foo @bar Baz");
            ITextBuffer changed = new StringTextBuffer("Foo @bap Daz");
            TextChange change = new TextChange(7, 3, original, 3, changed);

            ManualResetEventSlim parseComplete = new ManualResetEventSlim();
            int parseCount = 0;
            parser.DocumentParseComplete += (sender, args) => {
                Interlocked.Increment(ref parseCount);
                parseComplete.Set();
            };

            Assert.AreEqual(PartialParseResult.Rejected, parser.CheckForStructureChanges(new TextChange(0, 0, new StringTextBuffer(String.Empty), 12, original)));
            MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait); // Wait for the parse to finish
            parseComplete.Reset();

            // Act
            PartialParseResult result = parser.CheckForStructureChanges(change);
            
            // Assert
            Assert.AreEqual(PartialParseResult.Rejected, result);
            MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait);
            Assert.AreEqual(2, parseCount);
        }

        private static void SetupMockWorker(Mock<RazorEditorParser> parser, ManualResetEventSlim running) {
            SetupMockWorker(parser, running, null);
        }

        private static void SetupMockWorker(Mock<RazorEditorParser> parser, ManualResetEventSlim running, Action<int> backgroundThreadIdReceiver) {
            parser.Setup(p => p.ProcessChange(It.IsAny<CancellationToken>(), It.IsAny<TextChange>(), It.IsAny<Block>()))
                  .Callback<CancellationToken, TextChange>((token, change) => {
                      if (backgroundThreadIdReceiver != null) {
                          backgroundThreadIdReceiver(Thread.CurrentThread.ManagedThreadId);
                      }
                      running.Set();
                      while (!token.IsCancellationRequested) { }
                  });
        }

        private TextChange CreateDummyChange() {
            return new TextChange(0, 0, new StringTextBuffer(String.Empty), 3, new StringTextBuffer("foo"));
        }

        private static Mock<RazorEditorParser> CreateMockParser() {
            return new Mock<RazorEditorParser>(CreateHost(), TestLinePragmaFileName) { CallBase = true };
        }

        private static RazorEditorParser CreateClientParser() {
            return new RazorEditorParser(CreateHost(), TestLinePragmaFileName);
        }
    }
}

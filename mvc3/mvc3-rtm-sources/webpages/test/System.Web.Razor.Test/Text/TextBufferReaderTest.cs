using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Text {
    [TestClass]
    public class TextBufferReaderTest : LookaheadTextReaderTestBase {
        protected override LookaheadTextReader CreateReader(string testString) {
            return new TextBufferReader(new StringTextBuffer(testString));
        }

        [TestMethod]
        public void ConstructorRequiresNonNullTextBuffer() {
            ExceptionAssert.ThrowsArgNull(() => new TextBufferReader(null), "buffer");
        }

        [TestMethod]
        public void PeekReturnsCurrentCharacterWithoutAdvancingPosition() {
            RunPeekTest("abc", peekAt: 2);
        }

        [TestMethod]
        public void PeekReturnsNegativeOneAtEndOfSourceReader() {
            RunPeekTest("abc", peekAt: 3);
        }

        [TestMethod]
        public void ReadReturnsCurrentCharacterAndAdvancesToNextCharacter() {
            RunReadTest("abc", readAt: 2);
        }

        [TestMethod]
        public void EndingLookaheadReturnsReaderToPreviousLocation() {
            RunLookaheadTest("abcdefg", "abcb",
                             Read,
                             Lookahead(
                                Read,
                                Read),
                             Read);
        }

        [TestMethod]
        public void MultipleLookaheadsCanBePerformed() {
            RunLookaheadTest("abcdefg", "abcbcdc",
                             Read,
                             Lookahead(
                                Read,
                                Read),
                             Read,
                             Lookahead(
                                Read,
                                Read),
                             Read);
        }

        [TestMethod]
        public void LookaheadsCanBeNested() {
            RunLookaheadTest("abcdefg", "abcdefebc",
                             Read,          // Appended: "a" Reader: "bcdefg"
                             Lookahead(     // Reader: "bcdefg"
                                Read,       // Appended: "b" Reader: "cdefg";
                                Read,       // Appended: "c" Reader: "defg";
                                Read,       // Appended: "d" Reader: "efg";
                                Lookahead(  // Reader: "efg"
                                    Read,   // Appended: "e" Reader: "fg";
                                    Read    // Appended: "f" Reader: "g";
                                ),          // Reader: "efg"
                                Read        // Appended: "e" Reader: "fg";
                            ),              // Reader: "bcdefg"
                            Read,           // Appended: "b" Reader: "cdefg";
                            Read);          // Appended: "c" Reader: "defg";
        }

        [TestMethod]
        public void SourceLocationIsZeroWhenInitialized() {
            RunSourceLocationTest("abcdefg", SourceLocation.Zero, checkAt: 0);
        }

        [TestMethod]
        public void CharacterAndAbsoluteIndicesIncreaseAsCharactersAreRead() {
            RunSourceLocationTest("abcdefg", new SourceLocation(4, 0, 4), checkAt: 4);
        }

        [TestMethod]
        public void CharacterAndAbsoluteIndicesIncreaseAsSlashRInTwoCharacterNewlineIsRead() {
            RunSourceLocationTest("f\r\nb", new SourceLocation(2, 0, 2), checkAt: 2);
        }

        [TestMethod]
        public void CharacterIndexResetsToZeroAndLineIndexIncrementsWhenSlashNInTwoCharacterNewlineIsRead() {
            RunSourceLocationTest("f\r\nb", new SourceLocation(3, 1, 0), checkAt: 3);
        }

        [TestMethod]
        public void CharacterIndexResetsToZeroAndLineIndexIncrementsWhenSlashRInSingleCharacterNewlineIsRead() {
            RunSourceLocationTest("f\rb", new SourceLocation(2, 1, 0), checkAt: 2);
        }

        [TestMethod]
        public void CharacterIndexResetsToZeroAndLineIndexIncrementsWhenSlashNInSingleCharacterNewlineIsRead() {
            RunSourceLocationTest("f\nb", new SourceLocation(2, 1, 0), checkAt: 2);
        }

        [TestMethod]
        public void EndingLookaheadResetsRawCharacterAndLineIndexToValuesWhenLookaheadBegan() {
            RunEndLookaheadUpdatesSourceLocationTest();
        }

        [TestMethod]
        public void OnceBufferingBeginsReadsCanContinuePastEndOfBuffer() {
            RunLookaheadTest("abcdefg", "abcbcdefg",
                             Read,
                             Lookahead(Read(2)),
                             Read(2),
                             ReadToEnd);
        }

        [TestMethod]
        public void DisposeDisposesSourceReader() {
            RunDisposeTest(r => r.Dispose());
        }

        [TestMethod]
        public void CloseDisposesSourceReader() {
            RunDisposeTest(r => r.Close());
        }

        [TestMethod]
        public void ReadWithBufferSupportsLookahead() {
            RunBufferReadTest((reader, buffer, index, count) => reader.Read(buffer, index, count));
        }

        [TestMethod]
        public void ReadBlockSupportsLookahead() {
            RunBufferReadTest((reader, buffer, index, count) => reader.ReadBlock(buffer, index, count));
        }

        [TestMethod]
        public void ReadLineSupportsLookahead() {
            RunReadUntilTest(r => r.ReadLine(), expectedRaw: 8, expectedChar: 0, expectedLine: 2);
        }

        [TestMethod]
        public void ReadToEndSupportsLookahead() {
            RunReadUntilTest(r => r.ReadToEnd(), expectedRaw: 11, expectedChar: 3, expectedLine: 2);
        }

        [TestMethod]
        public void ReadLineMaintainsCorrectCharacterPosition() {
            RunSourceLocationTest("abc\r\ndef", new SourceLocation(5, 1, 0), r => r.ReadLine());
        }

        [TestMethod]
        public void ReadToEndWorksAsInNormalTextReader() {
            RunReadToEndTest();
        }

        [TestMethod]
        public void CancelBacktrackStopsNextEndLookaheadFromBacktracking() {
            RunLookaheadTest("abcdefg", "abcdefg",
                             Lookahead(
                                Read(2),
                                CancelBacktrack
                             ),
                             ReadToEnd);
        }

        [TestMethod]
        public void CancelBacktrackThrowsInvalidOperationExceptionIfCalledOutsideOfLookahead() {
            RunCancelBacktrackOutsideLookaheadTest();
        }

        [TestMethod]
        public void CancelBacktrackOnlyCancelsBacktrackingForInnermostNestedLookahead() {
            RunLookaheadTest("abcdefg", "abcdabcdefg",
                             Lookahead(
                                Read(2),
                                Lookahead(
                                    Read,
                                    CancelBacktrack
                                ),
                                Read
                             ),
                             ReadToEnd);
        }

        private static void RunDisposeTest(Action<LookaheadTextReader> triggerAction) {
            // Arrange
            StringTextBuffer source = new StringTextBuffer("abcdefg");
            LookaheadTextReader reader = new TextBufferReader(source);

            // Act
            triggerAction(reader);

            // Assert
            Assert.IsTrue(source.Disposed);
        }
    }
}

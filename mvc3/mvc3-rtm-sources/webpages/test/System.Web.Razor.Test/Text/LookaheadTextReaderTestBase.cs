using System.Web.Razor.Resources;
using System.Web.Razor.Text;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Text {
    public abstract class LookaheadTextReaderTestBase {
        protected abstract LookaheadTextReader CreateReader(string testString);

        protected void RunPeekTest(string input, int peekAt = 0) {
            RunPeekOrReadTest(input, peekAt, false);
        }

        protected void RunReadTest(string input, int readAt = 0) {
            RunPeekOrReadTest(input, readAt, true);
        }

        protected void RunSourceLocationTest(string input, SourceLocation expected, int checkAt = 0) {
            RunSourceLocationTest(input, expected, r => AdvanceReader(checkAt, r));
        }

        protected void RunSourceLocationTest(string input, SourceLocation expected, Action<LookaheadTextReader> readerAction) {
            // Arrange
            LookaheadTextReader reader = CreateReader(input);
            readerAction(reader);

            // Act
            SourceLocation actual = reader.CurrentLocation;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        protected void RunEndLookaheadUpdatesSourceLocationTest() {
            SourceLocation? expectedLocation = null;
            SourceLocation? actualLocation = null;

            RunLookaheadTest("abc\r\ndef\r\nghi", null,
                             Read(6),
                             CaptureSourceLocation(s => expectedLocation = s),
                             Lookahead(Read(6)),
                             CaptureSourceLocation(s => actualLocation = s));
            // Assert
            Assert.AreEqual(expectedLocation.Value.AbsoluteIndex, actualLocation.Value.AbsoluteIndex, "The reader did not correctly reset the RawIndex when Lookahead finished");
            Assert.AreEqual(expectedLocation.Value.CharacterIndex, actualLocation.Value.CharacterIndex, "The reader did not correctly reset the CharacterIndex when Lookahead finished");
            Assert.AreEqual(expectedLocation.Value.LineIndex, actualLocation.Value.LineIndex, "The reader did not correctly reset the LineIndex when Lookahead finished");
        }

        protected void RunReadToEndTest() {
            // Arrange
            LookaheadTextReader reader = CreateReader("abcdefg");

            // Act
            string str = reader.ReadToEnd();

            // Assert
            Assert.AreEqual("abcdefg", str, "The text read from the reader did not match the text provided to it");
        }

        protected void RunCancelBacktrackOutsideLookaheadTest() {
            // Arrange
            LookaheadTextReader reader = CreateReader("abcdefg");

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => reader.CancelBacktrack(), RazorResources.DoNotBacktrack_Must_Be_Called_Within_Lookahead);
        }

        protected Action<StringBuilder, LookaheadTextReader> CaptureSourceLocation(Action<SourceLocation> capture) {
            return (_, reader) => {
                capture(reader.CurrentLocation);
            };
        }

        protected Action<StringBuilder, LookaheadTextReader> Read(int count) {
            return (builder, reader) => {
                for (int i = 0; i < count; i++) {
                    Read(builder, reader);
                }
            };
        }

        protected void Read(StringBuilder builder, LookaheadTextReader reader) {
            builder.Append((char)reader.Read());
        }

        protected void ReadToEnd(StringBuilder builder, LookaheadTextReader reader) {
            builder.Append(reader.ReadToEnd());
        }

        protected void CancelBacktrack(StringBuilder builder, LookaheadTextReader reader) {
            reader.CancelBacktrack();
        }

        protected Action<StringBuilder, LookaheadTextReader> Lookahead(params Action<StringBuilder, LookaheadTextReader>[] readerCommands) {
            return (builder, reader) => {
                using (reader.BeginLookahead()) {
                    RunAll(readerCommands, builder, reader);
                }
            };
        }

        protected void RunLookaheadTest(string input, string expected, params Action<StringBuilder, LookaheadTextReader>[] readerCommands) {
            // Arrange
            StringBuilder builder = new StringBuilder();
            using (LookaheadTextReader reader = CreateReader(input)) {
                RunAll(readerCommands, builder, reader);
            }

            if (expected != null) {
                Assert.AreEqual(expected, builder.ToString(), "The reader did not backtrack correctly");
            }
        }

        protected void RunReadUntilTest(Func<LookaheadTextReader, string> readMethod, int expectedRaw, int expectedChar, int expectedLine) {
            // Arrange
            LookaheadTextReader reader = CreateReader("a\r\nbcd\r\nefg");

            reader.Read(); // Reader: "\r\nbcd\r\nefg"
            reader.Read(); // Reader: "\nbcd\r\nefg"
            reader.Read(); // Reader: "bcd\r\nefg"

            // Act
            string read = null;
            SourceLocation actualLocation;
            using (reader.BeginLookahead()) {
                read = readMethod(reader);
                actualLocation = reader.CurrentLocation;
            }

            // Assert
            Assert.AreEqual(3, reader.CurrentLocation.AbsoluteIndex, "The reader did not correctly restore the raw index when backtracking");
            Assert.AreEqual(0, reader.CurrentLocation.CharacterIndex, "The reader did not correctly restore the character index when backtracking");
            Assert.AreEqual(1, reader.CurrentLocation.LineIndex, "The reader did not correctly restore the line index when backtracking");
            Assert.AreEqual(expectedRaw, actualLocation.AbsoluteIndex, "The reader did not correctly advance the raw index");
            Assert.AreEqual(expectedChar, actualLocation.CharacterIndex, "The reader did not correctly advance the character index");
            Assert.AreEqual(expectedLine, actualLocation.LineIndex, "The reader did not correctly advance the line index");
            Assert.AreEqual('b', reader.Peek(), "The reader did not correctly backtrack to the appropriate character");
            Assert.AreEqual(read, readMethod(reader));
        }

        protected void RunBufferReadTest(Func<LookaheadTextReader, char[], int, int, int> readMethod) {
            // Arrange
            LookaheadTextReader reader = CreateReader("abcdefg");

            reader.Read(); // Reader: "bcdefg"

            // Act
            char[] buffer = new char[4];
            int read = -1;
            SourceLocation actualLocation;
            using (reader.BeginLookahead()) {
                read = readMethod(reader, buffer, 0, 4);
                actualLocation = reader.CurrentLocation;
            }

            // Assert
            Assert.AreEqual("bcde", new String(buffer), "The reader did not fill the buffer with the correct characters");
            Assert.AreEqual(4, read, "The reader did not report the correct number of read characters");
            Assert.AreEqual(5, actualLocation.AbsoluteIndex, "The reader did not correctly advance the raw index");
            Assert.AreEqual(5, actualLocation.CharacterIndex, "The reader did not correctly advance the character index");
            Assert.AreEqual(0, actualLocation.LineIndex, "The reader did not correctly advance the line index");
            Assert.AreEqual(1, reader.CurrentLocation.CharacterIndex, "The reader did not correctly restore the character index when backtracking");
            Assert.AreEqual(0, reader.CurrentLocation.LineIndex, "The reader did not correctly restore the line index when backtracking");
            Assert.AreEqual('b', reader.Peek(), "The reader did not correctly backtrack to the appropriate character");
        }

        private static void RunAll(Action<StringBuilder, LookaheadTextReader>[] readerCommands, StringBuilder builder, LookaheadTextReader reader) {
            foreach (Action<StringBuilder, LookaheadTextReader> readerCommand in readerCommands) {
                readerCommand(builder, reader);
            }
        }

        private void RunPeekOrReadTest(string input, int offset, bool isRead) {
            using (LookaheadTextReader reader = CreateReader(input)) {
                AdvanceReader(offset, reader);

                // Act
                int? actual = null;
                if (isRead) {
                    actual = reader.Read();
                }
                else {
                    actual = reader.Peek();
                }

                if (actual == null) {
                    Assert.Inconclusive("Actual value was never set?!");
                }

                // Asserts
                AssertReaderValueCorrect(actual.Value, input, offset, "Peek");

                if (isRead) {
                    AssertReaderValueCorrect(reader.Peek(), input, offset + 1, "Read");
                }
                else {
                    Assert.AreEqual(actual, reader.Peek(), "Peek moved the reader to the next character!");
                }
            }
        }

        private static void AdvanceReader(int offset, LookaheadTextReader reader) {
            for (int i = 0; i < offset; i++) { reader.Read(); }
        }

        private void AssertReaderValueCorrect(int actual, string input, int expectedOffset, string methodName) {
            if (expectedOffset < input.Length) {
                Assert.AreEqual(input[expectedOffset], actual, "{0} did not return the expected value", methodName);
            }
            else {
                Assert.AreEqual(-1, actual, "{0} did not return -1 at the end of the input", methodName);
            }
        }
    }
}

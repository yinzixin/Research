using System.IO;
using System.Web.Razor.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Text {
    [TestClass]
    public class TextReaderExtensionsTest {
        [TestMethod]
        public void ReadUntilWithCharThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, '@'), "reader");
        }

        [TestMethod]
        public void ReadUntilInclusiveWithCharThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, '@', inclusive: true), "reader");
        }

        [TestMethod]
        public void ReadUntilWithMultipleTerminatorsThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, '/', '>'), "reader");
        }

        [TestMethod]
        public void ReadUntilInclusiveWithMultipleTerminatorsThrowsArgNullIfReaderNull() {
            // NOTE: Using named parameters would be difficult here, hence the inline comment
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, /* inclusive */ true, '/', '>'), "reader");
        }

        [TestMethod]
        public void ReadUntilWithPredicateThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, c => true), "reader");
        }

        [TestMethod]
        public void ReadUntilInclusiveWithPredicateThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(null, c => true, inclusive: true), "reader");
        }

        [TestMethod]
        public void ReadUntilWithPredicateThrowsArgExceptionIfPredicateNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(new StringReader("Foo"), (Predicate<char>)null), "condition");
        }

        [TestMethod]
        public void ReadUntilInclusiveWithPredicateThrowsArgExceptionIfPredicateNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadUntil(new StringReader("Foo"), (Predicate<char>)null, inclusive: true), "condition");
        }

        [TestMethod]
        public void ReadWhileWithPredicateThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadWhile(null, c => true), "reader");
        }

        [TestMethod]
        public void ReadWhileInclusiveWithPredicateThrowsArgNullIfReaderNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadWhile(null, c => true, inclusive: true), "reader");
        }

        [TestMethod]
        public void ReadWhileWithPredicateThrowsArgNullIfPredicateNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadWhile(new StringReader("Foo"), (Predicate<char>)null), "condition");
        }

        [TestMethod]
        public void ReadWhileInclusiveWithPredicateThrowsArgNullIfPredicateNull() {
            ExceptionAssert.ThrowsArgNull(() => TextReaderExtensions.ReadWhile(new StringReader("Foo"), (Predicate<char>)null, inclusive: true), "condition");
        }

        [TestMethod]
        public void ReadUntilWithCharReadsAllTextUpToSpecifiedCharacterButNotPast() {
            RunReaderTest("foo bar baz @biz", "foo bar baz ", '@', r => r.ReadUntil('@'));
        }

        [TestMethod]
        public void ReadUntilWithCharWithInclusiveFlagReadsAllTextUpToSpecifiedCharacterButNotPastIfInclusiveFalse() {
            RunReaderTest("foo bar baz @biz", "foo bar baz ", '@', r => r.ReadUntil('@', inclusive: false));
        }

        [TestMethod]
        public void ReadUntilWithCharWithInclusiveFlagReadsAllTextUpToAndIncludingSpecifiedCharacterIfInclusiveTrue() {
            RunReaderTest("foo bar baz @biz", "foo bar baz @", 'b', r => r.ReadUntil('@', inclusive: true));
        }

        [TestMethod]
        public void ReadUntilWithCharReadsToEndIfSpecifiedCharacterNotFound() {
            RunReaderTest("foo bar baz", "foo bar baz", -1, r => r.ReadUntil('@'));
        }

        [TestMethod]
        public void ReadUntilWithMultipleTerminatorsReadsUntilAnyTerminatorIsFound() {
            RunReaderTest("<bar/>", "<bar", '/', r => r.ReadUntil('/', '>'));
        }

        [TestMethod]
        public void ReadUntilWithMultipleTerminatorsHonorsInclusiveFlagWhenFalse() {
            // NOTE: Using named parameters would be difficult here, hence the inline comment
            RunReaderTest("<bar/>", "<bar", '/', r => r.ReadUntil(/* inclusive */ false, '/', '>'));
        }

        [TestMethod]
        public void ReadUntilWithMultipleTerminatorsHonorsInclusiveFlagWhenTrue() {
            // NOTE: Using named parameters would be difficult here, hence the inline comment
            RunReaderTest("<bar/>", "<bar/", '>', r => r.ReadUntil(/* inclusive */ true, '/', '>'));
        }

        [TestMethod]
        public void ReadUntilWithPredicateStopsWhenPredicateIsTrue() {
            RunReaderTest("foo bar baz 0 zoop zork zoink", "foo bar baz ", '0', r => r.ReadUntil(c => Char.IsDigit(c)));
        }

        [TestMethod]
        public void ReadUntilWithPredicateHonorsInclusiveFlagWhenFalse() {
            RunReaderTest("foo bar baz 0 zoop zork zoink", "foo bar baz ", '0', r => r.ReadUntil(c => Char.IsDigit(c), inclusive: false));
        }

        [TestMethod]
        public void ReadUntilWithPredicateHonorsInclusiveFlagWhenTrue() {
            RunReaderTest("foo bar baz 0 zoop zork zoink", "foo bar baz 0", ' ', r => r.ReadUntil(c => Char.IsDigit(c), inclusive: true));
        }

        [TestMethod]
        public void ReadWhileWithPredicateStopsWhenPredicateIsFalse() {
            RunReaderTest("012345a67890", "012345", 'a', r => r.ReadWhile(c => Char.IsDigit(c)));
        }

        [TestMethod]
        public void ReadWhileWithPredicateHonorsInclusiveFlagWhenFalse() {
            RunReaderTest("012345a67890", "012345", 'a', r => r.ReadWhile(c => Char.IsDigit(c), inclusive: false));
        }

        [TestMethod]
        public void ReadWhileWithPredicateHonorsInclusiveFlagWhenTrue() {
            RunReaderTest("012345a67890", "012345a", '6', r => r.ReadWhile(c => Char.IsDigit(c), inclusive: true));
        }

        private static void RunReaderTest(string testString, string expectedOutput, int expectedPeek, Func<TextReader, string> action) {
            // Arrange
            StringReader reader = new StringReader(testString);

            // Act
            string read = action(reader);

            // Assert
            Assert.AreEqual(expectedOutput, read);

            if (expectedPeek == -1) {
                Assert.IsTrue(reader.Peek() == -1, "Expected that the reader would be positioned at the end of the input stream");
            }
            else {
                Assert.AreEqual((char)expectedPeek, (char)reader.Peek());
            }
        }
    }
}

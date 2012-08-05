using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Parser;

namespace System.Web.Razor.Test.Parser.PartialParsing {
    [TestClass]
    public class CSharpPartialParsingTest : PartialParsingTestBase<CSharpRazorCodeLanguage> {
        [TestMethod]
        public void ImplicitExpressionProvisionallyAcceptsDeleteOfIdentifierPartsIfDotRemains() {
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @User.Name baz");
            RunPartialParseTest(new TextChange(10, 4, old, 0, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("User.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" baz")),
                                additionalFlags: PartialParseResult.Provisional);
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsDeleteOfIdentifierPartsIfSomeOfIdentifierRemains() {
            StringTextBuffer changed = new StringTextBuffer("foo @Us baz");
            StringTextBuffer old = new StringTextBuffer("foo @User baz");
            RunPartialParseTest(new TextChange(7, 2, old, 0, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("Us", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" baz")));
        }

        [TestMethod]
        public void ImplicitExpressionProvisionallyAcceptsMultipleInsertionIfItCausesIdentifierExpansionAndTrailingDot() {
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @U baz");
            RunPartialParseTest(new TextChange(6, 0, old, 4, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("User.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" baz")),
                                additionalFlags: PartialParseResult.Provisional);
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsMultipleInsertionIfItOnlyCausesIdentifierExpansion() {
            StringTextBuffer changed = new StringTextBuffer("foo @barbiz baz");
            StringTextBuffer old = new StringTextBuffer("foo @bar baz");
            RunPartialParseTest(new TextChange(8, 0, old, 3, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("barbiz", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" baz")));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsIdentifierExpansionAtEndOfNonWhitespaceCharacters() {
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @food
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo
}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                                new MarkupBlock(
                                    new StatementBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(@"
    "),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan(@"food", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new CodeSpan(@"
"),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsIdentifierAfterDotAtEndOfNonWhitespaceCharacters() {
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @foo.d
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo.
}");
            RunPartialParseTest(new TextChange(13, 0, old, 1, changed),
                                new MarkupBlock(
                                    new StatementBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(@"
    "),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan(@"foo.d", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new CodeSpan(@"
"),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsDotAtEndOfNonWhitespaceCharacters() {
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @foo.
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo
}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                                new MarkupBlock(
                                    new StatementBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(@"
    "),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan(@"foo.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new CodeSpan(@"
"),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionRejectsChangeWhichWouldHaveBeenAcceptedIfLastChangeWasProvisionallyAcceptedOnDifferentSpan() {
            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo @bar"), 1, new StringTextBuffer("foo @foo. @bar"));
            TextChange charTyped = new TextChange(14, 0, new StringTextBuffer("foo @foo. @bar"), 1, new StringTextBuffer("foo @foo. @barb"));
            TestParserManager manager = CreateParserManager();
            manager.InitializeWithDocument(dotTyped.OldBuffer);

            // Apply the dot change
            Assert.AreEqual(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

            // Act (apply the identifier start char change)
            PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

            // Assert
            Assert.AreEqual(PartialParseResult.Rejected, result, "The change was accepted despite the previous change being provisionally accepted!");
            Assert.IsFalse(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
            ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                                             new MarkupBlock(
                                                 new MarkupSpan("foo "),
                                                 new ExpressionBlock(
                                                     new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                     new ImplicitExpressionSpan("foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                                 ),
                                                 new MarkupSpan(". "),
                                                 new ExpressionBlock(
                                                     new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                     new ImplicitExpressionSpan("barb", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                                 ),
                                                 new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsIdentifierTypedAfterDotIfLastChangeWasProvisionalAcceptanceOfDot() {
            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo bar"), 1, new StringTextBuffer("foo @foo. bar"));
            TextChange charTyped = new TextChange(9, 0, new StringTextBuffer("foo @foo. bar"), 1, new StringTextBuffer("foo @foo.b bar"));
            TestParserManager manager = CreateParserManager();
            manager.InitializeWithDocument(dotTyped.OldBuffer);

            // Apply the dot change
            Assert.AreEqual(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

            // Act (apply the identifier start char change)
            PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

            // Assert
            Assert.AreEqual(PartialParseResult.Accepted, result, "The change was not fully accepted!");
            Assert.IsFalse(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
            ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                                             new MarkupBlock(
                                                 new MarkupSpan("foo "),
                                                 new ExpressionBlock(
                                                     new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                                     new ImplicitExpressionSpan("foo.b", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                                 ),
                                                 new MarkupSpan(" bar")));
        }

        [TestMethod]
        public void ImplicitExpressionProvisionallyAcceptsDotAfterIdentifierInMarkup() {
            StringTextBuffer changed = new StringTextBuffer("foo @foo. bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("foo.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" bar")),
                                additionalFlags: PartialParseResult.Provisional);
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsAdditionalIdentifierCharactersIfEndOfSpanIsIdentifier() {
            StringTextBuffer changed = new StringTextBuffer("foo @foob bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                                new MarkupBlock(
                                    new MarkupSpan("foo "),
                                    new ExpressionBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new ImplicitExpressionSpan("foob", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                    ),
                                    new MarkupSpan(" bar")));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsAdditionalIdentifierStartCharactersIfEndOfSpanIsDot() {
            StringTextBuffer changed = new StringTextBuffer("@{@foo.b}");
            StringTextBuffer old = new StringTextBuffer("@{@foo.}");
            RunPartialParseTest(new TextChange(7, 0, old, 1, changed),
                                new MarkupBlock(
                                    new StatementBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(String.Empty),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan("foo.b", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new CodeSpan(String.Empty),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionAcceptsDotIfTrailingDotsAreAllowed() {
            StringTextBuffer changed = new StringTextBuffer("@{@foo.}");
            StringTextBuffer old = new StringTextBuffer("@{@foo}");
            RunPartialParseTest(new TextChange(6, 0, old, 1, changed),
                                new MarkupBlock(
                                    new StatementBlock(
                                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new MetaCodeSpan("{", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                        new CodeSpan(String.Empty),
                                        new ExpressionBlock(
                                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                                            new ImplicitExpressionSpan("foo.", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                                        ),
                                        new CodeSpan(String.Empty),
                                        new MetaCodeSpan("}", hidden: false, acceptedCharacters: AcceptedCharacters.None)),
                                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfIfKeywordTyped() {
            RunTypeKeywordTest("if");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfDoKeywordTyped() {
            RunTypeKeywordTest("do");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfTryKeywordTyped() {
            RunTypeKeywordTest("try");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForKeywordTyped() {
            RunTypeKeywordTest("for");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForEachKeywordTyped() {
            RunTypeKeywordTest("foreach");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfWhileKeywordTyped() {
            RunTypeKeywordTest("while");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSwitchKeywordTyped() {
            RunTypeKeywordTest("switch");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLockKeywordTyped() {
            RunTypeKeywordTest("lock");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfUsingKeywordTyped() {
            RunTypeKeywordTest("using");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSectionKeywordTyped() {
            RunTypeKeywordTest("section");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfInheritsKeywordTyped() {
            RunTypeKeywordTest("inherits");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfHelperKeywordTyped() {
            RunTypeKeywordTest("helper");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfFunctionsKeywordTyped() {
            RunTypeKeywordTest("functions");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfNamespaceKeywordTyped() {
            RunTypeKeywordTest("namespace");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfClassKeywordTyped() {
            RunTypeKeywordTest("class");
        }

        [TestMethod]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLayoutKeywordTyped() {
            RunTypeKeywordTest("layout");
        }
    }
}

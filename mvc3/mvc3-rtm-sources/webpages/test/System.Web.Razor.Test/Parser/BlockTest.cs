using System.Collections.Generic;
using System.Web.Razor.Parser;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser {
    [TestClass]
    public class BlockTest {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConstructorSetsPropertyValue() {
            // Arrange
            IEnumerable<SyntaxTreeNode> contents = new SyntaxTreeNode[0];

            // Act
            Block block = new Block(BlockType.Expression, contents);

            // Assert
            Assert.AreEqual(BlockType.Expression, block.Type);
            Assert.AreSame(contents, block.Children);
        }

        [TestMethod]
        public void ConstructorSetsParentPointerOnChildren() {
            // Arrange
            SyntaxTreeNode[] contents = new SyntaxTreeNode[2] {
                new Block(BlockType.Comment, new SyntaxTreeNode[0]),
                new CodeSpan(String.Empty)
            };

            // Act
            Block block = new Block(BlockType.Expression, contents);

            // Assert
            Assert.AreSame(block, contents[0].Parent);
            Assert.AreSame(block, contents[1].Parent);
        }

        [TestMethod]
        public void LocateOwnerReturnsSpanWhichReturnsTrueForOwnsSpan() {
            // Arrange
            Span expected = new CodeSpan(new SourceLocation(5, 0, 5), "bar");
            Block block = new MarkupBlock(new SyntaxTreeNode[] {
                new MarkupSpan(SourceLocation.Zero, "Foo "),
                new StatementBlock(new SyntaxTreeNode[] {
                    new TransitionSpan(new SourceLocation(4, 0, 4), "@"),
                    expected,
                }),
                new MarkupSpan(new SourceLocation(8,0,8), " Baz")
            });
            TextChange change = new TextChange(6, 1, new StringTextBuffer("Foo @bar Baz"), 1, new StringTextBuffer("Foo @bor Baz"));

            // Act
            Span actual = block.LocateOwner(change);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void LocateOwnerReturnsNullIfNoSpanReturnsTrueForOwnsSpan() {
            // Arrange
            Block block = new Block(BlockType.Markup, new SyntaxTreeNode[] {
                new MarkupSpan(SourceLocation.Zero, "Foo "),
                new StatementBlock(new SyntaxTreeNode[] {
                    new TransitionSpan(new SourceLocation(4, 0, 4), "@"),
                    new CodeSpan(new SourceLocation(5, 0, 5), "bar"),
                }),
                new MarkupSpan(new SourceLocation(8,0,8), " Baz")
            });
            TextChange change = new TextChange(128, 1, new StringTextBuffer("Foo @bar Baz"), 1, new StringTextBuffer("Foo @bor Baz"));

            // Act
            Span actual = block.LocateOwner(change);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void LocateOwnerReturnsNullIfChangeCrossesMultipleSpans() {
            // Arrange
            Block block = new Block(BlockType.Markup, new SyntaxTreeNode[] {
                new MarkupSpan(SourceLocation.Zero, "Foo "),
                new StatementBlock(new SyntaxTreeNode[] {
                    new TransitionSpan(new SourceLocation(4, 0, 4), "@"),
                    new CodeSpan(new SourceLocation(5, 0, 5), "bar"),
                }),
                new MarkupSpan(new SourceLocation(8,0,8), " Baz")
            });
            TextChange change = new TextChange(4, 10, new StringTextBuffer("Foo @bar Baz"), 10, new StringTextBuffer("Foo @bor Baz"));

            // Act
            Span actual = block.LocateOwner(change);

            // Assert
            Assert.IsNull(actual);
        }
    }
}

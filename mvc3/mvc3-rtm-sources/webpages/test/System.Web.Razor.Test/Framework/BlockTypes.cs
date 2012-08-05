using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework {
    // The product code doesn't need this, but having subclasses for the block types makes tests much cleaner :)

    public class StatementBlock : Block {
        public StatementBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public StatementBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Statement, children) { }
    }

    public class DirectiveBlock : Block {
        public DirectiveBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public DirectiveBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Directive, children) { }
    }

    public class FunctionsBlock : Block {
        public FunctionsBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public FunctionsBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Functions, children) { }
    }

    public class ExpressionBlock : Block {
        public ExpressionBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public ExpressionBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Expression, children) { }
    }

    public class HelperBlock : Block {
        public HelperBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public HelperBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Helper, children) { }
    }

    public class MarkupBlock : Block {
        public MarkupBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public MarkupBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Markup, children) { }
    }

    public class SectionBlock : Block {
        public SectionBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public SectionBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Section, children) { }
    }

    public class TemplateBlock : Block {
        public TemplateBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public TemplateBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Template, children) { }
    }

    public class CommentBlock : Block {
        public CommentBlock(params SyntaxTreeNode[] children) : this((IEnumerable<SyntaxTreeNode>)children) { }
        public CommentBlock(IEnumerable<SyntaxTreeNode> children) : base(BlockType.Comment, children) { }
    }
}

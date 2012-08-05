namespace System.Web.Mvc.Razor.Test {
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Razor;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MvcVBRazorCodeGeneratorTest {
        [TestMethod]
        public void Constructor() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();

            // Act
            var generator = new MvcVBRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.vbhtml", mockHost.Object);

            // Assert
            Assert.AreEqual("FooClass", generator.ClassName);
            Assert.AreEqual("Root.Namespace", generator.RootNamespaceName);
            Assert.AreEqual("SomeSourceFile.vbhtml", generator.SourceFileName);
            Assert.AreSame(mockHost.Object, generator.Host);
        }

        [TestMethod]
        public void OnEndSpan_ProcessesModelSpans() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());
            mockHost.SetupGet(h => h.DefaultBaseClass).Returns("System.Web.Mvc.WebViewPage");
            var generator = new MvcVBRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.vbhtml", mockHost.Object);
            Span modelSpan = new ModelSpan(SourceLocation.Zero, "MyFooModel", "MyFooModel");
            generator.VisitStartBlock(BlockType.Statement);

            // Act
            generator.VisitSpan(modelSpan);

            // Assert
            Assert.AreEqual(1, generator.GeneratedClass.BaseTypes.Count);
            var baseType = generator.GeneratedClass.BaseTypes[0];
            Assert.AreEqual("System.Web.Mvc.WebViewPage(Of MyFooModel)", baseType.BaseType);
            Assert.AreEqual(0, generator.GeneratedExecuteMethod.Statements.Count);
        }

        [TestMethod]
        public void OnEndSpan_ProcessesModelSpansAndAddsDesignTimeStatement() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());
            mockHost.SetupGet(h => h.DefaultBaseClass).Returns("System.Web.Mvc.WebViewPage");
            var generator = new MvcVBRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.vbhtml", mockHost.Object) {
                DesignTimeMode = true
            };
            Span modelNameSpan = new ModelSpan(SourceLocation.Zero, "MyFooModel", "MyFooModel");
            generator.VisitStartBlock(BlockType.Statement);

            // Act
            generator.VisitSpan(modelNameSpan);

            // Assert
            Assert.AreEqual(1, generator.HelperVariablesMethod.Statements.Count);
            var statements = generator.HelperVariablesMethod.Statements.Cast<CodeSnippetStatement>().Select(s => s.Value).ToList();
            Assert.AreEqual("Dim __modelHelper As MyFooModel = Nothing\r\n", statements[0]);
        }
    }
}

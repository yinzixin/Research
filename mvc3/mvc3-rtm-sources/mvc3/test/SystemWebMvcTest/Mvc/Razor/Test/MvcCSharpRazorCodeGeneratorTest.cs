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
    public class MvcCSharpRazorCodeGeneratorTest {
        [TestMethod]
        public void Constructor() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            Assert.AreEqual("FooClass", generator.ClassName);
            Assert.AreEqual("Root.Namespace", generator.RootNamespaceName);
            Assert.AreEqual("SomeSourceFile.cshtml", generator.SourceFileName);
            Assert.AreSame(mockHost.Object, generator.Host);
        }

        [TestMethod]
        public void Constructor_DoesNotSetBaseTypeForNonMvcHost() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            Assert.AreEqual(0, generator.GeneratedClass.BaseTypes.Count);
        }

        [TestMethod]
        public void Constructor_DoesNotSetBaseTypeForSpecialPage() {
            // Arrange
            Mock<MvcWebPageRazorHost> mockHost = new Mock<MvcWebPageRazorHost>("_viewStart.cshtml", "_viewStart.cshtml");
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "_viewStart.cshtml", mockHost.Object);

            // Assert
            Assert.AreEqual(0, generator.GeneratedClass.BaseTypes.Count);
        }

        [TestMethod]
        public void Constructor_SetsBaseTypeForRegularPage() {
            // Arrange
            Mock<MvcWebPageRazorHost> mockHost = new Mock<MvcWebPageRazorHost>("SomeSourceFile.cshtml", "SomeSourceFile.cshtml") { CallBase = true };
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            Assert.AreEqual(1, generator.GeneratedClass.BaseTypes.Count);
            Assert.AreEqual("System.Web.Mvc.WebViewPage<dynamic>", generator.GeneratedClass.BaseTypes[0].BaseType);
        }

        [TestMethod]
        public void OnEndSpan_ProcessesModelSpans() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());
            mockHost.SetupGet(h => h.DefaultBaseClass).Returns("System.Web.Mvc.WebViewPage");
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);
            Span modelSpan = new ModelSpan(SourceLocation.Zero, "MyFooModel", "MyFooModel");
            generator.VisitStartBlock(BlockType.Statement);

            // Act
            generator.VisitSpan(modelSpan);

            // Assert
            Assert.AreEqual(1, generator.GeneratedClass.BaseTypes.Count);
            var baseType = generator.GeneratedClass.BaseTypes[0];
            Assert.AreEqual("System.Web.Mvc.WebViewPage<MyFooModel>", baseType.BaseType);
            Assert.AreEqual(0, generator.GeneratedExecuteMethod.Statements.Count);
        }

        [TestMethod]
        public void OnEndSpan_ProcessesModelSpansAndAddsDesignTimeStatement() {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());
            mockHost.SetupGet(h => h.DefaultBaseClass).Returns("System.Web.Mvc.WebViewPage");
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object) {
                DesignTimeMode = true
            };
            Span modelNameSpan = new ModelSpan(SourceLocation.Zero, "MyFooModel", "MyFooModel");
            generator.VisitStartBlock(BlockType.Statement);

            // Act
            generator.VisitSpan(modelNameSpan);

            // Assert
            Assert.AreEqual(3, generator.HelperVariablesMethod.Statements.Count);
            var statements = generator.HelperVariablesMethod.Statements.Cast<CodeSnippetStatement>().Select(s => s.Value).ToList();
            Assert.AreEqual("#pragma warning disable 219", statements[0]);
            Assert.AreEqual("MyFooModel __modelHelper = null;\r\n", statements[1]);
            Assert.AreEqual("#pragma warning restore 219", statements[2]);
        }
    }
}

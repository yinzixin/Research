using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Razor;
using System.Web.WebPages.TestUtils;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Razor.Test {
    [TestClass]
    public class WebPageRazorEngineHostTest {
        [TestMethod]
        public void ConstructorRequiresNonNullOrEmptyVirtualPath() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new WebPageRazorHost(null), "virtualPath");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new WebPageRazorHost(String.Empty), "virtualPath");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new WebPageRazorHost(null, "foo"), "virtualPath");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new WebPageRazorHost(String.Empty, "foo"), "virtualPath");
        }

        [TestMethod]
        public void ConstructorWithVirtualPathUsesItToDetermineBaseClassClassNameAndLanguage() {
            // Act
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.cshtml");

            // Assert
            Assert.AreEqual("_Page_Foo_Bar_cshtml", host.DefaultClassName);
            Assert.AreEqual("System.Web.WebPages.WebPage", host.DefaultBaseClass);
            Assert.IsInstanceOfType(host.CodeLanguage, typeof(CSharpRazorCodeLanguage));
            Assert.IsFalse(host.StaticHelpers);
        }

        [TestMethod]
        public void PostProcessGeneratedCodeAddsGlobalImports() {
            // Arrange
            WebPageRazorHost.AddGlobalImport("Foo.Bar");
            CodeCompileUnit generatedCode = new CodeCompileUnit();
            CodeNamespace generatedNamespace = new CodeNamespace();
            CodeTypeDeclaration generatedClass = new CodeTypeDeclaration();
            CodeMemberMethod executeMethod = new CodeMemberMethod();
            WebPageRazorHost host = new WebPageRazorHost("Foo.cshtml");

            // Act
            host.PostProcessGeneratedCode(generatedCode, generatedNamespace, generatedClass, executeMethod);

            // Assert
            Assert.IsTrue(generatedNamespace.Imports.OfType<CodeNamespaceImport>().Any(import => String.Equals("Foo.Bar", import.Namespace)));
        }

        [TestMethod]
        public void PostProcessGeneratedCodeAddsApplicationInstanceProperty() {
            const string expectedPropertyCode = @"
protected Foo.Bar ApplicationInstance {
    get {
        return ((Foo.Bar)(Context.ApplicationInstance));
    }
}
";

            // Arrange
            CodeCompileUnit generatedCode = new CodeCompileUnit();
            CodeNamespace generatedNamespace = new CodeNamespace();
            CodeTypeDeclaration generatedClass = new CodeTypeDeclaration();
            CodeMemberMethod executeMethod = new CodeMemberMethod();
            WebPageRazorHost host = new WebPageRazorHost("Foo.cshtml") {
                GlobalAsaxTypeName = "Foo.Bar"
            };

            // Act
            host.PostProcessGeneratedCode(generatedCode, generatedNamespace, generatedClass, executeMethod);

            // Assert
            CodeMemberProperty property = generatedClass.Members[0] as CodeMemberProperty;
            Assert.IsNotNull(property);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringBuilder builder = new StringBuilder();
            using(StringWriter writer = new StringWriter(builder)) {
                provider.GenerateCodeFromMember(property, writer, new CodeDom.Compiler.CodeGeneratorOptions());
            }

            Assert.AreEqual(expectedPropertyCode, builder.ToString());
        }
    }
}

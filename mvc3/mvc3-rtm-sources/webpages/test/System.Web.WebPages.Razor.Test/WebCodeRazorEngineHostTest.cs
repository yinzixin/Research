using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CodeDom;

namespace System.Web.WebPages.Razor.Test {
    [TestClass]
    public class WebCodeRazorEngineHostTest {
        [TestMethod]
        public void ConstructorWithMalformedVirtualPathSetsDefaultProperties() {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost(@"~/Foo/App_Code\Bar\Baz\Qux.cshtml");

            // Assert
            Assert.AreEqual("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.AreEqual("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.AreEqual("Qux", host.DefaultClassName);
            Assert.IsFalse(host.DefaultDebugCompilation);
            Assert.IsTrue(host.StaticHelpers);
        }

        [TestMethod]
        public void ConstructorWithFileOnlyVirtualPathSetsDefaultProperties() {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost(@"Foo.cshtml");

            // Assert
            Assert.AreEqual("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.AreEqual("ASP", host.DefaultNamespace);
            Assert.AreEqual("Foo", host.DefaultClassName);
            Assert.IsFalse(host.DefaultDebugCompilation);
        }

        [TestMethod]
        public void ConstructorWithVirtualPathSetsDefaultProperties() {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost("~/Foo/App_Code/Bar/Baz/Qux.cshtml");

            // Assert
            Assert.AreEqual("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.AreEqual("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.AreEqual("Qux", host.DefaultClassName);
            Assert.IsFalse(host.DefaultDebugCompilation);
        }

        [TestMethod]
        public void ConstructorWithVirtualAndPhysicalPathSetsDefaultProperties() {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost("~/Foo/App_Code/Bar/Baz/Qux.cshtml", @"C:\Qux.doodad");

            // Assert
            Assert.AreEqual("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.AreEqual("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.AreEqual("Qux", host.DefaultClassName);
            Assert.IsFalse(host.DefaultDebugCompilation);
        }

        [TestMethod]
        public void PostProcessGeneratedCodeRemovesExecuteMethod() {
            // Arrange
            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace();
            CodeTypeDeclaration typeDecl = new CodeTypeDeclaration();
            CodeMemberMethod executeMethod = new CodeMemberMethod();
            typeDecl.Members.Add(executeMethod);
            WebCodeRazorHost host = new WebCodeRazorHost("Foo.cshtml");

            // Act
            host.PostProcessGeneratedCode(ccu, ns, typeDecl, executeMethod);

            // Assert
            Assert.AreEqual(0, typeDecl.Members.OfType<CodeMemberMethod>().Count());
        }

        [TestMethod]
        public void PostProcessGeneratedCodeAddsStaticApplicationInstanceProperty() {
            // Arrange
            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace();
            CodeTypeDeclaration typeDecl = new CodeTypeDeclaration();
            CodeMemberMethod executeMethod = new CodeMemberMethod();
            typeDecl.Members.Add(executeMethod);
            WebCodeRazorHost host = new WebCodeRazorHost("Foo.cshtml");

            // Act
            host.PostProcessGeneratedCode(ccu, ns, typeDecl, executeMethod);

            // Assert
            CodeMemberProperty appInstance = typeDecl.Members
                                                     .OfType<CodeMemberProperty>()
                                                     .Where(p => p.Name.Equals("ApplicationInstance"))
                                                     .SingleOrDefault();
            Assert.IsNotNull(appInstance);
            Assert.IsTrue(appInstance.Attributes.HasFlag(MemberAttributes.Static));
        }
    }
}

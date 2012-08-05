using System.CodeDom;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test {
    [TestClass]
    public class RazorEngineHostTest {
        [TestMethod]
        public void ConstructorRequiresNonNullCodeLanguage() {
            ExceptionAssert.ThrowsArgNull(() => new RazorEngineHost(null), "codeLanguage");
            ExceptionAssert.ThrowsArgNull(() => new RazorEngineHost(null, () => new HtmlMarkupParser()), "codeLanguage");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullMarkupParser() {
            ExceptionAssert.ThrowsArgNull(() => new RazorEngineHost(new CSharpRazorCodeLanguage(), null), "markupParserFactory");
        }

        [TestMethod]
        public void ConstructorWithCodeLanguageSetsPropertiesAppropriately() {
            // Arrange
            RazorCodeLanguage language = new CSharpRazorCodeLanguage();
            
            // Act
            RazorEngineHost host = new RazorEngineHost(language);

            // Assert
            VerifyCommonDefaults(host);
            Assert.AreSame(language, host.CodeLanguage);
            Assert.IsInstanceOfType(host.CreateMarkupParser(), typeof(HtmlMarkupParser));
        }

        [TestMethod]
        public void ConstructorWithCodeLanguageAndMarkupParserSetsPropertiesAppropriately() {
            // Arrange
            RazorCodeLanguage language = new CSharpRazorCodeLanguage();
            MarkupParser expected = new HtmlMarkupParser();

            // Act
            RazorEngineHost host = new RazorEngineHost(language, () => expected);

            // Assert
            VerifyCommonDefaults(host);
            Assert.AreSame(language, host.CodeLanguage);
            Assert.AreSame(expected, host.CreateMarkupParser());
        }

        [TestMethod]
        public void DecorateCodeParserRequiresNonNullCodeParser() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().DecorateCodeParser(null), "incomingCodeParser");
        }

        [TestMethod]
        public void DecorateMarkupParserRequiresNonNullMarkupParser() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().DecorateMarkupParser(null), "incomingMarkupParser");
        }

        [TestMethod]
        public void DecorateCodeGeneratorRequiresNonNullCodeGenerator() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().DecorateCodeGenerator(null), "incomingCodeGenerator");
        }

        [TestMethod]
        public void PostProcessGeneratedCodeRequiresNonNullCompileUnit() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().PostProcessGeneratedCode(codeCompileUnit: null, 
                                                                                      generatedNamespace: new CodeNamespace(),
                                                                                      generatedClass: new CodeTypeDeclaration(), 
                                                                                      executeMethod: new CodeMemberMethod()),
                                          "codeCompileUnit");
        }

        [TestMethod]
        public void PostProcessGeneratedCodeRequiresNonNullGeneratedNamespace() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().PostProcessGeneratedCode(codeCompileUnit: new CodeCompileUnit(),
                                                                                      generatedNamespace: null,
                                                                                      generatedClass: new CodeTypeDeclaration(),
                                                                                      executeMethod: new CodeMemberMethod()),
                                          "generatedNamespace");
        }

        [TestMethod]
        public void PostProcessGeneratedCodeRequiresNonNullGeneratedClass() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().PostProcessGeneratedCode(codeCompileUnit: new CodeCompileUnit(),
                                                                                      generatedNamespace: new CodeNamespace(),
                                                                                      generatedClass: null,
                                                                                      executeMethod: new CodeMemberMethod()),
                                          "generatedClass");
        }

        [TestMethod]
        public void PostProcessGeneratedCodeRequiresNonNullExecuteMethod() {
            ExceptionAssert.ThrowsArgNull(() => CreateHost().PostProcessGeneratedCode(codeCompileUnit: new CodeCompileUnit(),
                                                                                      generatedNamespace: new CodeNamespace(),
                                                                                      generatedClass: new CodeTypeDeclaration(),
                                                                                      executeMethod: null),
                                          "executeMethod");
        }

        [TestMethod]
        public void DecorateCodeParserDoesNotModifyIncomingParser() {
            // Arrange
            ParserBase expected = new CSharpCodeParser();
            
            // Act
            ParserBase actual = CreateHost().DecorateCodeParser(expected);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void DecorateMarkupParserReturnsIncomingParser() {
            // Arrange
            MarkupParser expected = new HtmlMarkupParser();
            
            // Act
            MarkupParser actual = CreateHost().DecorateMarkupParser(expected);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void DecorateCodeGeneratorReturnsIncomingCodeGenerator() {
            // Arrange
            RazorCodeGenerator expected = new CSharpRazorCodeGenerator("Foo", "Bar", "Baz", CreateHost());
            
            // Act
            RazorCodeGenerator actual = CreateHost().DecorateCodeGenerator(expected);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void PostProcessGeneratedCodeDoesNotModifyCode() {
            // Arrange
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace();
            CodeTypeDeclaration typeDecl = new CodeTypeDeclaration();
            CodeMemberMethod execMethod = new CodeMemberMethod();

            // Act
            CreateHost().PostProcessGeneratedCode(compileUnit, ns, typeDecl, execMethod);

            // Assert
            Assert.AreEqual(0, compileUnit.Namespaces.Count);
            Assert.AreEqual(0, ns.Imports.Count);
            Assert.AreEqual(0, ns.Types.Count);
            Assert.AreEqual(0, typeDecl.Members.Count);
            Assert.AreEqual(0, execMethod.Statements.Count);
        }

        private static RazorEngineHost CreateHost() {
            return new RazorEngineHost(new CSharpRazorCodeLanguage());
        }

        private static void VerifyCommonDefaults(RazorEngineHost host) {
            Assert.AreEqual(GeneratedClassContext.Default, host.GeneratedClassContext);
            Assert.AreEqual(0, host.NamespaceImports.Count);
            Assert.IsFalse(host.DesignTimeMode);
            Assert.AreEqual(RazorEngineHost.InternalDefaultClassName, host.DefaultClassName);
            Assert.AreEqual(RazorEngineHost.InternalDefaultNamespace, host.DefaultNamespace);
        }
    }
}

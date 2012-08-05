using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace ASP {
    public class _Page_Foo_Test_cshtml {}
}

namespace System.Web.WebPages.Razor.Test {
    [TestClass]
    public class RazorBuildProviderTest {
        class MockAssemblyBuilder : IAssemblyBuilder {
            public BuildProvider BuildProvider { get; private set; }
            public CodeCompileUnit CompileUnit { get; private set; }
            public string LastTypeFactoryGenerated { get; private set; }

            public void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit) {
                BuildProvider = buildProvider;
                CompileUnit = compileUnit;
            }

            public void GenerateTypeFactory(string typeName) {
                LastTypeFactoryGenerated = typeName;
            }
        }

        [TestMethod]
        public void CodeCompilerTypeReturnsTypeFromCodeLanguage() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.vbhtml", @"C:\Foo\Bar.vbhtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Act
            CompilerType type = provider.CodeCompilerType;

            // Assert
            Assert.AreEqual(typeof(VBCodeProvider), type.CodeDomProviderType);
        }

        [TestMethod]
        public void CodeCompilerTypeSetsDebugFlagInFullTrust() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.vbhtml", @"C:\Foo\Bar.vbhtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Act
            CompilerType type = provider.CodeCompilerType;

            // Assert
            Assert.IsTrue(type.CompilerParameters.IncludeDebugInformation);
        }

        [TestMethod]
        public void GetGeneratedTypeUsesNameAndNamespaceFromHostToExtractType() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Test.cshtml", @"C:\Foo\Test.cshtml");
            RazorBuildProvider provider = new RazorBuildProvider() { Host = host };
            CompilerResults results = new CompilerResults(new TempFileCollection());
            results.CompiledAssembly = typeof(ASP._Page_Foo_Test_cshtml).Assembly;

            // Act
            Type typ = provider.GetGeneratedType(results);

            // Assert
            Assert.AreEqual(typeof(ASP._Page_Foo_Test_cshtml), typ);
        }

        [TestMethod]
        public void GenerateCodeCoreAddsGeneratedCodeToAssemblyBuilder() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.vbhtml", @"C:\Foo\Bar.vbhtml");
            RazorBuildProvider provider = new RazorBuildProvider();
            CodeCompileUnit ccu = new CodeCompileUnit();
            MockAssemblyBuilder asmBuilder = new MockAssemblyBuilder();
            provider.Host = host;
            provider.GeneratedCode = ccu;

            // Act
            provider.GenerateCodeCore(asmBuilder);

            // Assert
            Assert.AreSame(provider, asmBuilder.BuildProvider);
            Assert.AreSame(ccu, asmBuilder.CompileUnit);
            Assert.AreEqual("ASP._Page_Foo_Bar_vbhtml", asmBuilder.LastTypeFactoryGenerated);
        }

        [TestMethod]
        public void CodeGenerationStartedTest() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.vbhtml", @"C:\Foo\Bar.vbhtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Expected original base dependencies
            var baseDependencies = new ArrayList();
            baseDependencies.Add("/Samples/Foo/Bar.vbhtml");

            // Expected list of dependencies after GenerateCode is called
            var dependencies = new ArrayList();
            dependencies.Add(baseDependencies[0]);
            dependencies.Add("/Samples/Foo/Foo.vbhtml");

            // Set up the event handler
            provider.CodeGenerationStartedInternal += (sender, e) => {
                var bp = sender as RazorBuildProvider;
                bp.AddVirtualPathDependency("/Samples/Foo/Foo.vbhtml");
            };

            // Set up the base dependency
            MockAssemblyBuilder builder = new MockAssemblyBuilder();
            typeof(BuildProvider).GetField("_virtualPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(provider, CreateVirtualPath("/Samples/Foo/Bar.vbhtml"));

            // Test that VirtualPathDependencies returns the original dependency before GenerateCode is called
            Assert.IsTrue(baseDependencies.OfType<string>().SequenceEqual(provider.VirtualPathDependencies.OfType<string>()));

            // Act
            provider.GenerateCodeCore(builder);

            // Assert
            Assert.IsNotNull(provider.AssemblyBuilderInternal);
            Assert.AreEqual(builder, provider.AssemblyBuilderInternal);
            Assert.IsTrue(dependencies.OfType<string>().SequenceEqual(provider.VirtualPathDependencies.OfType<string>()));
            Assert.AreEqual("/Samples/Foo/Bar.vbhtml", provider.VirtualPath);
        }

        [TestMethod]
        public void AfterGeneratedCodeEventGetsExecutedAtCorrectTime() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.vbhtml", @"C:\Foo\Bar.vbhtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            provider.CodeGenerationCompletedInternal += (sender, e) => {
                Assert.AreEqual("~/Foo/Bar.vbhtml", e.VirtualPath);
                e.GeneratedCode.Namespaces.Add(new CodeNamespace("DummyNamespace"));
            };

            // Act
            CodeCompileUnit generated = provider.GeneratedCode;

            // Assert
            Assert.IsNotNull(generated.Namespaces
                                      .OfType<CodeNamespace>()
                                      .SingleOrDefault(ns => String.Equals(ns.Name, "DummyNamespace")));
        }

        [TestMethod]
        public void GeneratedCodeThrowsHttpParseExceptionForLastParserError() {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.cshtml", @"C:\Foo\Bar.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @{ if( baz");
            provider.Host = host;

            // Act
            ExceptionAssert.Throws<HttpParseException>(() => { 
                CodeCompileUnit ccu = provider.GeneratedCode; 
            });
        }

        private static object CreateVirtualPath(string path) {
            var vPath = typeof(BuildProvider).Assembly.GetType("System.Web.VirtualPath");
            var method = vPath.GetMethod("CreateNonRelative", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return method.Invoke(null, new object[] { path });
        }

        private static RazorBuildProvider CreateBuildProvider(string razorContent) {
            Mock<RazorBuildProvider> mockProvider = new Mock<RazorBuildProvider>() {
                CallBase = true
            };
            mockProvider.Setup(p => p.InternalOpenReader())
                        .Returns(() => new StringReader(razorContent));
            return mockProvider.Object;
        }
    }
}

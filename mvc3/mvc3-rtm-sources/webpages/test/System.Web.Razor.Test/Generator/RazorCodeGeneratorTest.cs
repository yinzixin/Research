//#define GENERATE_BASELINES

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Razor.Generator;
using System.Web.Razor.Test.Utils;
using System.Text;
using System.Web.Compilation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Generator {
    public abstract class RazorCodeGeneratorTest<TLanguage> where TLanguage : RazorCodeLanguage, new() {
        protected static readonly string TestRootNamespaceName = "TestOutput";
        
        protected abstract string FileExtension { get; }
        protected abstract string LanguageName { get; }

        protected RazorEngineHost CreateHost() {
            return new RazorEngineHost(new TLanguage());
        }

        protected void RunTest(string name, string baselineName = null, bool generatePragmas = true, bool designTimeMode = false, IList<LinePragmaCodeInfo> expectedDesignTimePragmas = null, Action<RazorEngineHost> hostConfig = null) {
            // Load the test files
            if (baselineName == null) { 
                baselineName = name; 
            }
            string source = TestFile.Create(String.Format("CodeGenerator.{1}.Source.{0}.{2}", name, LanguageName, FileExtension)).ReadAllText();
            string expectedOutput = TestFile.Create(String.Format("CodeGenerator.{1}.Output.{0}.txt", baselineName, LanguageName)).ReadAllText();
            
            // Set up the host and engine
            RazorEngineHost host = CreateHost();
            host.NamespaceImports.Add("System");
            host.DesignTimeMode = designTimeMode;
            host.StaticHelpers = true;
            if (hostConfig != null) {
                hostConfig(host);
            }

            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            // Add support for templates, etc.
            host.GeneratedClassContext = new GeneratedClassContext(GeneratedClassContext.DefaultExecuteMethodName,
                                                                   GeneratedClassContext.DefaultWriteMethodName,
                                                                   GeneratedClassContext.DefaultWriteLiteralMethodName,
                                                                   "WriteTo",
                                                                   "WriteLiteralTo",
                                                                   "Template",
                                                                   "DefineSection");
            
            // Generate code for the file
            GeneratorResults results = null;
            using (StringTextBuffer buffer = new StringTextBuffer(source)) {
                results = engine.GenerateCode(buffer, className: name, rootNamespace: TestRootNamespaceName, sourceFileName: generatePragmas ? String.Format("{0}.{1}", name, FileExtension) : null);
            }

            
            // Generate code
            CodeCompileUnit ccu = results.GeneratedCode;
            CodeDomProvider codeProvider = (CodeDomProvider)Activator.CreateInstance(host.CodeLanguage.CodeDomProviderType);

            CodeGeneratorOptions options = new CodeGeneratorOptions();
            
            // Both run-time and design-time use these settings. See:
            // * $/Dev10/pu/SP_WebTools/venus/html/Razor/Impl/RazorCodeGenerator.cs:204
            // * $/Dev10/Releases/RTMRel/ndp/fx/src/xsp/System/Web/Compilation/BuildManagerHost.cs:373
            options.BlankLinesBetweenMembers = false;
            options.IndentString = string.Empty;

            StringBuilder output = new StringBuilder();
            using (StringWriter writer = new StringWriter(output)) {
                codeProvider.GenerateCodeFromCompileUnit(ccu, writer, options);
            }

#if GENERATE_BASELINES
            // Update baseline
            // IMPORTANT! Replace this path with the local path on your machine to the baseline files!
            string baselineFile = String.Format(@"D:\dd\Plan9\Main\test\System.Web.Razor.Test\TestFiles\CodeGenerator\{0}\Output\{1}.txt", LanguageName, baselineName);
            File.Delete(baselineFile);
            File.WriteAllText(baselineFile, MiscUtils.StripRuntimeVersion(output.ToString()));
#else
            // Verify code against baseline
            Assert.AreEqual(expectedOutput, MiscUtils.StripRuntimeVersion(output.ToString()));
#endif

            // Verify design-time pragmas
            if (designTimeMode) {
                Assert.IsTrue(expectedDesignTimePragmas != null || results.DesignTimeLineMappings == null || results.DesignTimeLineMappings.Count == 0);
                Assert.IsTrue(expectedDesignTimePragmas == null || (results.DesignTimeLineMappings != null && results.DesignTimeLineMappings.Count > 0));
                Enumerable.Zip(expectedDesignTimePragmas, results.DesignTimeLineMappings, (expected, actual) => {
                    Assert.AreEqual(expected.CodeLength, actual.Value.CodeLength, "CodeLength values are not equal for pragma {0}!", actual.Key);
                    Assert.AreEqual(expected.StartColumn, actual.Value.StartColumn, "StartColumn values are not equal for pragma {0}!", actual.Key);
                    Assert.AreEqual(expected.StartGeneratedColumn, actual.Value.StartGeneratedColumn, "StartGeneratedColumn values are not equal for pragma {0}!", actual.Key);
                    Assert.AreEqual(expected.StartLine, actual.Value.StartLine, "StartLine values are not equal for pragma {0}!", actual.Key);
                    return (object)null;
                }).ToList();
                Assert.AreEqual(expectedDesignTimePragmas.Count, results.DesignTimeLineMappings.Count);
            }
        }
    }
}

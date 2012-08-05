using System.Collections.Generic;
using System.Web.Razor.Generator;
using System.Web.Compilation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Generator {
    [TestClass]
    public class VBRazorCodeGeneratorTest : RazorCodeGeneratorTest<VBRazorCodeLanguage> {
        private const string TestPhysicalPath = @"C:\Bar.vbhtml";
        private const string TestVirtualPath = "~/Foo/Bar.vbhtml";

        protected override string FileExtension { get { return "vbhtml"; } }
        protected override string LanguageName { get { return "VB"; } }

        [TestMethod]
        public void ConstructorRequiresNonNullClassName() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new VBRazorCodeGenerator(null, TestRootNamespaceName, TestPhysicalPath, CreateHost()), "className");
        }

        [TestMethod]
        public void ConstructorRequiresNonEmptyClassName() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new VBRazorCodeGenerator(String.Empty, TestRootNamespaceName, TestPhysicalPath, CreateHost()), "className");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullRootNamespaceName() {
            ExceptionAssert.ThrowsArgNull(() => new VBRazorCodeGenerator("Foo", null, TestPhysicalPath, CreateHost()), "rootNamespaceName");
        }

        [TestMethod]
        public void ConstructorAllowsEmptyRootNamespaceName() {
            new VBRazorCodeGenerator("Foo", String.Empty, TestPhysicalPath, CreateHost());
        }

        [TestMethod]
        public void ConstructorRequiresNonNullHost() {
            ExceptionAssert.ThrowsArgNull(() => new VBRazorCodeGenerator("Foo", TestRootNamespaceName, TestPhysicalPath, null), "host");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesNestedCodeBlocks() {
            RunTest("NestedCodeBlocks");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesSimpleCodeBlock() {
            RunTest("CodeBlock");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesSimpleExplicitExpression() {
            RunTest("ExplicitExpression");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesSimpleCodeBlockWithMarkup() {
            RunTest("MarkupInCodeBlock");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesSimpleImplicitBlocksWithMarkup() {
            RunTest("Blocks");
        }


        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesImplicitExpression() {
            RunTest("ImplicitExpression");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesImportStatements() {
            RunTest("Imports");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesImportStatementsAtDesignTimeButCannotWrapPragmasAroundImportStatement() {
            RunTest("Imports", "Imports.DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 2, 1, 19, false),
                /* 02 */ new LinePragmaCodeInfo(2, 2, 1, 36, false),
                /* 03 */ new LinePragmaCodeInfo(3, 2, 1, 16, false),
                /* 04 */ new LinePragmaCodeInfo(5, 30, 30, 22, false),
                /* 05 */ new LinePragmaCodeInfo(6, 36, 36, 21, false),
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesExpressionsWithinCode() {
            RunTest("ExpressionsInCode");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesFunctionsBlocks() {
            RunTest("FunctionsBlock");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesFunctionsBlocksAtDesignTime() {
            RunTest("FunctionsBlock", "FunctionsBlock.DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 11, 1, 129, false),
                /* 02 */ new LinePragmaCodeInfo(8, 26, 26, 11, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesOptionStatements() {
            RunTest("Options");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesTemplates() {
            RunTest("Templates");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesRazorComments() {
            RunTest("RazorComments");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesSections() {
            RunTest("Sections");
        }

        [TestMethod]
        public void VBCodeGeneratorGeneratesCodeWithParserErrorsInDesignTimeMode() {
            RunTest("ParserError", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 6, 6, 16, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesInheritsAtRuntime() {
            RunTest("Inherits", baselineName: "Inherits.Runtime");
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesInheritsAtDesigntime() {
            RunTest("Inherits", baselineName: "Inherits.Designtime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 2, 7, 5, false),
                /* 02 */ new LinePragmaCodeInfo(3, 11, 25, 31, false),
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForUnfinishedExpressionsInCode() {
            RunTest("UnfinishedExpressionInCode", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 6, 6, 2, false),
                /* 02 */ new LinePragmaCodeInfo(2, 2, 7, 9, false),
                /* 03 */ new LinePragmaCodeInfo(2, 11, 11, 2, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasMarkupAndExpressions() {
            RunTest("DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(2, 14, 13, 17, false),
                /* 02 */ new LinePragmaCodeInfo(3, 20, 20, 1, false),
                /* 03 */ new LinePragmaCodeInfo(3, 25, 25, 18, false),
                /* 04 */ new LinePragmaCodeInfo(8, 3, 7, 12, false),
                /* 05 */ new LinePragmaCodeInfo(9, 2, 7, 4,false),
                /* 06 */ new LinePragmaCodeInfo(9, 16, 16, 3,false),
                /* 07 */ new LinePragmaCodeInfo(9, 27, 27, 1,false),
                /* 08 */ new LinePragmaCodeInfo(14,6, 7, 3,false),
                /* 09 */ new LinePragmaCodeInfo(17, 9, 24, 5,false),
                /* 10 */ new LinePragmaCodeInfo(17, 14, 14, 28,false),
                /* 11 */ new LinePragmaCodeInfo(19, 20, 20, 14,false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForImplicitExpressionStartedAtEOF() {
            RunTest("ImplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 2, 7, 0, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForExplicitExpressionStartedAtEOF() {
            RunTest("ExplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 7, 0, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForCodeBlockStartedAtEOF() {
            RunTest("CodeBlockAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 6, 6, 0, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpression() {
            RunTest("EmptyImplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 2, 7, 0, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpressionInCode() {
            RunTest("EmptyImplicitExpressionInCode", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 6, 6, 6, false),
                /* 02 */ new LinePragmaCodeInfo(2, 6, 7, 0, false),
                /* 03 */ new LinePragmaCodeInfo(2, 6, 6, 2, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyExplicitExpression() {
            RunTest("EmptyExplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 7, 0, false)
            });
        }

        [TestMethod]
        public void VBCodeGeneratorDoesNotRenderLinePragmasIfGenerateLinePragmasIsSetToFalse() {
            RunTest("NoLinePragmas", generatePragmas: false);
        }

        [TestMethod]
        public void VBCodeGeneratorRendersHelpersBlockCorrectlyWhenInstanceHelperRequested() {
            RunTest("Helpers", baselineName: "Helpers.Instance", hostConfig: h => h.StaticHelpers = false);
        }

        [TestMethod]
        public void VBCodeGeneratorRendersHelpersBlockCorrectly() {
            RunTest("Helpers");
        }

        [TestMethod]
        public void VBCodeGeneratorRendersHelpersBlockWithMissingCloseParenCorrectly() {
            RunTest("HelpersMissingCloseParen");
        }

        [TestMethod]
        public void VBCodeGeneratorRendersHelpersBlockWithMissingNameCorrectly() {
            RunTest("HelpersMissingName");
        }

        [TestMethod]
        public void VBCodeGeneratorRendersHelpersBlockWithMissingOpenParenCorrectly() {
            RunTest("HelpersMissingOpenParen");
        }

        [TestMethod]
        public void VBCodeGeneratorRendersNestedHelpersBlockCorrectly() {
            RunTest("NestedHelpers");
        }
    }
}

/* 01 */
using System.Collections.Generic;
using System.Web.Compilation;
using System.Web.Razor.Generator;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test.Generator {
    [TestClass]
    public class CSharpRazorCodeGeneratorTest : RazorCodeGeneratorTest<CSharpRazorCodeLanguage> {

        protected override string FileExtension {
            get { return "cshtml"; }
        }

        protected override string LanguageName {
            get { return "CS"; }
        }

        private const string TestPhysicalPath = @"C:\Bar.cshtml";
        private const string TestVirtualPath = "~/Foo/Bar.cshtml";

        [TestMethod]
        public void ConstructorRequiresNonNullClassName() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new CSharpRazorCodeGenerator(null, TestRootNamespaceName, TestPhysicalPath, CreateHost()), "className");
        }

        [TestMethod]
        public void ConstructorRequiresNonEmptyClassName() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => new CSharpRazorCodeGenerator(String.Empty, TestRootNamespaceName, TestPhysicalPath, CreateHost()), "className");
        }

        [TestMethod]
        public void ConstructorRequiresNonNullRootNamespaceName() {
            ExceptionAssert.ThrowsArgNull(() => new CSharpRazorCodeGenerator("Foo", null, TestPhysicalPath, CreateHost()), "rootNamespaceName");
        }

        [TestMethod]
        public void ConstructorAllowsEmptyRootNamespaceName() {
            new CSharpRazorCodeGenerator("Foo", String.Empty, TestPhysicalPath, CreateHost());
        }

        [TestMethod]
        public void ConstructorRequiresNonNullHost() {
            ExceptionAssert.ThrowsArgNull(() => new CSharpRazorCodeGenerator("Foo", TestRootNamespaceName, TestPhysicalPath, null), "host");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesNestedCodeBlocks() {
            RunTest("NestedCodeBlocks");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesSimpleCodeBlock() {
            RunTest("CodeBlock");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesSimpleExplicitExpression() {
            RunTest("ExplicitExpression");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesSimpleCodeBlockWithMarkup() {
            RunTest("MarkupInCodeBlock");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesSimpleImplicitBlocksWithMarkup() {
            RunTest("Blocks");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesImplicitExpression() {
            RunTest("ImplicitExpression");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesImportStatements() {
            RunTest("Imports");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesImportStatementsAtDesignTime() {
            RunTest("Imports", "Imports.DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 2, 1, 15, false),
                /* 02 */ new LinePragmaCodeInfo(2, 2, 1, 32, false),
                /* 03 */ new LinePragmaCodeInfo(3, 2, 1, 12, false),
                /* 04 */ new LinePragmaCodeInfo(5, 30, 30, 21, false),
                /* 05 */ new LinePragmaCodeInfo(6, 36, 36, 20, false),
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesExpressionsWithinCode() {
            RunTest("ExpressionsInCode");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesFunctionsBlocks() {
            RunTest("FunctionsBlock");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesFunctionsBlocksAtDesignTime() {
            RunTest("FunctionsBlock", "FunctionsBlock.DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 13, 1, 104, false),
                /* 02 */ new LinePragmaCodeInfo(8, 26, 26, 11, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesTemplates() {
            RunTest("Templates");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesSections() {
            RunTest("Sections");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesRazorComments() {
            RunTest("RazorComments");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesHiddenSpansWithinCode() {
            RunTest("HiddenSpansInCode", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo> {
                new LinePragmaCodeInfo(1, 3, 3, 6, false),
                new LinePragmaCodeInfo(2, 6, 6, 5, false),
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorGeneratesCodeWithParserErrorsInDesignTimeMode() {
            RunTest("ParserError", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 3, 3, 31, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesInheritsAtRuntime() {
            RunTest("Inherits", baselineName: "Inherits.Runtime");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesInheritsAtDesigntime() {
            RunTest("Inherits", baselineName: "Inherits.Designtime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 2, 7, 5, false),
                /* 02 */ new LinePragmaCodeInfo(3, 11, 11, 27, false),
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForUnfinishedExpressionsInCode() {
            RunTest("UnfinishedExpressionInCode", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 3, 3, 2, false),
                /* 02 */ new LinePragmaCodeInfo(2, 2, 7, 9, false),
                /* 03 */ new LinePragmaCodeInfo(2, 11, 11, 2, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasMarkupAndExpressions() {
            RunTest("DesignTime", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(2, 14, 13, 36, false),
                /* 02 */ new LinePragmaCodeInfo(3, 23, 23, 1, false),
                /* 03 */ new LinePragmaCodeInfo(3, 28, 28, 15, false),
                /* 04 */ new LinePragmaCodeInfo(8, 3, 7, 12, false),
                /* 05 */ new LinePragmaCodeInfo(9, 2, 7, 4,false),
                /* 06 */ new LinePragmaCodeInfo(9, 15, 15, 3,false),
                /* 07 */ new LinePragmaCodeInfo(9, 26, 26, 1,false),
                /* 08 */ new LinePragmaCodeInfo(14, 6, 7, 3,false),
                /* 09 */ new LinePragmaCodeInfo(17, 9, 24, 7,false),
                /* 10 */ new LinePragmaCodeInfo(17, 16, 16, 26,false),
                /* 11 */ new LinePragmaCodeInfo(19, 19, 19, 9,false),
                /* 12 */ new LinePragmaCodeInfo(21, 1, 1, 1,false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForImplicitExpressionStartedAtEOF() {
            RunTest("ImplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 2, 7, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForExplicitExpressionStartedAtEOF() {
            RunTest("ExplicitExpressionAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 7, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForCodeBlockStartedAtEOF() {
            RunTest("CodeBlockAtEOF", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 3, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpression() {
            RunTest("EmptyImplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 2, 7, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyImplicitExpressionInCode() {
            RunTest("EmptyImplicitExpressionInCode", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(1, 3, 3, 6, false),
                /* 02 */ new LinePragmaCodeInfo(2, 6, 7, 0, false),
                /* 03 */ new LinePragmaCodeInfo(2, 6, 6, 2, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyExplicitExpression() {
            RunTest("EmptyExplicitExpression", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 7, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyGeneratesDesignTimePragmasForEmptyCodeBlock() {
            RunTest("EmptyCodeBlock", designTimeMode: true, expectedDesignTimePragmas: new List<LinePragmaCodeInfo>() {
                /* 01 */ new LinePragmaCodeInfo(3, 3, 3, 0, false)
            });
        }

        [TestMethod]
        public void CSharpCodeGeneratorDoesNotRenderLinePragmasIfGenerateLinePragmasIsSetToFalse() {
            RunTest("NoLinePragmas", generatePragmas: false);
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockCorrectlyWhenInstanceHelperRequested() {
            RunTest("Helpers", baselineName: "Helpers.Instance", hostConfig: h => h.StaticHelpers = false);
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockCorrectly() {
            RunTest("Helpers");
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockWithMissingCloseParenCorrectly() {
            RunTest("HelpersMissingCloseParen");
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockWithMissingNameCorrectly() {
            RunTest("HelpersMissingName");
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockWithMissingOpenBraceCorrectly() {
            RunTest("HelpersMissingOpenBrace");
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersHelpersBlockWithMissingOpenParenCorrectly() {
            RunTest("HelpersMissingOpenParen");
        }

        [TestMethod]
        public void CSharpCodeGeneratorRendersNestedHelpersBlockCorrectly() {
            RunTest("NestedHelpers");
        }

        [TestMethod]
        public void CSharpCodeGeneratorCorrectlyRendersPragmasForInlineBlocks() {
            RunTest("InlineBlocks");
        }
    }
}

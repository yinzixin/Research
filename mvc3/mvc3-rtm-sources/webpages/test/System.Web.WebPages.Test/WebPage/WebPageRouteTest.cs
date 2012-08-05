using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class WebPageRouteTest {
        private class HashyVPP : VirtualPathProvider {
            HashSet<string> _existingFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            public HashyVPP(IEnumerable<string> validFilePaths) {
                foreach (string file in validFilePaths) {
                    _existingFiles.Add(file);
                }
            }

            public override bool FileExists(string virtualPath) {
                return _existingFiles.Contains(virtualPath);
            }
        }

        // Helper to test smarty route match, null match string is used for no expected match
        private void ConstraintTest(IEnumerable<string> validFiles, IEnumerable<string> supportedExt, string url, string match, string pathInfo) {
            HashyVPP vpp = new HashyVPP(validFiles);
            var virtualPathFactoryManager = new VirtualPathFactoryManager(vpp);

            WebPageMatch smartyMatch = WebPageRoute.MatchRequest(url, supportedExt, virtualPathFactoryManager);
            if (match != null) {
                Assert.IsNotNull(smartyMatch, "Should have found a match: "+match);
                Assert.AreEqual(match, smartyMatch.MatchedPath);
                Assert.AreEqual(pathInfo, smartyMatch.PathInfo);
            }
            else {
                Assert.IsNull(smartyMatch, "unexpected match");
            }

        }

        [TestMethod]
        public void MultipleExtensionsTest() {
            string[] files = new[] { "~/1.one", "~/2.two", "~/1.1/2/3.3", "~/one/two/3/4.4", "~/one/two/3/4/5/6/foo.htm" };
            string[] extensions = new[] { "aspx", "hao", "one", "two", "3", "4" };

            ConstraintTest(files, extensions , "1.1/2/3", "1.1/2/3.3", "");
            ConstraintTest(files, extensions, "1/2/3/4", "1.one", "2/3/4");
            ConstraintTest(files, extensions, "2/3/4", "2.two", "3/4");
            ConstraintTest(files, extensions, "one/two/3/4/5/6", "one/two/3/4.4", "5/6");
            ConstraintTest(files, extensions, "one/two/3/4/5/6/foo", "one/two/3/4.4", "5/6/foo");
            ConstraintTest(files, extensions, "one/two/3/4/5/6/foo.htm", null, null);
        }

        [TestMethod]
        public void BlockUnderscoreTests() {
            string[] files = new[] { "~/hi.evil", "~/_hi.evil", "~/_nest/good.evil", "~/_nest/_hide.evil", "~/_ok.good" };
            string[] extensions = new[] { "evil" };

            ConstraintTest(files, extensions, "hi", "hi.evil", "");
            ConstraintTest(files, extensions, "_nest/good/some/extra/path/info", "_nest/good.evil", "some/extra/path/info");
            ExceptionAssert.Throws<HttpException>(() => { ConstraintTest(files, extensions, "_hi", null, null); }, "Files with leading underscores (\"_\") cannot be served.");
            ExceptionAssert.Throws<HttpException>(() => { ConstraintTest(files, extensions, "_nest/_hide", null, null); }, "Files with leading underscores (\"_\") cannot be served.");
        }

        [TestMethod]
        public void UnderscoreWithNoExtensionNotBlockedTest() {
            string[] files = new[] { "~/_ok.good" };
            string[] extensions = new[] { "evil" };
            ConstraintTest(files, extensions, "_ok.good", null, null);
        }

        [TestMethod]
        public void UnsupportedExtensionExistingFileTest() {
            ConstraintTest(new[] { "~/hao.aspx", "~/hao/hao.txt" }, new[] { "aspx" }, "hao/hao.txt", null, null);
        }

        [TestMethod]
        public void NullPathValueDoesNotMatchTest() {
            ConstraintTest(new[] { "~/hao.aspx", "~/hao/hao.txt" }, new[] { "aspx" }, null, null, null);
        }

        [TestMethod]
        public void RightToLeftPrecedenceTest() {
            ConstraintTest(new[] { "~/one/two/three.aspx", "~/one/two.aspx", "~/one.aspx" }, new[] { "aspx" }, "one/two/three", "one/two/three.aspx", "");
        }

        [TestMethod]
        public void DefaultPrecedenceTests() {
            string[] files = new[] { "~/one/two/default.aspx", "~/one/default.aspx", "~/default.aspx" };
            string[] extensions = new[] { "aspx" };

            // Default only tries to look at the full path level
            ConstraintTest(files, extensions, "one/two/three", null, null);
            ConstraintTest(files, extensions, "one/two", "one/two/default.aspx", "");
            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
            ConstraintTest(files, extensions, "", "default.aspx", "");
            ConstraintTest(files, extensions, "one/two/three/four/five/six/7/8", null, null);
        }

        [TestMethod]
        public void IndexTests() {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/index.aspq" };
            string[] extensions = new[] { "aspx", "aspq" };

            // index only tries to look at the full path level
            ConstraintTest(files, extensions, "one/two/three", null, null);
            ConstraintTest(files, extensions, "one/two", "one/two/index.aspx", "");
            ConstraintTest(files, extensions, "one", "one/index.aspx", "");
            ConstraintTest(files, extensions, "", "index.aspq", "");
            ConstraintTest(files, extensions, "one/two/three/four/five/six/7/8", null, null);
        }

        [TestMethod]
        public void DefaultVsIndexNestedTest() {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/one/default.aspx", "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx", "aspq" };

            ConstraintTest(files, extensions, "one/two", "one/two/index.aspx", "");
            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
            ConstraintTest(files, extensions, "", "default.aspx", "");
        }

        [TestMethod]
        public void DefaultVsIndexSameExtensionTest() {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/one/default.aspx", "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx" };

            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
        }

        [TestMethod]
        public void DefaultVsIndexDifferentExtensionTest() {
            string[] files = new[] { "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx", "aspq" };

            ConstraintTest(files, extensions, "", "default.aspx", "");
        }

        [TestMethod]
        public void DefaultVsIndexOnlyOneExtensionTest() {
            string[] files = new[] { "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "", "index.aspq", "");
        }

        [TestMethod]
        public void FullMatchNoPathInfoTest() {
            ConstraintTest(new[] { "~/hao.aspx" }, new[] { "aspx" }, "hao", "hao.aspx", "");
        }

        [TestMethod]
        public void MatchFileWithExtensionTest() {
            string[] files = new[] { "~/page.aspq" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "page.aspq", "page.aspq", "");
        }

        [TestMethod]
        public void NoMatchFileWithWrongExtensionTest() {
            string[] files = new[] { "~/page.aspx" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "page.aspx", null, null);
        }


    }
}

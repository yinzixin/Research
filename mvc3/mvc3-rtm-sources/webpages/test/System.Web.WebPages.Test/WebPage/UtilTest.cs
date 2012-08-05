using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.WebPages.Resources;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class UtilTest {
        [TestMethod]
        public void IsSimpleNameTest() {
            Assert.IsTrue(Util.IsSimpleName("Test.cshtml"));
            Assert.IsTrue(Util.IsSimpleName("Test.Hello.cshtml"));
            Assert.IsFalse(Util.IsSimpleName("~/myapp/Test/Hello.cshtml"));
            Assert.IsFalse(Util.IsSimpleName("../Test/Hello.cshtml"));
            Assert.IsFalse(Util.IsSimpleName("../../Test/Hello.cshtml"));
            Assert.IsFalse(Util.IsSimpleName("/Test/Hello.cshtml"));
        }

        [TestMethod]
        public void IsAnonymousTypeTest() {
            Assert.IsFalse(Util.IsAnonymousType(typeof(object)));
            Assert.IsFalse(Util.IsAnonymousType(typeof(string)));
            Assert.IsFalse(Util.IsAnonymousType(typeof(IDictionary<object, object>)));
            Assert.IsTrue(Util.IsAnonymousType((new { A = "a", B = "b" }.GetType())));
            var x = "x";
            var y = "y";
            Assert.IsTrue(Util.IsAnonymousType((new { x, y }.GetType())));
        }

        [TestMethod]
        public void IsAnonymousTypeNullTest() {
            ExceptionAssert.ThrowsArgNull(() =>
                Util.IsAnonymousType(null), "type");
        }

        [TestMethod]
        public void AddAnonymousTypeObjectToDictionaryTest() {
            IDictionary<string, object> d = new Dictionary<string, object>();
            d.Add("X", "Xvalue");
            Util.AddAnonymousTypeObjectToDictionary(d, new { A = "a", B = "b" });
            Assert.AreEqual("Xvalue", d["X"]);
            Assert.AreEqual("a", d["A"]);
            Assert.AreEqual("b", d["B"]);
        }

        [TestMethod]
        public void CodeDomDefinedExtensionThrowsTest() {
            var extension = ".js";
            var virtualPath = "Layout.js";

            ExceptionAssert.Throws<HttpException>(
                () => {
                    Util.ThrowIfCodeDomDefinedExtension(virtualPath, new HttpCompileException());
                }, String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
        }

        [TestMethod]
        public void CodeDomDefinedExtensionDoesNotThrowTest() {
            var virtualPath = "Layout.txt";
            // Should not throw an exception
            Util.ThrowIfCodeDomDefinedExtension(virtualPath, new HttpCompileException());
        }
        [TestMethod]
        public void ResolveClientUrlTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx");
                    Utils.CreateHttpRuntime("/WebSite1/");

                    var control = new Control();
                    var result = control.ResolveClientUrl("~/world/test");
                    Assert.AreEqual("../world/test", result);
                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void UrlTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx");
                    Utils.CreateHttpRuntime("/WebSite1/");
                    var vpath = "~/subfolder1/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "../world/test.aspx";
                    Assert.AreEqual(expected, Util.Url(vpath, href));
                    Assert.AreEqual(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void UrlTest2() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/default.aspx");
                    Utils.CreateHttpRuntime("/WebSite1/");

                    var vpath = "~/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "world/test.aspx";
                    Assert.AreEqual(expected, Util.Url(vpath, href));
                    Assert.AreEqual(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void UrlTest3() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx");
                    Utils.CreateHttpRuntime("/WebSite1/");

                    var vpath = "~/subfolder1/default.aspx";
                    var href = "world/test.aspx";
                    var expected = "world/test.aspx";
                    Assert.AreEqual(expected, Util.Url(vpath, href));
                    Assert.AreEqual(expected, new MockPage() { VirtualPath = vpath }.Href(href));

                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void UrlTest4() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx");
                    Utils.CreateHttpRuntime("/WebSite1/");

                    var vpath = "~/subfolder2/default.aspx";
                    var href = "world/test.aspx";
                    var expected = "../subfolder2/world/test.aspx";
                    Assert.AreEqual(expected, Util.Url(vpath, href));
                    Assert.AreEqual(expected, new MockPage() { VirtualPath = vpath }.Href(href));

                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void BuildUrlEncodesPagePart() {
            // Arrange
            var page = "This is a really bad name for a page";
            var expected = "This%20is%20a%20really%20bad%20name%20for%20a%20page";

            // Act
            var actual = Util.BuildUrl(page);

            // Assert
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void BuildUrlAppendsNonAnonymousTypesToPathPortion() {
            // Arrange
            object[] pathParts = new object[] { "part", Decimal.One, 1.25f };
            var page = "home";

            // Act
            var actual = Util.BuildUrl(page, pathParts);

            // Assert
            Assert.AreEqual(actual, page + "/part/1/1.25");
        }

        [TestMethod]
        public void BuildUrlEncodesAppendedPathPortion() {
            // Arrange
            object[] pathParts = new object[] { "path portion", "ζ" };
            var page = "home";

            // Act
            var actual = Util.BuildUrl(page, pathParts);

            // Assert
            Assert.AreEqual(actual, page + "/path%20portion/%ce%b6");
        }

        [TestMethod]
        public void BuildUrlAppendsAnonymousObjectsToQueryString() {
            // Arrange
            var page = "home";
            var queryString = new { sort = "FName", dir = "desc" };

            // Act
            var actual = Util.BuildUrl(page, queryString);

            // Assert
            Assert.AreEqual(actual, page + "?sort=FName&dir=desc");
        }

        [TestMethod]
        public void BuildUrlAppendsMultipleAnonymousObjectsToQueryString() {
            // Arrange
            var page = "home";
            var queryString1 = new { sort = "FName", dir = "desc" };
            var queryString2 = new { view = "Activities", page = 7 };

            // Act
            var actual = Util.BuildUrl(page, queryString1, queryString2);

            // Assert
            Assert.AreEqual(actual, page + "?sort=FName&dir=desc&view=Activities&page=7");
        }

        [TestMethod]
        public void BuildUrlEncodesQueryStringKeysAndValues() {
            // Arrange
            var page = "home";
            var queryString = new { ζ = "my=value&", mykey = "<π" };

            // Act
            var actual = Util.BuildUrl(page, queryString);

            // Assert
            Assert.AreEqual(actual, page + "?%ce%b6=my%3dvalue%26&mykey=%3c%cf%80");
        }

        [TestMethod]
        public void BuildUrlGeneratesPathPartsAndQueryString() {
            // Arrange
            var page = "home";

            // Act
            var actual = Util.BuildUrl(page, "products", new { cat = 37 }, "furniture", new { sort = "name", dir = "desc" });

            // Assert
            Assert.AreEqual(actual, page + "/products/furniture?cat=37&sort=name&dir=desc");
        }

        [TestMethod]
        public void UrlAppRootTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                try {
                    Utils.CreateHttpContext("default.aspx", "http://localhost/");
                    Utils.CreateHttpRuntime("/");

                    var vpath = "~/";
                    var href = "~/world/test.aspx";
                    var expected = "world/test.aspx";
                    Assert.AreEqual(expected, Util.Url(vpath, href));
                    Assert.AreEqual(expected, new MockPage() { VirtualPath = vpath }.Href(href));

                }
                finally {
                    Utils.RestoreHttpContext();
                    Utils.RestoreHttpRuntime();
                }
            });
        }

        [TestMethod]
        public void UrlAnonymousObjectTest() {
            Assert.AreEqual("~/world/test.cshtml?Prop1=value1",
                Util.Url("~/world/", "test.cshtml", new { Prop1 = "value1" }));
            Assert.AreEqual("~/world/test.cshtml?Prop1=value1&Prop2=value2",
                Util.Url("~/world/", "test.cshtml", new { Prop1 = "value1", Prop2 = "value2" }));
        }

        [TestMethod]
        public void IsWithinAppRootNestedTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                Utils.CreateHttpRuntime("/subfolder1/website1");
                Assert.IsTrue(Util.IsWithinAppRoot("~/"));
                Assert.IsTrue(Util.IsWithinAppRoot("~/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("~/test/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("/subfolder1/website1"));
                Assert.IsTrue(Util.IsWithinAppRoot("/subfolder1/website1/"));
                Assert.IsTrue(Util.IsWithinAppRoot("/subfolder1/website1/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("/subfolder1/website1/test/default.cshtml"));

                Assert.IsFalse(Util.IsWithinAppRoot("/"));
                Assert.IsFalse(Util.IsWithinAppRoot("/subfolder1"));
                Assert.IsFalse(Util.IsWithinAppRoot("/subfolder1/"));
                Assert.IsFalse(Util.IsWithinAppRoot("/subfolder1/website2"));
                Assert.IsFalse(Util.IsWithinAppRoot("/subfolder2"));
            });
        }

        [TestMethod]
        public void IsWithinAppRootTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                Utils.CreateHttpRuntime("/website1");
                Assert.IsTrue(Util.IsWithinAppRoot("~/"));
                Assert.IsTrue(Util.IsWithinAppRoot("~/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("~/test/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("/website1"));
                Assert.IsTrue(Util.IsWithinAppRoot("/website1/"));
                Assert.IsTrue(Util.IsWithinAppRoot("/website1/default.cshtml"));
                Assert.IsTrue(Util.IsWithinAppRoot("/website1/test/default.cshtml"));

                Assert.IsFalse(Util.IsWithinAppRoot("/"));
                Assert.IsFalse(Util.IsWithinAppRoot("/website2"));
                Assert.IsFalse(Util.IsWithinAppRoot("/subfolder1/"));
            });
        }
    }

    // Dummy class to simulate exception from CompilationUtil.GetBuildProviderTypeFromExtension
    internal class CompilationUtil {
        public static void GetBuildProviderTypeFromExtension(string extension) {
            throw new HttpException(extension);
        }

        public static HttpException GetBuildProviderException(string extension) {
            try {
                CompilationUtil.GetBuildProviderTypeFromExtension(extension);
            }
            catch (HttpException e) {
                return e;
            }
            return null;
        }
    }
}
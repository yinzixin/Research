using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class ApplicationStartPageTest {
        [TestMethod]
        public void StartPageBasicTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var page = new ApplicationStartPageTest().CreateStartPage(p => {
                    p.AppState["x"] = "y";
                    p.WriteLiteral("test");
                });
                page.ExecuteInternal();
                Assert.AreEqual("y", page.ApplicationState["x"]);
                Assert.AreEqual("test", ApplicationStartPage.Markup.ToHtmlString());
            });
        }

        [TestMethod]
        public void StartPageDynamicAppStateBasicTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var page = new ApplicationStartPageTest().CreateStartPage(p => {
                    p.App.x = "y";
                    p.WriteLiteral("test");
                });
                page.ExecuteInternal();
                Assert.AreEqual("y", page.ApplicationState["x"]);
                Assert.AreEqual("y", page.App["x"]);
                Assert.AreEqual("y", page.App.x);
                Assert.AreEqual("test", ApplicationStartPage.Markup.ToHtmlString());
            });
        }

        [TestMethod]
        public void ExceptionTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var msg = "This is an error message";
                var e = new InvalidOperationException(msg);
                var page = new ApplicationStartPageTest().CreateStartPage(p => {
                    throw e;
                });
                ExceptionAssert.Throws<HttpException>(() => {
                    page.ExecuteStartPage();
                }, ex => ex.InnerException.Message == msg);
                Assert.AreEqual(e, ApplicationStartPage.Exception);
            });
        }

        [TestMethod]
        public void HtmlEncodeTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                // Set HideRequestResponse to true to simulate the condition in IIS 7/7.5
                var context = new HttpContext(new HttpRequest("default.cshtml", "http://localhost/default.cshtml", null), new HttpResponse(new StringWriter(new StringBuilder())));
                var hideRequestResponse = typeof(HttpContext).GetField("HideRequestResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                hideRequestResponse.SetValue(context, true);

                HttpContext.Current = context;
                var page = new ApplicationStartPageTest().CreateStartPage(p => {
                    p.Write("test");
                });
                page.ExecuteStartPage();
            });
        }

        [TestMethod]
        public void GetVirtualPathTest() {
            var page = new MockStartPage();
            Assert.AreEqual(ApplicationStartPage.StartPageVirtualPath, page.VirtualPath);
        }

        [TestMethod]
        public void SetVirtualPathTest() {
            var page = new MockStartPage();
            ExceptionAssert.Throws<NotSupportedException>(() => { page.VirtualPath = "~/hello.cshtml"; });
        }

        [TestMethod]
        public void ExecuteStartPageTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var startPage = new MockStartPage() { ExecuteAction = p => p.AppState["x"] = "y" };
                ApplicationStartPage.ExecuteStartPage(new WebPageHttpApplication(),
                    p => { },
                    path => path == "~/_appstart.vbhtml",
                    p => startPage,
                    new string[] { "cshtml", "vbhtml" });
                Assert.AreEqual("y", startPage.ApplicationState["x"]);
            });
        }

        [TestMethod]
        public void ExecuteStartPageDynamicAppStateTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var startPage = new MockStartPage() { ExecuteAction = p => p.App.x = "y" };
                ApplicationStartPage.ExecuteStartPage(new WebPageHttpApplication(),
                    p => { },
                    path => path == "~/_appstart.vbhtml",
                    p => startPage,
                    new string[] { "cshtml", "vbhtml" });
                Assert.AreEqual("y", startPage.ApplicationState["x"]);
                Assert.AreEqual("y", startPage.App.x);
                Assert.AreEqual("y", startPage.App["x"]);
            });

        }

        public class MockStartPage : ApplicationStartPage {
            public Action<ApplicationStartPage> ExecuteAction { get; set; }
            public HttpApplicationStateBase ApplicationState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
            public override void Execute() {
                ExecuteAction(this);
            }

            public override HttpApplicationStateBase AppState {
                get {
                    return ApplicationState;
                }
            }

            public void ExecuteStartPage() {
                ApplicationStartPage.ExecuteStartPage(new WebPageHttpApplication(),
                    p => { },
                    p => true,
                    p => this,
                    new string[] { "cshtml", "vbhtml" });
            }
        }

        public MockStartPage CreateStartPage(Action<ApplicationStartPage> action) {
            var startPage = new MockStartPage() { ExecuteAction = action };
            return startPage;
        }

        public sealed class WebPageHttpApplication : HttpApplication {
        }
    }
}

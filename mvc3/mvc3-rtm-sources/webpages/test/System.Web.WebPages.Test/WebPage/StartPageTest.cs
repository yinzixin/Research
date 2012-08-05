using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class StartPageTest {
        // The page ~/_pagestart.cshtml does the following:
        // this is the init page
        // 
        // The page ~/index.cshtml does the following:
        // hello world
        // Expected result:
        // this is the init page hello world
        [TestMethod] 
        public void InitPageBasicTest() {
            var init = Utils.CreateStartPage(p =>
                p.Write("this is the init page "));
            var page = Utils.CreatePage(p =>
                p.Write("hello world"));

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("this is the init page hello world", result);
        }


        // The page ~/_pagestart.cshtml does the following:
        // this is the init page
        // 
        // The page ~/folder1/index.cshtml does the following:
        // hello world
        // Expected result:
        // this is the init page hello world
        [TestMethod]
        public void InitSubfolderTest() {
            var init = Utils.CreateStartPage(p =>
                p.Write("this is the init page "));
            var page = Utils.CreatePage(p =>
                p.Write("hello world"), "~/folder1/index.cshtml");

            init.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("this is the init page hello world", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // PageData["Title"] = "InitPage";
        // Layout = "Layout.cshtml";
        // this is the init page
        // 
        // The page ~/index.cshtml does the following:
        // PageData["Title"] = "IndexCshtmlPage"
        // hello world
        //
        // The layout page ~/Layout.cshtml does the following:
        // layout start
        // @PageData["Title"]
        // @RenderBody()
        // layout end
        //
        // Expected result:
        // layout start IndexCshtmlPage this is the init page hello world layout end
        [TestMethod]
        public void InitPageLayoutTest() {
            var init = Utils.CreateStartPage(p => {
                p.Layout = "Layout.cshtml";
                p.Write(" this is the init page ");
                Assert.AreEqual("~/Layout.cshtml", p.Layout);
            });
            var page = LayoutTest.CreatePageWithLayout(
                p => {
                    p.PageData["Title"] = "IndexCshtmlPage";
                    p.Write("hello world");
                },
                p => {
                    p.Write("layout start ");
                    p.Write(p.PageData["Title"]);
                    p.WriteLiteral(p.RenderBody());
                    p.Write(" layout end");
                });

            init.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("layout start IndexCshtmlPage this is the init page hello world layout end", result);
        }

        // _pagestart.cshtml sets the LayoutPage to be null
        [TestMethod]
        public void InitPageNullLayoutPageTest() {
            var init1 = Utils.CreateStartPage(
                p => {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(
                p => {
                    p.Layout = null;
                    p.WriteLiteral("<init2>");
                    p.RunPage();
                    p.WriteLiteral("</init2>");
                }, init2path);
            var page = Utils.CreatePage(p =>
                p.Write("hello world"), "~/folder1/index.cshtml");
            var layoutPage = Utils.CreatePage(p =>
                p.Write("layout page"), "~/Layout.cshtml");

            page.PageInstances["~/Layout.cshtml"] = layoutPage;

            init1.ChildPage = init2;
            init2.ChildPage = page;

            var result = Utils.RenderWebPage(page, init1);
            Assert.AreEqual("<init1><init2>hello world</init2></init1>", result);
        }

        // _pagestart.cshtml sets the LayoutPage, but page sets it to null
        [TestMethod]
        public void PageSetsNullLayoutPageTest() {
            var init1 = Utils.CreateStartPage(
                p => {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var layoutPage = Utils.CreatePage(p =>
                p.Write("layout page"), "~/Layout.cshtml");
            var page = Utils.CreatePage(p => {
                p.Layout = null;
                p.Write("hello world");
            });
            page.PageInstances["~/Layout.cshtml"] = layoutPage;
            init1.ChildPage = page;
            var result = Utils.RenderWebPage(page, init1);
            Assert.AreEqual("<init1>hello world</init1>", result);
        }

        [TestMethod]
        public void PageSetsEmptyLayoutPageTest() {
            var init1 = Utils.CreateStartPage(
                p => {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var layoutPage = Utils.CreatePage(p =>
                p.Write("layout page"), "~/Layout.cshtml");
            var page = Utils.CreatePage(p => {
                p.Layout = "";
                p.Write("hello world");
            });
            page.PageInstances["~/Layout.cshtml"] = layoutPage;
            init1.ChildPage = page;
            var result = Utils.RenderWebPage(page, init1);
            Assert.AreEqual("<init1>hello world</init1>", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // init page start
        // @RunPage()
        // init page end
        // 
        // The page ~/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // init page start hello world init page end
        [TestMethod]
        public void RunPageTest() {
            var init = Utils.CreateStartPage(
                p => {
                    p.Write("init page start ");
                    p.RunPage();
                    p.Write(" init page end");
                });
            var page = Utils.CreatePage(p =>
                p.Write("hello world"));

            init.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("init page start hello world init page end", result);
        }
        
        // The page ~/_pagestart.cshtml does the following:
        // <init1>
        // @RunPage()
        // </init1>
        // 
        // The page ~/folder1/_pagestart.cshtml does the following:
        // <init2>
        // @RunPage()
        // </init2>
        // 
        // The page ~/folder1/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // <init1><init2>hello world</init2></init1>
        [TestMethod]
        public void NestedRunPageTest() {
            var init1 = Utils.CreateStartPage(
                p => {
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(
                p => {
                    p.WriteLiteral("<init2>");
                    p.RunPage();
                    p.WriteLiteral("</init2>");
                }, init2path);
            var page = Utils.CreatePage(p =>
                p.Write("hello world"), "~/folder1/index.cshtml");

            init1.ChildPage = init2;
            init2.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init1);
            Assert.AreEqual("<init1><init2>hello world</init2></init1>", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // PageData["key1"] = "value1";
        // 
        // The page ~/folder1/_pagestart.cshtml does the following:
        // PageData["key2"] = "value2";
        // 
        // The page ~/folder1/index.cshtml does the following:
        // @PageData["key1"] @PageData["key2"] @PageData["key3"]
        //
        // Expected result:
        // value1 value2
        [TestMethod]
        public void PageDataTest() {
            var init1 = Utils.CreateStartPage(p => p.PageData["key1"] = "value1");
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(p => p.PageData["key2"] = "value2", init2path);
            var page = Utils.CreatePage(
                p => {
                    p.Write(p.PageData["key1"]);
                    p.Write(" ");
                    p.Write(p.PageData["key2"]);
                },
                "~/folder1/index.cshtml");

            init1.ChildPage = init2;
            init2.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init1);
            Assert.AreEqual("value1 value2", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // init page
        // @RenderPage("subpage.cshtml", "init_data");
        //
        // The page ~/subpage.cshtml does the following:
        // subpage
        // @PageData[0]
        //
        // The page ~/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // init page subpage init_data hello world
        [TestMethod]
        public void RenderPageTest() {
            var init = Utils.CreateStartPage(
                p => {
                    p.Write("init page ");
                    p.Write(p.RenderPage("subpage.cshtml", "init_data"));
                });
            var subpagePath = "~/subpage.cshtml";
            var subpage = Utils.CreatePage(
                p => {
                    p.Write("subpage ");
                    p.Write(p.PageData[0]);
                }, subpagePath);
            var page = Utils.CreatePage(p =>
                p.Write(" hello world"));

            page.PageInstances[subpagePath] = subpage;

            init.ChildPage = page;
            
            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("init page subpage init_data hello world", result);
        }

        [TestMethod]
        // The page ~/_pagestart.cshtml does the following:
        // <init>
        // @{ 
        //     try {
        //         RunPage();
        //     } catch (Exception e) {
        //         Write("Exception: " + e.Message);
        //     }
        // }
        // </init>
        //
        // The page ~/index.cshtml does the following:
        // hello world
        // @{throw new InvalidOperation("exception from index.cshtml");}
        //
        // Expected result:
        // <init>hello world Exception: exception from index.cshtml</init>
        public void InitCatchExceptionTest() {
            var init = Utils.CreateStartPage(
                p => {
                    p.WriteLiteral("<init>");
                    try {
                        p.RunPage();
                    } catch (Exception e) {
                        p.Write("Exception: " + e.Message);
                    }
                    p.WriteLiteral("</init>");
                });
            var page = Utils.CreatePage(
                p => {
                    p.WriteLiteral("hello world ");
                    throw new InvalidOperationException("exception from index.cshtml");
                });

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.AreEqual("<init>hello world Exception: exception from index.cshtml</init>", result);
        }

        public class MockInitPage : MockStartPage {
            internal override Func<object> GetObjectFactory(string vpath) {
                // Simulate the call to validate the virtual path
                var validateVirtualPathMethod = typeof(BuildManager).GetMethod("ValidateVirtualPathInternal", BindingFlags.Instance | BindingFlags.NonPublic);
                validateVirtualPathMethod.Invoke(GetBuildManager(), new object[] { Utils.CreateVirtualPath(vpath), false, false });
                return null;
            }

            internal object GetBuildManager() {
                return typeof(BuildManager).GetField("_theBuildManager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            }
        }

        // Simulate a site that is nested, eg /subfolder1/website1
        [TestMethod]
        public void ExecuteWithinInitTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                Utils.CreateHttpRuntime("/subfolder1/website1");
                new System.Web.Hosting.HostingEnvironment();
                var stringSet = Activator.CreateInstance(typeof(BuildManager).Assembly.GetType("System.Web.Util.StringSet"), true);
                typeof(BuildManager).GetField("_forbiddenTopLevelDirectories", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(new MockInitPage().GetBuildManager(), stringSet); ;

                var init = new MockInitPage() {
                    VirtualPath = "~/_pagestart.cshtml",
                    ExecuteAction = p => { },
                };
                var page = Utils.CreatePage(p => { });
                page.PageInstances["~/_pagestart.cshtml"] = init;

                var result = Utils.RenderWebPage(page);
            });
        }

        [TestMethod]
        public void SetGetPropertiesTest() {

            var init = new MockInitPage();
            var page = new MockPage();
            init.ChildPage = page;

            // Context
            var context = new Mock<HttpContextBase>().Object;
            init.Context = context;
            Assert.AreEqual(context, init.Context);
            Assert.AreEqual(context, page.Context);

            // Request/Response/Server/Cache/Session/Application
            var request = new Mock<HttpRequestBase>().Object;
            var response = new Mock<HttpResponseBase>().Object;
            var server = new Mock<HttpServerUtilityBase>().Object;
            var cache = new System.Web.Caching.Cache();
            var app = new Mock<HttpApplicationStateBase>().Object;
            var session = new Mock<HttpSessionStateBase>().Object;

            var contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(c => c.Request).Returns(request);
            contextMock.Setup(c => c.Response).Returns(response);
            contextMock.Setup(c => c.Cache).Returns(cache);
            contextMock.Setup(c => c.Server).Returns(server);
            contextMock.Setup(c => c.Application).Returns(app);
            contextMock.Setup(c => c.Session).Returns(session);

            context = contextMock.Object;
            page.Context = context;
            Assert.AreEqual(request, init.Request);
            Assert.AreEqual(response, init.Response);
            Assert.AreEqual(cache, init.Cache);
            Assert.AreEqual(server, init.Server);
            Assert.AreEqual(session, init.Session);
            Assert.AreEqual(app, init.AppState);
        }

        [TestMethod]
        public void GetDirectoryTest() {
            var initPage = new Mock<StartPage>().Object;
            Assert.AreEqual("/website1/", initPage.GetDirectory("/website1/default.cshtml"));
            Assert.AreEqual("~/", initPage.GetDirectory("~/default.cshtml"));
            Assert.AreEqual("/", initPage.GetDirectory("/website1/"));
            Assert.AreEqual(null, initPage.GetDirectory("/"));
        }

        [TestMethod]
        public void GetStartPageTest() {
            var initPage = Utils.CreateStartPage(p => p.Write("<init>"), "~/_pagestart.vbhtml");
            var page = Utils.CreatePage(p => p.Write("test"));
            page.PageInstances["~/_pagestart.vbhtml"] = initPage;
            var result = StartPage.GetStartPage(page, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });
            Assert.AreEqual(initPage, result);
        }

        [TestMethod]
        public void GetStartPage_ThrowsOnNullPage() {
            ExceptionAssert.ThrowsArgNull(() => StartPage.GetStartPage(null, "name", new[] { "cshtml" }), "page");
        }

        [TestMethod]
        public void GetStartPage_ThrowsOnNullFileName() {
            var page = Utils.CreatePage(p => p.Write("test"));
            ExceptionAssert.ThrowsArgNullOrEmpty(() => StartPage.GetStartPage(page, null, new[] { "cshtml" }), "fileName");
        }

        [TestMethod]
        public void GetStartPage_ThrowsOnEmptyFileName() {
            var page = Utils.CreatePage(p => p.Write("test"));
            ExceptionAssert.ThrowsArgNullOrEmpty(() => StartPage.GetStartPage(page, String.Empty, new[] { "cshtml" }), "fileName");
        }

        [TestMethod]
        public void GetStartPage_ThrowsOnNullSupportedExtensions() {
            var page = Utils.CreatePage(p => p.Write("test"));
            ExceptionAssert.ThrowsArgNull(() => StartPage.GetStartPage(page, "name", null), "supportedExtensions");
        }

        [TestMethod]
        public void FileExistsTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var env = new HostingEnvironment();
                AppDomainUtils.SetAppData();
                var vpp = new Mock<VirtualPathProvider>();
                var vpath1 = "/website1/default.cshtml";
                var vpath2 = "/website2/default.cshtml";
                vpp.Setup(p => p.FileExists(vpath1)).Returns(true).Verifiable();
                vpp.Setup(p => p.FileExists(vpath2)).Returns(false).Verifiable();

                var register = typeof(HostingEnvironment).GetMethod("RegisterVirtualPathProviderInternal", BindingFlags.Static | BindingFlags.NonPublic);
                register.Invoke(null, new object[] { vpp.Object });

                var initPage = new Mock<StartPage>().Object;
                Assert.IsTrue(initPage.FileExists(vpath1));
                Assert.IsFalse(initPage.FileExists(vpath2));
                vpp.Verify();
            });
        }
    }
}

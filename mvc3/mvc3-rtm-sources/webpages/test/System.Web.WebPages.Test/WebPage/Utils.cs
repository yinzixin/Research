using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.WebPages.Test {
    public class Utils {
        public static string RenderWebPage(WebPage page, StartPage startPage = null) {
            var writer = new StringWriter();

            // Create an actual dummy HttpContext that has a request object
            var filename = "default.aspx";
            var url = "http://localhost/default.aspx";
            var request = new HttpRequest(filename, url, null);
            var httpContext = new HttpContext(request, new HttpResponse(new StringWriter(new StringBuilder())));

            var pageContext = Util.CreatePageContext(httpContext);

            page.ExecutePageHierarchy(pageContext, writer, startPage);

            return writer.ToString();
        }

        public static string RenderWebPage(Action<WebPage> pageExecuteAction, string pagePath = "~/index.cshtml") {
            var page = CreatePage(pageExecuteAction, pagePath);
            return RenderWebPage(page);
        }

        public static MockPage CreatePage(Action<WebPage> pageExecuteAction, string pagePath = "~/index.cshtml") {
            var page = new MockPage() {
                VirtualPath = pagePath,
                ExecuteAction = p => {
                    pageExecuteAction(p);
                }
            };
            return page;
        }

        public static MockStartPage CreateStartPage(Action<StartPage> pageExecuteAction, string pagePath = "~/_pagestart.cshtml") {
            var page = new MockStartPage() {
                VirtualPath = pagePath,
                ExecuteAction = p => {
                    pageExecuteAction(p);
                }
            };
            return page;
        }

        public static string RenderWebPageWithSubPage(Action<WebPage> pageExecuteAction, Action<WebPage> subpageExecuteAction,
            string pagePath = "~/index.cshtml", string subpagePath = "~/subpage.cshtml") {
            var page = CreatePage(pageExecuteAction);
            page.PageInstances[subpagePath] = CreatePage(subpageExecuteAction, subpagePath);
            return RenderWebPage(page);
        }

        internal static void CreateHttpRuntime(string appVPath) {
            var runtime = new HttpRuntime();
            var appDomainAppVPathField = typeof(HttpRuntime).GetField("_appDomainAppVPath", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            appDomainAppVPathField.SetValue(runtime, CreateVirtualPath(appVPath));
            GetTheRuntime().SetValue(null, runtime);
            var appDomainIdField = typeof(HttpRuntime).GetField("_appDomainId", BindingFlags.NonPublic | BindingFlags.Instance);
            appDomainIdField.SetValue(runtime, "test");
        }

        internal static FieldInfo GetTheRuntime() {
            return typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static);
        }

        internal static void RestoreHttpRuntime() {
            GetTheRuntime().SetValue(null, null);
        }

        // E.g. "default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"
        internal static void CreateHttpContext(string filename, string url) {
            var request = new HttpRequest(filename, url, null);
            var httpContext = new HttpContext(request, new HttpResponse(new StringWriter(new StringBuilder())));
            HttpContext.Current = httpContext;
        }

        internal static void RestoreHttpContext() {
            HttpContext.Current = null;
        }

        internal static object CreateVirtualPath(string path) {
            var vPath = typeof(Page).Assembly.GetType("System.Web.VirtualPath");
            var method = vPath.GetMethod("CreateNonRelativeTrailingSlash", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return method.Invoke(null, new object[] { path });
        }

        public static void SetupVirtualPathInAppDomain(string vpath, string contents) {
            var file = new Mock<VirtualFile>(vpath);
            file.Setup(f => f.Open()).Returns(new MemoryStream(ASCIIEncoding.Default.GetBytes(contents)));
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(p => p.FileExists(vpath)).Returns(true);
            vpp.Setup(p => p.GetFile(vpath)).Returns(file.Object);
            AppDomainUtils.SetAppData();
            var env = new HostingEnvironment();

            var register = typeof(HostingEnvironment).GetMethod("RegisterVirtualPathProviderInternal", BindingFlags.Static | BindingFlags.NonPublic);
            register.Invoke(null, new object[] { vpp.Object });
        }
    }

    public class MockPageHelper {
        internal static Func<object> GetObjectFactory(string vpath, Dictionary<string, object> pageInstances) {
            if (pageInstances.ContainsKey(vpath)) {
                return () => pageInstances[vpath];
            }
            else {
                return null;
            }
        }

        internal static string GetDirectory(string virtualPath) {
            var dir = Path.GetDirectoryName(virtualPath);
            if (dir == "~") {
                return null;
            }
            return dir;
        }
    }

    // This is a mock implementation of WebPage mainly to make the Render method work and
    // generate a string.
    // The Execute method simulates what is typically generated based on markup by the parsers.
    public class MockPage : WebPage {
        public Dictionary<string, object> PageInstances = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public Action<WebPage> ExecuteAction { get; set; }

        internal override Func<object> GetObjectFactory(string vpath) {
            return MockPageHelper.GetObjectFactory(vpath, PageInstances);
        }

        internal override string GetDirectory(string virtualPath) {
            return MockPageHelper.GetDirectory(virtualPath);
        }

        internal override bool FileExists(string path, bool useCache) {
            return PageInstances.ContainsKey(path);
        }

        public override void Execute() {
            ExecuteAction(this);
        }

        internal override WebPageBase CreatePageFromVirtualPath(string path) {
            return PageInstances[path] as WebPageBase;
        }
    }

    public class MockStartPage : StartPage {
        public Dictionary<string, object> PageInstances = new Dictionary<string, object>();
        public Action<StartPage> ExecuteAction { get; set; }

        internal override Func<object> GetObjectFactory(string vpath) {
            return MockPageHelper.GetObjectFactory(vpath, CombinedPageInstances);
        }

        internal override string GetDirectory(string virtualPath) {
            return MockPageHelper.GetDirectory(virtualPath);
        }

        internal override bool FileExists(string path, bool useCache) {
            return CombinedPageInstances.ContainsKey(path);
        }

        public override void Execute() {
            ExecuteAction(this);
        }

        public Dictionary<string, object> CombinedPageInstances {
            get {
                var combinedInstances = new Dictionary<string, object>();
                var instances = new Dictionary<string, object>();
                WebPageRenderingBase childPage = this;
                while (childPage != null) {
                    if (childPage is MockStartPage) {
                        var initPage = childPage as MockStartPage;
                        instances = initPage.PageInstances;
                        childPage = initPage.ChildPage;
                    }
                    else if (childPage is MockPage) {
                        instances = ((MockPage)childPage).PageInstances;
                        childPage = null;
                    }
                    foreach (var kvp in instances) {
                        combinedInstances.Add(kvp.Key, kvp.Value);
                    }
                }
                return combinedInstances;
            }
        }
    }

    public class MockHttpRuntime {
        public static Version RequestValidationMode {
            get;
            set;
        }
    }

    public class MockHttpApplication {
        public static Type ModuleType { get; set; }
        public static void RegisterModule(Type moduleType) {
            ModuleType = moduleType;
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.SessionState;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

namespace System.Web.WebPages {
    public class WebPageHttpHandler : IHttpHandler, IRequiresSessionState {
        internal const string StartPageFileName = "_PageStart";

        private readonly WebPage _webPage;
        private static List<string> _supportedExtensions = new List<string>();
        internal static readonly string WebPagesVersion = GetVersionString();
        public static readonly string WebPagesVersionHeaderName = "X-AspNetWebPages-Version";

        public WebPageHttpHandler(WebPage webPage) {
            if (webPage == null) {
                throw new ArgumentNullException("webPage");
            }
            _webPage = webPage;
        }

        public static bool DisableWebPagesResponseHeader {
            get;
            set;
        }

        public virtual bool IsReusable {
            get {
                return false;
            }
        }

        internal static void AddVersionHeader(HttpContextBase httpContext) {
            if (!DisableWebPagesResponseHeader) {
                httpContext.Response.AppendHeader(WebPagesVersionHeaderName, WebPagesVersion);
            }
        }
        public static IHttpHandler CreateFromVirtualPath(string virtualPath) {
            return CreateFromVirtualPath(virtualPath, VirtualPathFactoryManager.Instance);
        }

        // For testing purpose
        internal static IHttpHandler CreateFromVirtualPath(string virtualPath, Func<string, Type, object> createInstanceMethod) {
            return CreateFromVirtualPath(virtualPath, VirtualPathFactoryManager.CreateFromLambda(virtualPath, createInstanceMethod));
        }

        internal static IHttpHandler CreateFromVirtualPath(string virtualPath, VirtualPathFactoryManager virtualPathFactoryManager) {
            // Instantiate the page from the virtual path
            object instance = virtualPathFactoryManager.CreateInstance<object>(virtualPath);

            WebPage page = instance as WebPage;

            // If it's not a page, assume it's a regular handler
            if (page == null) {
                return (IHttpHandler)instance;
            }

            // Mark it as a 'top level' page (as opposed to a user control or master)
            page.TopLevelPage = true;

            // Give it its virtual path
            page.VirtualPath = virtualPath;

            // Return a handler over it
            return new WebPageHttpHandler(page);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "We don't want a property")]
        public static ReadOnlyCollection<string> GetRegisteredExtensions() {
            return new ReadOnlyCollection<string>(_supportedExtensions);
        }

        private static string GetVersionString() {
            // DevDiv 216459:
            // This code originally used Assembly.GetName(), but that requires FileIOPermission, which isn't granted in
            // medium trust. However, Assembly.FullName *is* accessible in medium trust.
            return new AssemblyName(typeof(WebPageHttpHandler).Assembly.FullName).Version.ToString(2);
        }

        private static bool HandleError(Exception e) {
            // This method is similar to System.Web.UI.Page.HandleError

            // Don't touch security exception
            if (e is System.Security.SecurityException) {
                return false;
            }

            throw new HttpUnhandledException(null, e);
        }

        internal static void GenerateSourceFilesHeader(WebPageContext context) {
            if (context.SourceFiles.Any()) {
                var files = String.Join("|", context.SourceFiles);
                // Since the characters in the value are files that may include characters outside of the ASCII set, header encoding as specified in RFC2047. 
                // =?<charset>?<encoding>?...?= 
                // In the following case, UTF8 is used with base64 encoding 
                var encodedText = "=?UTF-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(files)) + "?=";
                context.HttpContext.Response.AddHeader("X-SourceFiles", encodedText);
            }
        }

        public virtual void ProcessRequest(HttpContext context) {
            // Dev10 bug 921943 - Plan9 should lower its permissions if operating in legacy CAS
            SecurityUtil.ProcessInApplicationTrust(() => {
                ProcessRequestInternal(context);
            });
        }

        internal void ProcessRequestInternal(HttpContext context) {
            // enable dynamic validation for this request
            ValidationUtility.EnableDynamicValidation(context);
            context.Request.ValidateInput();

            try {
                HttpContextBase contextBase = new HttpContextWrapper(context);
                //WebSecurity.Context = contextBase;
                AddVersionHeader(contextBase);

                WebPageRenderingBase startPage = StartPage.GetStartPage(_webPage, StartPageFileName, WebPageHttpHandler.GetRegisteredExtensions());

                // This is also the point where a Plan9 request truly begins execution

                // We call ExecutePageHierarchy on the requested page, passing in the possible initPage, so that
                // the requested page can take care of the necessary push/pop context and trigger the call to
                // the initPage.
                _webPage.ExecutePageHierarchy(Util.CreatePageContext(context), context.Response.Output, startPage);

                if (ShouldGenerateSourceHeader(contextBase)) {
                    GenerateSourceFilesHeader(_webPage.PageContext);
                }
            }
            catch (Exception e) {
                if (!HandleError(e)) {
                    throw;
                }
            }
        }

        public static void RegisterExtension(string extension) {
            // Note: we don't lock or check for duplicates because we only expect this method to be called during PreAppStart
            _supportedExtensions.Add(extension);
        }

        internal static bool ShouldGenerateSourceHeader(HttpContextBase context) {
            return context.Request.IsLocal;
        }
    }
}

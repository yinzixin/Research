using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Web.WebPages.Resources;

/*
WebPage class hierarchy

WebPageExecutingBase                        The base class for all Plan9 files (_pagestart, _appstart, and regular pages)
    AppStartBase                            Used for _appstart.cshtml
    WebPageRenderingBase
        PageStartBase                       Used for _pagestart.cshtml
        WebPageBase
            WebPage                         Plan9Pages
            ViewWebPage?                    MVC Views
*/

namespace System.Web.WebPages {
    // The base class for all CSHTML files (_pagestart, _appstart, and regular pages)
    public abstract class WebPageExecutingBase {
        private DynamicHttpApplicationState _dynamicAppState;

        public virtual HttpApplicationStateBase AppState {
            get {
                if (Context != null) {
                    return Context.Application;
                }
                return null;
            }
        }

        public virtual dynamic App {
            get {
                if (_dynamicAppState == null && AppState != null) {
                    _dynamicAppState = new DynamicHttpApplicationState(AppState);
                }
                return _dynamicAppState;
            }
        }

        public virtual HttpContextBase Context {
            get;
            set;
        }

        public virtual string VirtualPath {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void Execute();

        internal virtual bool FileExists(string path) {
            return FileExists(path, false);
        }

        internal virtual bool FileExists(string path, bool useCache) {
            return VirtualPathFactoryManager.Instance.PageExists(path, useCache);
        }

        internal virtual string GetDirectory(string virtualPath) {
            return VirtualPathUtility.GetDirectory(virtualPath);
        }

        // These are virtual instance methods in order for unit tests to override them.
        internal virtual Func<object> GetObjectFactory(string virtualPath) {
            return () => VirtualPathFactoryManager.Instance.CreateInstance<object>(virtualPath);
        }

        internal virtual string NormalizeLayoutPagePath(string layoutPage) {
            return NormalizeLayoutPagePath(layoutPage, FileExists);
        }

        internal string NormalizeLayoutPagePath(string layoutPage, Func<string, bool> fileExists) {
            var path = NormalizePath(layoutPage);
            // Look for it as specified, either absolute, relative or same folder
            if (fileExists(path)) {
                return path;
            }
            throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_LayoutPageNotFound, layoutPage, path));
        }

        public virtual string NormalizePath(string path) {
            // If it's relative, resolve it
            return VirtualPathUtility.Combine(VirtualPath, path);
        }

        public abstract void Write(HelperResult result);

        public abstract void Write(object value);

        public abstract void WriteLiteral(object value);

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, HelperResult content) {
            if (content != null) {
                content.WriteTo(writer);
            }
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, object content) {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteLiteralTo(TextWriter writer, object content) {
            writer.Write(content);
        }
    }
}

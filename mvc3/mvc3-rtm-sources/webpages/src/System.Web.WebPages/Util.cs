using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.UI;
using System.Web.WebPages.Resources;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages {
    internal static class Util {
        private static Control s_helperControl = CreateHelperControl();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is used by the caller and does not go out of scope")]
        private static Control CreateHelperControl() {
            var control = new Control();
            control.AppRelativeTemplateSourceDirectory = "~/";
            return control;
        }

        public static string Url(string basePath, string path, params object[] pathParts) {
            if (basePath != null) {
                path = VirtualPathUtility.Combine(basePath, path);
            }

            // Make sure it's not a ~/ path, which the client couldn't handle
            if (HttpRuntime.AppDomainAppVirtualPath != null) {
                path = s_helperControl.ResolveClientUrl(path);
            }

            return BuildUrl(path, pathParts);
        }

        internal static string BuildUrl(string path, params object[] pathParts) {
            path = HttpUtility.UrlPathEncode(path);
            StringBuilder queryString = new StringBuilder();

            foreach (var pathPart in pathParts) {

                Type partType = pathPart.GetType();
                if (IsDisplayableType(partType)) {
                    var displayablePath = Convert.ToString(pathPart, CultureInfo.InvariantCulture);
                    path += "/" + HttpUtility.UrlPathEncode(displayablePath);
                }
                else {
                    // If it smells like an anonymous object, treat it as query string name/value pairs instead of path info parts
                    // REVIEW: this is hacky!

                    foreach (var propInfo in partType.GetProperties()) {
                        if (queryString.Length == 0) {
                            queryString.Append('?');
                        }
                        else {
                            queryString.Append('&');
                        }

                        object value = propInfo.GetValue(pathPart, null);
                        string stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);

                        queryString.Append(HttpUtility.UrlEncode(propInfo.Name))
                                .Append('=')
                                .Append(HttpUtility.UrlEncode(stringValue));
                    }
                }
            }
            return path + queryString;
        }

        internal static bool IsWithinAppRoot(string vpath) {
            if (HttpRuntime.AppDomainAppVirtualPath == null) {
                // If the runtime has not been initialized, just return true.
                return true;
            }

            var absPath = vpath;
            if (!VirtualPathUtility.IsAbsolute(absPath)) {
                absPath = VirtualPathUtility.ToAbsolute(absPath);
            }
            // We need to call this overload because it returns null if the path is not within the application root.
            // The overload calls into MakeVirtualPathAppRelative(string virtualPath, string applicationPath, bool nullIfNotInApp), with 
            // nullIfNotInApp set to true.
            return VirtualPathUtility.ToAppRelative(absPath, HttpRuntime.AppDomainAppVirtualPath) != null;
        }

        private static bool IsDisplayableType(Type t) {
            // If it doesn't support any interfaces (e.g. IFormattable), we probably can't display it.  It's likely an anonymous type.
            // REVIEW: this is hacky!
            return t.GetInterfaces().Length > 0;
        }

        internal static WebPageContext CreatePageContext(HttpContext context) {
            var httpContext = new HttpContextWrapper(context);
            return CreatePageContext(httpContext);
        }

        internal static WebPageContext CreatePageContext(HttpContextBase httpContext) {
            var pageContext = new WebPageContext() {
                HttpContext = httpContext,
            };
            return pageContext;
        }

        internal static WebPageContext CreateNestedPageContext<TModel>(WebPageContext parentContext, IDictionary<object, dynamic> pageData, TModel model, bool isLayoutPage) {
            var nestedContext = new WebPageContext() {
                HttpContext = parentContext.HttpContext,
                OutputStack = parentContext.OutputStack,
                PageData = pageData,
                Model = model,
            };
            if (isLayoutPage) {
                nestedContext.BodyAction = parentContext.BodyAction;
                nestedContext.SectionWritersStack = parentContext.SectionWritersStack;
            }
            return nestedContext;
        }

        // Returns true if the path is simply "MyPath", and not app-relative "~/MyPath" or absolute "/MyApp/MyPath" or relative "../Test/MyPath"
        internal static bool IsSimpleName(string path) {
            if (VirtualPathUtility.IsAbsolute(path) || VirtualPathUtility.IsAppRelative(path)) {
                return false;
            }
            if (path.StartsWith(".", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            return true;
        }

        // This code is copied from http://www.liensberger.it/web/blog/?p=191
        internal static bool IsAnonymousType(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                       && type.IsGenericType && type.Name.Contains("AnonymousType")
                       && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                       && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        // Given an object of anonymous type, add each property as a key and associated with its value to the given dictionary.
        internal static void AddAnonymousTypeObjectToDictionary(IDictionary<string, object> dictionary, object o) {
            // Create a key for each property, and associate with the value.
            // E.g. new { X = "xvalue" } will add a { X, "xvalue" } entry.

            // TODO(elipton): Switch this to use RouteValueDictionary?

            foreach (var p in o.GetType().GetProperties()) {
                var key = p.Name;
                object value = p.GetValue(o, BindingFlags.Public | BindingFlags.Instance, null, null, null);
                dictionary.Add(key, value);
            }
        }

        // Checks the exception to see if it is from CompilationUtil.GetBuildProviderTypeFromExtension, which will throw
        // an exception about an unsupported extension. 
        // Actual error format: There is no build provider registered for the extension '.txt'. You can register one in the <compilation><buildProviders> section in machine.config or web.config. Make sure is has a BuildProviderAppliesToAttribute attribute which includes the value 'Web' or 'All'. 
        internal static bool IsUnsupportedExtensionError(HttpException e) {
            Exception exception = e;

            // Go through the layers of exceptions to find if any of them is from GetBuildProviderTypeFromExtension
            while (exception != null) {
                var site = exception.TargetSite;
                if (site != null && site.Name == "GetBuildProviderTypeFromExtension" && site.DeclaringType != null && site.DeclaringType.Name == "CompilationUtil") {
                    return true;
                }
                exception = exception.InnerException;
            }
            return false;
        }

        internal static void ThrowIfUnsupportedExtension(string virtualPath, HttpException e) {
            if (Util.IsUnsupportedExtensionError(e)) {
                var extension = Path.GetExtension(virtualPath);
                throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
            }
        }

        internal static void ThrowIfCodeDomDefinedExtension(string virtualPath, HttpException e) {
            if (e is HttpCompileException) {
                var extension = Path.GetExtension(virtualPath);
                if (InfrastructureHelper.IsCodeDomDefinedExtension(extension)) {
                    throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
                }
            }
        }

        internal static void EnsureValidPageType(WebPageBase page, string virtualPath) {

            bool error = true;
            try {
                if (page.FileExists(virtualPath)) {
                    var factory = page.GetObjectFactory(virtualPath);
                    if (factory != null) {
                        var result = factory();
                        if (result != null && typeof(WebPageBase).IsAssignableFrom(result.GetType())) {
                            error = false;
                        }
                    }
                }
            }
            catch (HttpException e) {
                // If the path uses an unregistered extension, such as Foo.txt,
                // then an error regarding build providers will be thrown.
                // Check if this is the case and throw a simpler error.
                ThrowIfUnsupportedExtension(virtualPath, e);

                // If the path uses an extension registered with codedom, such as Foo.js,
                // then an unfriendly compilation error might get thrown by the underlying compiler.
                // Check if this is the case and throw a simpler error.
                ThrowIfCodeDomDefinedExtension(virtualPath, e);

                // Rethrow any errors
                throw;
            }

            if (error) {
                // The page is missing, could not be compiled or is of an invalid type.
                throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_InvalidPageType, virtualPath));
            }
        }

       
    }
}

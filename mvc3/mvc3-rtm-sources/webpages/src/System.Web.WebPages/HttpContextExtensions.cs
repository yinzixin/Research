using System;
using System.Web;

namespace System.Web.WebPages {
    public static class HttpContextExtensions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Response.Redirect() takes its URI as a string parameter.")]
        public static void RedirectLocal(this HttpContextBase context, string url) {
            if (context.Request.IsUrlLocalToHost(url)) {
                context.Response.Redirect(url);
            }
            else {
                context.Response.Redirect("~/");
            }
        }

        public static void RegisterForDispose(this HttpContextBase context, IDisposable resource) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            RequestResourceTracker.RegisterForDispose(context, resource);
        }
    }
}

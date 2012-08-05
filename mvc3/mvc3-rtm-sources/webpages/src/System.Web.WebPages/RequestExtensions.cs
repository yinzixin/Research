namespace System.Web.WebPages {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Web;
    using System.Diagnostics.CodeAnalysis;

    public static class RequestExtensions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Response.Redirect() takes its URI as a string parameter.")]
        public static bool IsUrlLocalToHost(this HttpRequestBase request, string url) {
            if (url.IsEmpty()) {
                return false;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri)) {
                return String.Equals(request.Url.Host, absoluteUri.Host, StringComparison.OrdinalIgnoreCase);
            }
            else {
                bool isLocal = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                    && Uri.IsWellFormedUriString(url, UriKind.Relative);
                return isLocal;
            }
        }

    }
}

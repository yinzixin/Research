using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Helpers.Resources;

namespace Microsoft.Web.Helpers {

    // Validates under XHTML 1.0, but not XHTML 1.1 due to use of iframe in the noscript rendering.
    // HTML5 validation fails when noscript is inside a form, div, table, etc; but it needs to be in a
    // form for post-back of input fields.  HTML5 should also fail because of the iframe's frameborder.
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Re", Justification="Matches the recaptcha.net name")]
    public static class ReCaptcha {
        private const string _reCaptchaUrl = "http://api.recaptcha.net/";
        private const string _reCaptchaSecureUrl = "https://api-secure.recaptcha.net/";
        private const string _reCaptchaVerifyUrl = "http://api-verify.recaptcha.net/verify";
        private static readonly object _errorCodeCacheKey = new object();
        internal static readonly object _privateKey = new object();
        internal static readonly object _publicKey = new object();

        public static string PrivateKey {
            get {
                return ScopeStorage.CurrentScope[_privateKey] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_privateKey] = value;
            }
        }

        public static string PublicKey {
            get {
                return ScopeStorage.CurrentScope[_publicKey] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_publicKey] = value;
            }
        }

        

        /// <summary>
        /// Render a ReCaptcha widget to validate whether the user is human.
        /// </summary>
        /// <remarks>
        /// Register for keys at <a href="http://recaptcha.net/whyrecaptcha.html">ReCaptcha.net</a>.
        /// 
        /// See the
        /// <a href="http://recaptcha.net/apidocs/captcha/client.html#customization">ReCaptcha Client API</a>
        /// for more details on available themes and languages.
        /// </remarks>
        /// <param name="publicKey">ReCaptcha.net public key. Optional if ReCaptcha.PrivateKey was set.</param>
        /// <param name="theme">ReCaptcha theme name</param>
        /// <param name="language">ReCaptcha language code</param>
        /// <param name="tabIndex">Tab index for the ReCaptcha textbox</param>
        /// <returns>Helper for rendering HTML</returns>
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        public static IHtmlString GetHtml(string publicKey = null, string theme = "red",
            string language = "en", int tabIndex = 0) {

            return GetHtmlWithOptions(publicKey, options: new Dictionary<string, object>() {
                { "theme", theme }, { "lang", language }, { "tabindex", tabIndex }
            });
        }

        /// <summary>
        /// Render a ReCaptcha widget to validate whether the user is human.
        /// </summary>
        /// <remarks>
        /// Register for keys at <a href="http://recaptcha.net/whyrecaptcha.html">ReCaptcha.net</a>.
        /// 
        /// See the
        /// <a href="http://recaptcha.net/apidocs/captcha/client.html#customization">ReCaptcha Client API</a>
        /// for more details on available ReCaptcha options.
        /// </remarks>
        /// <example>
        /// <code>
        /// ReCaptcha.GetHtml(options: new {
        ///     theme = "white",
        ///     lang = "es",
        ///     tabindex = 5
        /// });
        /// </code>
        /// </example>
        /// <param name="options">ReCaptcha client options</param>
        /// <param name="publicKey">ReCaptcha.net public key</param>
        /// <returns>Helper for rendering HTML</returns>
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        public static IHtmlString GetHtmlWithOptions(string publicKey = null, object options = null) {
            return GetHtml(HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current), publicKey, options);
        }

        /// <summary>
        /// Validate the response posted back for the ReCaptcha widget.
        /// </summary>
        /// <remarks>
        /// Register for keys at <a href="http://recaptcha.net/whyrecaptcha.html">ReCaptcha.net</a>.
        /// </remarks>
        /// <param name="privateKey">ReCaptcha private key. Optional if ReCaptcha.PrivateKey was set.</param>
        /// <returns>True if response is valid, false otherwise</returns>
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        public static bool Validate(string privateKey = null) {
            return Validate(HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current), privateKey);
        }

        internal static string GetLastError(HttpContextBase context) {
            if (context.Items.Contains(_errorCodeCacheKey)) {
                return context.Items[_errorCodeCacheKey] as string;
            }
            return String.Empty;
        }

        internal static string GetValidatePostData(HttpContextBase context, string privateKey) {
            string remoteIP = context.Request.ServerVariables["REMOTE_ADDR"];
            if (String.IsNullOrEmpty(remoteIP)) {
                throw new InvalidOperationException(HelpersToolkitResources.ReCaptcha_RemoteIPNotFound);
            }

            // Noscript rendering requires the user to copy and paste the challenge string to a textarea.
            // When the challenge is invalid the recaptcha service doesn't return an error that affects
            // UI rendering, so Validate should just return false without issuing the web request.
            string challenge = context.Request.Form["recaptcha_challenge_field"];
            if (String.IsNullOrEmpty(challenge)) {
                return String.Empty;
            }
            string response = (context.Request.Form["recaptcha_response_field"] ?? String.Empty).Trim();

            return String.Format(CultureInfo.InvariantCulture,
                "privatekey={0}&remoteip={1}&challenge={2}&response={3}",
                privateKey,
                context.Request.ServerVariables["REMOTE_ADDR"],
                HttpUtility.HtmlEncode(challenge),
                HttpUtility.HtmlEncode(response));
        }

        internal static bool HandleValidateResponse(HttpContextBase context, string response) {
            if (!String.IsNullOrEmpty(response)) {
                string[] results = response.Split('\n');
                if (results.Length > 0) {
                    bool rval = Convert.ToBoolean(results[0], CultureInfo.InvariantCulture);
                    if (!rval && (results.Length > 1)) {
                        SetLastError(context, results[1]);
                    }
                    return rval;
                }
            }
            return false;
        }

        internal static IHtmlString GetHtml(HttpContextBase context, string publicKey = null, object options = null) {
            var optionsScript = GetOptionsScriptHtml(options);

            TagBuilder renderScript = new TagBuilder("script");
            renderScript.MergeAttribute("type", "text/javascript");
            renderScript.MergeAttribute("src", GetChallengeUrl(context, publicKey, GetLastError(context)));

            var noScript = GetNoScriptHtml(context, publicKey);

            return new HtmlString(optionsScript + renderScript + noScript);
        }

        internal static bool Validate(HttpContextBase context, string privateKey) {
            privateKey = privateKey ?? PrivateKey;

            if (String.IsNullOrEmpty(privateKey)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "privateKey");
            }

            SetLastError(context, String.Empty);
            string postedBody = GetValidatePostData(context, privateKey);
            if (String.IsNullOrEmpty(postedBody)) {
                return false;
            }
            string result = ExecuteValidateRequest(postedBody);
            return HandleValidateResponse(context, result);
        }

        private static string ExecuteValidateRequest(string formData) {
            WebRequest request = WebRequest.Create(_reCaptchaVerifyUrl);
            request.Method = "POST";
            request.Timeout = 5000;//milliseconds
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] content = Encoding.UTF8.GetBytes(formData);
            using (Stream stream = request.GetRequestStream()) {
                stream.Write(content, 0, content.Length);
            }
            using (WebResponse response = request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (MemoryStream memStream = new MemoryStream()) {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        do {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            memStream.Write(buffer, 0, bytesRead);
                        }
                        while (bytesRead > 0);
                        return Encoding.UTF8.GetString(memStream.ToArray());
                    }
                }
            }
        }

        private static string GetChallengeUrl(HttpContextBase context, string publicKey = null,
            string errorCode = null) {

            return GetUrlHelper(context, "challenge", publicKey, errorCode: errorCode);
        }

        private static string GetOptionsScriptHtml(object options) {
            if (options != null) {
                TagBuilder script = new TagBuilder("script");
                script.MergeAttribute("type", "text/javascript");
                script.InnerHtml = "var RecaptchaOptions=" + Json.Encode(options);
                return script.ToString();
            }
            return String.Empty;
        }

        private static string GetUrlHelper(HttpContextBase context, string path, string publicKey,
            string errorCode = null) {

            publicKey = publicKey ?? PublicKey;
            if (String.IsNullOrEmpty(publicKey)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "publicKey");
            }

            StringBuilder sb = new StringBuilder(context.Request.IsSecureConnection ? _reCaptchaSecureUrl : _reCaptchaUrl);
            sb.Append(path);
            sb.Append("?k=");
            sb.Append(publicKey);
            if (!String.IsNullOrEmpty(errorCode)) {
                sb.Append("&error=");
                sb.Append(errorCode);
            }
            return sb.ToString();
        }

        private static string GetNoScriptHtml(HttpContextBase context, string publicKey) {
            TagBuilder noscript = new TagBuilder("noscript");

            TagBuilder iframe = new TagBuilder("iframe");
            iframe.MergeAttribute("src", GetUrlHelper(context, "noscript", publicKey));
            iframe.MergeAttribute("height", "300");
            iframe.MergeAttribute("width", "500");
            iframe.MergeAttribute("frameborder", "0");
            noscript.InnerHtml += iframe.ToString();

            TagBuilder br = new TagBuilder("br");
            noscript.InnerHtml += br.ToString();

            TagBuilder textarea = new TagBuilder("textarea");
            textarea.MergeAttribute("name", "recaptcha_challenge_field");
            textarea.MergeAttribute("rows", "3");
            textarea.MergeAttribute("cols", "40");
            noscript.InnerHtml += textarea.ToString();

            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("name", "recaptcha_response_field");
            input.MergeAttribute("type", "hidden");
            input.MergeAttribute("value", "manual_challenge");
            noscript.InnerHtml += input.ToString();

            return noscript.ToString();
        }

        private static void SetLastError(HttpContextBase context, string value) {
            context.Items[_errorCodeCacheKey] = value;
        }
    }
}

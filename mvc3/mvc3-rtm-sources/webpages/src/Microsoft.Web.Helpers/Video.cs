namespace Microsoft.Web.Helpers {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Microsoft.Internal.Web.Utils;
    using Microsoft.Web.Helpers.Resources;
    using System.Web.WebPages;

    public static class Video {

        private const string FlashCab =
            "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab";
        private const string FlashClassId = "clsid:d27cdb6e-ae6d-11cf-96b8-444553540000";
        private const string FlashMimeType = "application/x-shockwave-flash";
        private const string MediaPlayerClassId = "clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6";
        private const string MediaPlayerMimeType = "application/x-mplayer2";
        private const string OleMimeType = "application/x-oleobject";
        private const string SilverlightMimeType = "application/x-silverlight-2";

        // These attributes can't be specified using anonymous objects (either because they are available as separate arguments or because
        // they don't make sense in the context of the helper).
        private static string[] GlobalBlacklist = new string[] { "width", "height", "type", "data", "classid", "codebase" };
        private static string[] MediaPlayerBlacklist = new string[] { "autoStart", "playCount", "uiMode", "stretchToFit", "enableContextMenu", "mute", "volume", "baseURL" };
        private static string[] SilverlightBlacklist = new string[] { "background", "initparams", "minruntimeversion", "autoUpgrade" };
        private static string[] FlashBlacklist = new string[] { "play", "loop", "menu", "bgColor", "quality", "scale", "wmode", "base" };

        private static VirtualPathUtilityWrapper _pathUtility = new VirtualPathUtilityWrapper();

#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        private static HttpContextBase HttpContext {
            get {
                var httpContext = System.Web.HttpContext.Current;
                return httpContext == null ? null : new HttpContextWrapper(httpContext);
            }
        }


        // see: http://kb2.adobe.com/cps/127/tn_12701.html
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "10#",
            Justification = "string parameter passed to flash in object tag")]
        public static HelperResult Flash(string path, string width = null, string height = null,
            bool play = true, bool loop = true, bool menu = true, string bgColor = null,
            string quality = null, string scale = null, string windowMode = null, string baseUrl = null,
            string version = null, object options = null, object htmlAttributes = null, string embedName = null) {

            return Flash(HttpContext, _pathUtility, path, width, height, play, loop, menu, bgColor,
            quality, scale, windowMode, baseUrl, version, options, htmlAttributes, embedName);
        }

        // see: http://msdn.microsoft.com/en-us/library/aa392321
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "10#",
            Justification = "string parameter passed to media player in object tag")]
        public static HelperResult MediaPlayer(string path, string width = null, string height = null,
            bool autoStart = true, int playCount = 1, string uiMode = null, bool stretchToFit = false,
            bool enableContextMenu = true, bool mute = false, int volume = -1, string baseUrl = null,
            object options = null, object htmlAttributes = null, string embedName = null) {

            return MediaPlayer(HttpContext, _pathUtility, path, width, height, autoStart, playCount, uiMode, stretchToFit,
                enableContextMenu, mute, volume, baseUrl, options, htmlAttributes, embedName);
        }

        // should users really use Silverlight.js?
        // see: http://msdn.microsoft.com/en-us/library/cc838259(v=VS.95).aspx
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        public static HelperResult Silverlight(string path, string width, string height,
            string bgColor = null, string initParameters = null, string minimumVersion = null, bool autoUpgrade = true,
            object options = null, object htmlAttributes = null) {

            return Silverlight(HttpContext, _pathUtility, path, width, height, bgColor, initParameters, minimumVersion, autoUpgrade,
                options, htmlAttributes);
        }

        internal static HelperResult Flash(HttpContextBase context, VirtualPathUtilityBase pathUtility, string path,
            string width = null, string height = null, bool play = true, bool loop = true, bool menu = true,
            string bgColor = null, string quality = null, string scale = null, string windowMode = null,
            string baseUrl = null, string version = null, object options = null, object htmlAttributes = null, string embedName = null) {

            Dictionary<string, object> parameters = ObjectToDictionary(options, "options", FlashBlacklist);
            if (!play) {
                parameters["play"] = false;
            }
            if (!loop) {
                parameters["loop"] = false;
            }
            if (!menu) {
                parameters["menu"] = false;
            }
            if (!String.IsNullOrEmpty(bgColor)) {
                parameters["bgColor"] = bgColor;
            }
            if (!String.IsNullOrEmpty(quality)) {
                parameters["quality"] = quality;
            }
            if (!String.IsNullOrEmpty(scale)) {
                parameters["scale"] = scale;
            }
            if (!String.IsNullOrEmpty(windowMode)) {
                parameters["wmode"] = windowMode;
            }
            if (!String.IsNullOrEmpty(baseUrl)) {
                parameters["base"] = baseUrl;
            }

            string cab = FlashCab;
            if (!String.IsNullOrEmpty(version)) {
                cab += "#version=" + version.Replace('.', ',');
            }
            return GetHtml(context, pathUtility, path, width, height,
                OleMimeType, null, FlashClassId, cab, "movie", FlashMimeType, parameters, htmlAttributes, embedName);
        }

        internal static HelperResult MediaPlayer(HttpContextBase context, VirtualPathUtilityBase pathUtility, string path, string width = null, string height = null,
            bool autoStart = true, int playCount = 1, string uiMode = null, bool stretchToFit = false,
            bool enableContextMenu = true, bool mute = false, int volume = -1, string baseUrl = null,
            object options = null, object htmlAttributes = null, string embedName = null) {

            Dictionary<string, object> parameters = ObjectToDictionary(options, "options", MediaPlayerBlacklist);
            if (!autoStart) {
                parameters["autoStart"] = false;
            }
            if (playCount != 1) {
                parameters["playCount"] = playCount;
            }
            if (!String.IsNullOrEmpty(uiMode)) {
                parameters["uiMode"] = uiMode;
            }
            if (stretchToFit) {
                parameters["stretchToFit"] = true;
            }
            if (!enableContextMenu) {
                parameters["enableContextMenu"] = false;
            }
            if (mute) {
                parameters["mute"] = true;
            }
            if (volume >= 0) {
                parameters["volume"] = Math.Min(volume, 100);
            }
            if (!string.IsNullOrEmpty(baseUrl)) {
                parameters["baseURL"] = baseUrl;
            }

            return GetHtml(context, pathUtility, path, width, height,
                null, null, MediaPlayerClassId, null, "URL", MediaPlayerMimeType, parameters, htmlAttributes, embedName);
        }

        internal static HelperResult Silverlight(HttpContextBase context, VirtualPathUtilityBase pathUtility, string path, string width, string height,
            string bgColor = null, string initParameters = null, string minimumVersion = null, bool autoUpgrade = true,
            object options = null, object htmlAttributes = null) {

            if (String.IsNullOrEmpty(width)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "width");
            }
            if (String.IsNullOrEmpty(height)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "height");
            }

            Dictionary<string, object> parameters = ObjectToDictionary(options, "options", SilverlightBlacklist);
            if (!String.IsNullOrEmpty(bgColor)) {
                parameters["background"] = bgColor;
            }
            if (!String.IsNullOrEmpty(initParameters)) {
                parameters["initparams"] = initParameters;
            }
            if (!String.IsNullOrEmpty(minimumVersion)) {
                parameters["minruntimeversion"] = minimumVersion;
            }
            if (!autoUpgrade) {
                parameters["autoUpgrade"] = autoUpgrade;
            }

            return GetHtml(context, pathUtility, path, width, height,
                SilverlightMimeType, "data:" + SilverlightMimeType + ",", // ',' required for Opera support
                null, null, "source", null, parameters, htmlAttributes, null,
                tw => {
                    tw.WriteLine("<a href=\"http://go.microsoft.com/fwlink/?LinkID=149156\" style=\"text-decoration:none\">");
                    tw.WriteLine("<img src=\"http://go.microsoft.com/fwlink?LinkId=108181\" alt=\"Get Microsoft Silverlight\" style=\"border-style:none\"/>");
                    tw.WriteLine("</a>");
                });
        }

        private static Dictionary<string, object> ObjectToDictionary(object o, string argName, string[] blackList) {
            
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(o);
            Dictionary<string, object> d = new Dictionary<string, object>(properties.Count + 10);
            
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(o)) {
                if (blackList.Contains(pd.Name, StringComparer.OrdinalIgnoreCase)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture,
                        HelpersToolkitResources.Video_PropertyCannotBeSet, pd.Name), argName);
                }
                d[pd.Name] = pd.GetValue(o);
            }
            return d;
        }

        private static HelperResult GetHtml(HttpContextBase context, VirtualPathUtilityBase pathUtility,
            string path, string width, string height, string objectType, string objectDataType,
            string objectClassId, string objectCodeBase, string pathParamName, string embedContentType,
            IDictionary<string, object> parameters = null, object htmlAttributes = null, string embedName = null,
            Action<TextWriter> plugin = null) {

            path = ValidatePath(context, pathUtility, path);

            Dictionary<string, object> objectAttr = ObjectToDictionary(htmlAttributes, "htmlAttributes", GlobalBlacklist);
            objectAttr["width"] = width;
            objectAttr["height"] = height;
            objectAttr["type"] = objectType;
            objectAttr["data"] = objectDataType;
            objectAttr["classid"] = objectClassId;
            objectAttr["codebase"] = objectCodeBase;

            return new HelperResult(tw => {
                tw.Write("<object ");
                foreach (var a in objectAttr.OrderBy(a => a.Key, StringComparer.OrdinalIgnoreCase)) {
                    var value = (a.Value == null) ? null : a.Value.ToString();
                    WriteIfNotNullOrEmpty(tw, a.Key, value);
                }
                tw.WriteLine(">");

                // object parameters
                if (!String.IsNullOrEmpty(pathParamName)) {
                    tw.WriteLine("<param name=\"{0}\" value=\"{1}\" />", 
                        HttpUtility.HtmlAttributeEncode(pathParamName), 
                        HttpUtility.HtmlAttributeEncode(HttpUtility.UrlPathEncode(path)));
                }
                if (parameters != null) {
                    foreach (var p in parameters) {
                        tw.WriteLine("<param name=\"{0}\" value=\"{1}\" />", 
                            HttpUtility.HtmlAttributeEncode(p.Key),
                            HttpUtility.HtmlAttributeEncode(p.Value.ToString()));
                    }
                }

                // embed tag and parameters: used by Netscape and IE on Mac
                if (!String.IsNullOrEmpty(embedContentType)) {
                    tw.Write("<embed src=\"{0}\" ", HttpUtility.HtmlAttributeEncode(HttpUtility.UrlPathEncode(path)));
                    WriteIfNotNullOrEmpty(tw, "width", width);
                    WriteIfNotNullOrEmpty(tw, "height", height);
                    WriteIfNotNullOrEmpty(tw, "name", embedName);
                    WriteIfNotNullOrEmpty(tw, "type", embedContentType);
                    if (parameters != null) {
                        foreach (var p in parameters) {
                            tw.Write("{0}=\"{1}\" ", HttpUtility.HtmlEncode(p.Key), HttpUtility.HtmlAttributeEncode(p.Value.ToString()));
                        }
                    }
                    tw.WriteLine("/>");
                }
                if (plugin != null) {
                    plugin(tw);
                }
                tw.WriteLine("</object>");
            });
        }

        private static string ValidatePath(HttpContextBase context, VirtualPathUtilityBase pathUtility, string path) {
            if (String.IsNullOrEmpty(path)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }
            string _path = path;
            if (!path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                // resolve relative paths
                path = pathUtility.Combine(context.Request.AppRelativeCurrentExecutionFilePath, path);
                // resolve to app absolute - SL doesn't support app relative
                path = pathUtility.ToAbsolute(path);
                if (!File.Exists(context.Server.MapPath(path))) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture,
                        HelpersToolkitResources.Video_FileDoesNotExist, _path));
                }
            }
            return path;
        }

        private static void WriteIfNotNullOrEmpty(TextWriter tw, string key, string value) {
            Debug.Assert(!String.IsNullOrEmpty(key));

            if (!String.IsNullOrEmpty(value)) {
                tw.Write("{0}=\"{1}\" ", HttpUtility.HtmlEncode(key), HttpUtility.HtmlAttributeEncode(value));
            }
        }
    }
}

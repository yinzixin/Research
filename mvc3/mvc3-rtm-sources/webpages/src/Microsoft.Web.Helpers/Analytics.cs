using System.Globalization;
using System.Web;
using System;

namespace Microsoft.Web.Helpers
{
    public static class Analytics {

        public static HtmlString GetGoogleHtml(string webPropertyId) {
            webPropertyId = HttpUtility.JavaScriptStringEncode(webPropertyId, addDoubleQuotes: true);
            return new HtmlString(
                String.Format(CultureInfo.InvariantCulture, "<script type=\"text/javascript\">\n" +
                    "var gaJsHost = ((\"https:\" == document.location.protocol) ? \"https://ssl.\" : \"http://www.\");\n" +
                    "document.write(unescape(\"%3Cscript src='\" + gaJsHost + \"google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E\"));\n" +
                    "</script>\n" +
                    "<script type=\"text/javascript\">\n" +
                        "try{{\n" +
                            "var pageTracker = _gat._getTracker({0});\n" +
                            "pageTracker._trackPageview();\n" +
                        "}} catch(err) {{}}\n" +
                    "</script>\n", webPropertyId)
            );
        }
        
        public static HtmlString GetGoogleAsyncHtml(string webPropertyId)
        {
            webPropertyId = HttpUtility.JavaScriptStringEncode(webPropertyId, addDoubleQuotes: false);
            return new HtmlString(
                String.Format(CultureInfo.InvariantCulture, "<script type=\"text/javascript\">\n" + 
                    "var _gaq = _gaq || [];\n" +
                    "_gaq.push(['_setAccount', '{0}']);\n" + 
                    "_gaq.push(['_trackPageview']);\n" +
                    "(function() {{\n" +
                    "var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;\n" +
                    "ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';\n" +
                    "var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);\n" +
                    "}})();\n" +
                    "</script>\n", webPropertyId)
            );
        }

        public static HtmlString GetYahooHtml(string account) {
            account = HttpUtility.JavaScriptStringEncode(account, addDoubleQuotes: true);
            return new HtmlString(
                String.Format(CultureInfo.InvariantCulture, "<SCRIPT language=\"JavaScript\" type=\"text/javascript\">\n" +
                    "<!-- Yahoo! Inc.\n" +
                    "window.ysm_customData = new Object();\n" +
                    "window.ysm_customData.conversion = \"transId=,currency=,amount=\";\n" +
                    "var ysm_accountid = {0};\n" +
                    "document.write(\"<SCR\" + \"IPT language='JavaScript' type='text/javascript' \"\n" +
                    "+ \"SRC=//\" + \"srv3.wa.marketingsolutions.yahoo.com\" + \"/script/ScriptServlet\" + \"?aid=\" + ysm_accountid\n" +
                    "+ \"></SCR\" + \"IPT>\");\n" +
                    "// -->\n" +
                "</SCRIPT>\n", account)
            );
        }

        public static HtmlString GetStatCounterHtml(int project, string security) {
            string url = String.Format(CultureInfo.InvariantCulture, 
                "https://c.statcounter.com/{0}/0/{1}/1/", project, HttpUtility.UrlPathEncode(security));
            url = HttpUtility.HtmlAttributeEncode(url);
            security = HttpUtility.JavaScriptStringEncode(security, addDoubleQuotes: true);

            return new HtmlString(
                String.Format(CultureInfo.InvariantCulture, "<!-- Start of StatCounter Code -->\n" +
                    "<script type=\"text/javascript\">\n" + 
                    "var sc_project={0};\n" +
                    "var sc_invisible=1;\n" + 
                    "var sc_security={1};\n" +
                    "var sc_text=2;\n" +
                    "var sc_https=1;\n" +  
                    "var scJsHost = ((\"https:\" == document.location.protocol) ? \"https://secure.\" : \"http://www.\");\n" +
                    "document.write(\"<sc\" + \"ript type='text/javascript' src='\" + " +
                    "scJsHost + \"statcounter.com/counter/counter_xhtml.js'></\" + \"script>\");\n" + 
                    "</script>\n\n" + 
                    "<noscript>" + 
                    "<div class=\"statcounter\">" +
                    "<a title=\"tumblrstatistics\" class=\"statcounter\" href=\"http://www.statcounter.com/tumblr/\">" +
                    "<img class=\"statcounter\" src=\"{2}\" alt=\"tumblr statistics\"/>" + 
                    "</a>" +
                    "</div>" +
                    "</noscript>\n" +
                    "<!-- End of StatCounter Code -->", project, security, url)
             );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Helpers.Resources;

namespace Microsoft.Web.Helpers
{
    public static class Bing
    {
        internal static readonly object _siteTitle = new object();
        internal static readonly object _siteUrl = new object();
        private const string DefaultBoxWith = "322px";
        private const string BingSearchScriptHeader = "<script type=\"text/javascript\" charset=\"utf-8\">\n" +
            "var WLSearchBoxConfiguration=\n{{\n\"global\":{{\n\"serverDNS\":\"www.bing.com\",\n\"market\":\"{0}\"\n" +
            "}},\n\"appearance\":{{\"autoHideTopControl\":false,\n\"width\":{1},\n\"height\":{2},\n\"theme\":\"{3}\"\n}},\n\"scopes\":[\n";
        private const string BingSiteSearchScript = "{{\n\"type\":\"web\",\n\"caption\":\"{0}\",\n\"searchParam\":\"{1}\"\n}},\n";
        private const string BingSearchScriptFooter = "{{\n\"type\":\"web\",\n\"caption\":\"Web\",\n\"searchParam\":\"\"" +
                "}}\n]\n}}\n</script>\n<script type=\"text/javascript\" charset=\"utf-8\" " +
                "src=\"http://www.bing.com/bootstrap.js?market={0}&amp;ServId=SearchBox&amp;ServId=SearchBoxWeb&amp;Callback=WLSearchBoxScriptReady\"></script>";
        //Create list of locales currently supported by Bing Search - this may change.  
        //Omitting cultures for which text is "Loading..." as these will behave correctly by default
        private static Dictionary<string, string> supportedLocales = new Dictionary<string, string>() {
            {"fr-CA", "Chargement..."},
            {"zh-CN", "正在加载..."},
            {"fr-FR", "Chargement..."},
            {"de-DE", "Wird geladen..."},
            {"it-IT", "Caricamento in corso..."},
            {"ja-JP", "読み込み中..."},
            {"ko-KR", "잠시만 기다려 주세요."},
            {"nl-NL", "Laden..."},
            {"es-ES", "Cargando..."},
            {"es-US", "Cargando..."}
        };

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The use of SiteTitle in the exception is a better user experience")]
        public static string SiteTitle {
            get {
                return ScopeStorage.CurrentScope[_siteTitle] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("SiteTitle");
                }
                ScopeStorage.CurrentScope[_siteTitle] = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Strings are easier to work with for Plan9 scenario")]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The use of SiteUrl in the exception is a better user experience")]
        public static string SiteUrl {
            get {
                return ScopeStorage.CurrentScope[_siteUrl] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("SiteUrl");
                }
                ScopeStorage.CurrentScope[_siteUrl] = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Strings are easier to work with for Plan9 scenario")]
        public static IHtmlString SearchBox(string boxWidth = DefaultBoxWith) {
            if (HttpContext.Current == null) {
                return null;
            }
            return SearchBox(SiteUrl, SiteTitle, boxWidth, new HttpContextWrapper(HttpContext.Current));
        }

        internal static IHtmlString SearchBox(string siteUrl, string siteTitle, string boxWidth, HttpContextBase httpContext) {
            TagBuilder codePage = new TagBuilder("input");
            codePage.MergeAttribute("name", "cp");
            codePage.MergeAttribute("type", "hidden");
            codePage.MergeAttribute("value", GetCodePageFromRequest(httpContext).ToString(CultureInfo.InvariantCulture));
            
            TagBuilder formHiddenField = new TagBuilder("input");
            formHiddenField.MergeAttribute("name", "FORM");
            formHiddenField.MergeAttribute("type", "hidden");
            formHiddenField.MergeAttribute("value", "FREESS");

            TagBuilder formTag = new TagBuilder("form");
            formTag.MergeAttribute("method", "get");
            formTag.MergeAttribute("action", "http://www.bing.com/search");
            formTag.MergeAttribute("class", "BingSearch");
            formTag.MergeAttribute("target", "_blank");

            formTag.InnerHtml = formHiddenField.ToString(TagRenderMode.SelfClosing) + codePage.ToString(TagRenderMode.SelfClosing) + ConstructBasicSearchTable(siteUrl, siteTitle, boxWidth);

            return new HtmlString(formTag.ToString());
        }

        private static string ConstructBasicSearchTable(string siteUrl, string siteTitle, string boxWidth){
            TagBuilder bingSearchTable = new TagBuilder("table");
            bingSearchTable.MergeAttribute("cellpadding", "0");
            bingSearchTable.MergeAttribute("cellspacing", "0");
            bingSearchTable.MergeAttribute("style", String.Format(CultureInfo.CurrentCulture, "width:{0};", boxWidth));

            //Create Table Cell with Search Box Input
            TagBuilder bingSearchBoxPlaceHolderCell = new TagBuilder("td");
            bingSearchBoxPlaceHolderCell.MergeAttribute("style", "width: 100%; border:solid 1px #ccc; border-right-style:none; " +
                "padding-left:10px; padding-right:10px; vertical-align:middle;");
            TagBuilder bingSearchBoxInput = new TagBuilder("input");
            bingSearchBoxInput.MergeAttribute("type", "text");
            bingSearchBoxInput.MergeAttribute("name", "q");
            bingSearchBoxInput.MergeAttribute("title", HelpersToolkitResources.BingSearch_Title);
            bingSearchBoxInput.MergeAttribute("style", "background-image:url(http://www.bing.com/siteowner/s/siteowner/searchbox_background_k.png); " +
                "background-position:right; background-repeat:no-repeat; font-family:Arial; font-size:14px; color:#000; " +
                "width:100%; border:none 0 transparent;");
            //Merge bingSearchBoxPlaceHolderCell and bingSearchBoxInput
            bingSearchBoxPlaceHolderCell.InnerHtml = bingSearchBoxInput.ToString(TagRenderMode.SelfClosing);

            //Create Table Cell with Search Box Button
            TagBuilder bingSearchButtonCell = new TagBuilder("td");
            bingSearchButtonCell.MergeAttribute("style", "border:solid 1px #ccc; border-left-style:none; padding-left:0px; padding-right:3px;");
            TagBuilder bingSearchBoxButton = new TagBuilder("input");
            bingSearchBoxButton.MergeAttribute("type", "image");
            bingSearchBoxButton.MergeAttribute("alt", HelpersToolkitResources.BingSearch_SearchButtonText);
            bingSearchBoxButton.MergeAttribute("src", "http://www.bing.com/siteowner/s/siteowner/searchbutton_normal_k.gif");
            bingSearchBoxButton.MergeAttribute("style", "border:none 0 transparent; height:24px; width:24px; vertical-align:top;");

            //Merge bingSearchButtonCell and bingSearchBoxButton
            bingSearchButtonCell.InnerHtml = bingSearchBoxButton.ToString(TagRenderMode.SelfClosing);

            //Perform merging of the bingSearchBoxPlaceHolderRow with the two cells
            TagBuilder bingSearchBoxPlaceHolderRow = new TagBuilder("tr");
            bingSearchBoxPlaceHolderRow.MergeAttribute("style", "height: 32px");
            bingSearchBoxPlaceHolderRow.InnerHtml = bingSearchBoxPlaceHolderCell.ToString() + bingSearchButtonCell.ToString();
            //Merge Table
            bingSearchTable.InnerHtml = bingSearchBoxPlaceHolderRow.ToString();
            
            if(!String.IsNullOrEmpty(siteUrl)) {
                TagBuilder localOnlyLabel = new TagBuilder("label");
                TagBuilder localOnlyCheckbox = new TagBuilder("input");
                localOnlyCheckbox.MergeAttribute("type", "radio");
                localOnlyCheckbox.MergeAttribute("name", "q1");
                localOnlyCheckbox.MergeAttribute("value", "site:" + siteUrl);
                localOnlyCheckbox.MergeAttribute("checked", "checked");
                if (!String.IsNullOrEmpty(siteTitle)) {
                    localOnlyLabel.InnerHtml = localOnlyCheckbox.ToString(TagRenderMode.SelfClosing) + siteTitle;
                } else {
                    localOnlyLabel.InnerHtml = localOnlyCheckbox.ToString(TagRenderMode.SelfClosing) + HelpersToolkitResources.BingSearch_DefaultSiteSearchText;
                }

                TagBuilder searchWebLabel = new TagBuilder("label");
                TagBuilder searchWebCheckbox = new TagBuilder("input");
                searchWebCheckbox.MergeAttribute("type", "radio");
                searchWebCheckbox.MergeAttribute("name", "q1");
                searchWebCheckbox.MergeAttribute("value", String.Empty);
                searchWebLabel.InnerHtml = searchWebCheckbox.ToString(TagRenderMode.SelfClosing) + HelpersToolkitResources.BingSearch_DefaultWebSearchText;

                TagBuilder siteSearchOptionsCell = new TagBuilder("td") { InnerHtml = localOnlyLabel.ToString() + "&nbsp;" + searchWebLabel.ToString() };
                siteSearchOptionsCell.MergeAttribute("colspan", "2");
                siteSearchOptionsCell.MergeAttribute("style", "font-size: small");

                bingSearchTable.InnerHtml += new TagBuilder("tr") { InnerHtml = siteSearchOptionsCell.ToString() }.ToString();
            }

            return bingSearchTable.ToString();
        }

        internal static int GetCodePageFromRequest(HttpContextBase context) {
            Debug.Assert(context != null);
            return context.Response.ContentEncoding.CodePage;
        }

        /// <summary>
        /// Used to display an Advanced Bing Search Box
        /// </summary>
        /// <remarks>
        /// This allows you to search both your own site(s) and the web.  If the user does not
        /// specify a url, we assume that they only want to search the web and output the
        /// markup to do so.  Note that bing supports searching more than one site, but 
        /// for simplicity, we will only allow one site to be searched
        /// </remarks>
        /// <see cref="http://www.bing.com/siteowner/searchboxstep1.aspx"/>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Strings are easier to work with for Plan9 scenario")]
        public static IHtmlString AdvancedSearchBox(string boxWidth = DefaultBoxWith,
            int resultWidth = 600, int resultHeight = 400, string themeColor = "Blue", string locale = "en-US") {
            //Check HttpContext
            if (HttpContext.Current == null)
            {
                return null;
            }
            return AdvancedSearchBox(SiteUrl, SiteTitle, boxWidth, resultWidth, resultHeight, themeColor, locale);
        }

        internal static IHtmlString AdvancedSearchBox(string siteUrl, string siteTitle, string boxWidth, int resultWidth, int resultHeight, 
            string themeColor, string locale) {
            //Parameter checking
            if (resultWidth < 0) {
                throw new ArgumentOutOfRangeException("resultWidth", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (resultHeight < 0) {
                throw new ArgumentOutOfRangeException("resultHeight", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }

            //Construct Meta Tag
            TagBuilder metaTag = new TagBuilder("meta");
            metaTag.MergeAttribute("name", "Search.WLSearchBox");
            metaTag.MergeAttribute("content", "1.1, en-US");

            //Construct Search Box Div
            TagBuilder WLSearchBoxDiv = new TagBuilder("div");
            WLSearchBoxDiv.MergeAttribute("id", "WLSearchBoxDiv");

            //Construct Table and Script Block
            string scriptBlock = ConstructScriptBlock(siteUrl, siteTitle, resultWidth, resultHeight, themeColor, locale);
            //Determine loading text, defaulting to Loading... if not found in our collection of supported locales
            string determinedLocaleText = supportedLocales.ContainsKey(locale) ? supportedLocales[locale] : "Loading...";
            WLSearchBoxDiv.InnerHtml = ConstructAdvancedSearchTable(boxWidth, determinedLocaleText) + scriptBlock;

            //Construct Final Markup
            return (new HtmlString("<!-- Bing -->\n" + metaTag.ToString(TagRenderMode.SelfClosing) + WLSearchBoxDiv.ToString() + "\n<!-- Bing -->"));
        }
        private static string ConstructAdvancedSearchTable(string boxWidth, string loadingText) {
            //Construct Search Box Table with inputs, using input width from user
            TagBuilder bingSearchTable = new TagBuilder("table");
            bingSearchTable.MergeAttribute("cellpadding", "0");
            bingSearchTable.MergeAttribute("cellspacing", "0");
            bingSearchTable.MergeAttribute("style", String.Format(CultureInfo.CurrentCulture, "width:{0}; height:32px;", boxWidth));
            TagBuilder bingSearchBoxPlaceHolderRow = new TagBuilder("tr");
            bingSearchBoxPlaceHolderRow.MergeAttribute("id", "WLSearchBoxPlaceholder");

            //Create Table Cell with Search Box Input
            TagBuilder bingSearchBoxPlaceHolderCell = new TagBuilder("td");
            bingSearchBoxPlaceHolderCell.MergeAttribute("style", "width:100%; border:solid 1px #ccc; border-right-style:none; " +
                "padding-left:10px; padding-right:10px; vertical-align:middle;");
            TagBuilder bingSearchBoxInput = new TagBuilder("input");
            bingSearchBoxInput.MergeAttribute("id", "WLSearchBoxInput");
            bingSearchBoxInput.MergeAttribute("type", "text");
            bingSearchBoxInput.MergeAttribute("value", loadingText);
            bingSearchBoxInput.MergeAttribute("disabled", "disabled");
            bingSearchBoxInput.MergeAttribute("style", "background-image:url(http://www.bing.com/siteowner/s/siteowner/searchbox_background_k.png); " +
                "background-position:right; background-repeat:no-repeat; font-family:Arial; font-size:14px; color:#000; " +
                "width:100%; border:none 0 transparent;");
            //Merge bingSearchBoxPlaceHolderCell and bingSearchBoxInput
            bingSearchBoxPlaceHolderCell.InnerHtml = bingSearchBoxInput.ToString(TagRenderMode.SelfClosing);

            //Create Table Cell with Search Box Button
            TagBuilder bingSearchButtonCell = new TagBuilder("td");
            bingSearchButtonCell.MergeAttribute("style", "border:solid 1px #ccc; border-left-style:none; padding-left:0px; padding-right:3px;");
            TagBuilder bingSearchBoxButton = new TagBuilder("input");
            bingSearchBoxButton.MergeAttribute("id", "WLSearchBoxButton");
            bingSearchBoxButton.MergeAttribute("type", "image");
            bingSearchBoxButton.MergeAttribute("src", "http://www.bing.com/siteowner/s/siteowner/searchbutton_normal_k.gif");
            bingSearchBoxButton.MergeAttribute("style", "border:none 0 transparent; height:24px; width:24px; vertical-align:top;");

            //Merge bingSearchButtonCell and bingSearchBoxButton
            bingSearchButtonCell.InnerHtml = bingSearchBoxButton.ToString(TagRenderMode.SelfClosing);
            //Perform merging of the bingSearchBoxPlaceHolderRow with the two cells
            bingSearchBoxPlaceHolderRow.InnerHtml = bingSearchBoxPlaceHolderCell.ToString() + bingSearchButtonCell.ToString();
            //Merge Table
            bingSearchTable.InnerHtml = bingSearchBoxPlaceHolderRow.ToString();

            //Return final markup for the table
            return bingSearchTable.ToString();
        }

        private static string ConstructScriptBlock(string siteUrl, string siteTitle, int resultWidth, int resultHeight, string themeColor, string locale) {
            StringBuilder finalScript = new StringBuilder();
            finalScript.AppendFormat(CultureInfo.CurrentCulture, BingSearchScriptHeader, locale, resultWidth, resultHeight, 
                HttpUtility.JavaScriptStringEncode(themeColor));
            //If the user supplies a site url and name, we will attempt to add in the site search script
            if (!string.IsNullOrEmpty(siteUrl)) {
                //If the user does not supply a site name, we specify a default
                string encodedSiteUrl = HttpUtility.UrlPathEncode(siteUrl);
                if (string.IsNullOrEmpty(siteTitle)) {
                    finalScript.AppendFormat(CultureInfo.CurrentCulture, BingSiteSearchScript, 
                        HttpUtility.JavaScriptStringEncode(HelpersToolkitResources.BingSearch_DefaultSiteSearchText), 
                        HttpUtility.JavaScriptStringEncode(encodedSiteUrl));
                } else {
                    finalScript.AppendFormat(CultureInfo.CurrentCulture, BingSiteSearchScript, 
                        HttpUtility.JavaScriptStringEncode(siteTitle), 
                        HttpUtility.JavaScriptStringEncode(encodedSiteUrl));
                }
            }
            finalScript.AppendFormat(CultureInfo.CurrentCulture, BingSearchScriptFooter, locale);
            return finalScript.ToString();
        }
    }
}

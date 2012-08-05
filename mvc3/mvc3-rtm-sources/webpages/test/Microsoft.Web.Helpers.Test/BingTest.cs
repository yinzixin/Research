using System;
using System.Text;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.Helpers.Resources;
using System.Web.WebPages.TestUtils;
using Moq;
using System.Globalization;
using System.Web.Helpers.Test;
using System.Web.WebPages.Scope;

namespace Microsoft.Web.Helpers.Test {
    [TestClass]
    public class BingTest
    {
        private const string AdvancedBingSearchHtmlHeader = "<!-- Bing -->\n<meta content=\"1.1, en-US\" name=\"Search.WLSearchBox\" />" +
            "<div id=\"WLSearchBoxDiv\">";
        private const string AdvancedBingSearchHtmlTable = "<table cellpadding=\"0\" cellspacing=\"0\" style=\"width:{0}; height:32px;\">" +
            "<tr id=\"WLSearchBoxPlaceholder\"><td style=\"width:100%; border:solid 1px #ccc; border-right-style:none; " +
            "padding-left:10px; padding-right:10px; vertical-align:middle;\"><input disabled=\"disabled\" id=\"WLSearchBoxInput\" " +
            "style=\"background-image:url(http://www.bing.com/siteowner/s/siteowner/searchbox_background_k.png); " +
            "background-position:right; background-repeat:no-repeat; font-family:Arial; font-size:14px; color:#000; " +
            "width:100%; border:none 0 transparent;\" type=\"text\" value=\"{1}\" /></td>" +
            "<td style=\"border:solid 1px #ccc; border-left-style:none; padding-left:0px; padding-right:3px;\">" +
            "<input id=\"WLSearchBoxButton\" src=\"http://www.bing.com/siteowner/s/siteowner/searchbutton_normal_k.gif\" " +
            "style=\"border:none 0 transparent; height:24px; width:24px; vertical-align:top;\" type=\"image\" /></td></tr></table>";
        private const string AdvancedBingSearchScriptHeader = "<script type=\"text/javascript\" charset=\"utf-8\">\n" +
            "var WLSearchBoxConfiguration=\n{{\n\"global\":{{\n\"serverDNS\":\"www.bing.com\",\n\"market\":\"{0}\"\n" +
            "}},\n\"appearance\":{{\"autoHideTopControl\":false,\n\"width\":{1},\n\"height\":{2},\n\"theme\":\"{3}\"\n}},\n\"scopes\":[\n";
        private const string AdvancedBingSiteSearchScript = "{{\n\"type\":\"web\",\n\"caption\":\"{0}\",\n\"searchParam\":\"{1}\"\n}},\n";
        private const string AdvancedBingSearchScriptFooter = "{{\n\"type\":\"web\",\n\"caption\":\"Web\",\n\"searchParam\":\"\"" +
            "}}\n]\n}}\n</script>\n<script type=\"text/javascript\" charset=\"utf-8\" " +
            "src=\"http://www.bing.com/bootstrap.js?market={0}&amp;ServId=SearchBox&amp;ServId=SearchBoxWeb&amp;Callback=WLSearchBoxScriptReady\"></script>";
        private const string AdvancedBingSearchFooter = "</div>\n<!-- Bing -->";

        private const string BasicBingSearchTemplate = @"<form action=""http://www.bing.com/search"" class=""BingSearch"" method=""get"" target=""_blank"">"
                + @"<input name=""FORM"" type=""hidden"" value=""FREESS"" /><input name=""cp"" type=""hidden"" value=""{0}"" />"
                + @"<table cellpadding=""0"" cellspacing=""0"" style=""width:{1}px;""><tr style=""height: 32px"">"
                + @"<td style=""width: 100%; border:solid 1px #ccc; border-right-style:none; padding-left:10px; padding-right:10px; vertical-align:middle;"">"
                + @"<input name=""q"" style=""background-image:url(http://www.bing.com/siteowner/s/siteowner/searchbox_background_k.png); background-position:right; background-repeat:no-repeat; font-family:Arial; font-size:14px; color:#000; width:100%; border:none 0 transparent;"" title=""Search Bing"" type=""text"" />"
                + @"</td><td style=""border:solid 1px #ccc; border-left-style:none; padding-left:0px; padding-right:3px;"">"
                + @"<input alt=""Search"" src=""http://www.bing.com/siteowner/s/siteowner/searchbutton_normal_k.gif"" style=""border:none 0 transparent; height:24px; width:24px; vertical-align:top;"" type=""image"" />"
                + @"</td></tr>";

        private const string BasicBingSearchFooter = "</table></form>";

        private const string BasicBingSearchLocalSiteSearch = @"<tr><td colspan=""2"" style=""font-size: small""><label><input checked=""checked"" name=""q1"" type=""radio"" value=""site:{0}"" />{1}</label>&nbsp;<label><input name=""q1"" type=""radio"" value="""" />Search Web</label></td></tr>";

        [TestMethod]
        public void SiteTitleThrowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => Bing.SiteTitle = null, "SiteTitle");
        }

        [TestMethod]
        public void SiteTitleUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            Bing.SiteTitle = value;

            // Assert
            Assert.AreEqual(Bing.SiteTitle, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[Bing._siteTitle], value);
        }

        [TestMethod]
        public void SiteUrlThrowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => Bing.SiteUrl = null, "SiteUrl");
        }

        [TestMethod]
        public void SiteUrlUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            Bing.SiteUrl = value;

            // Assert
            Assert.AreEqual(Bing.SiteUrl, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[Bing._siteUrl], value);
        }

        [TestMethod]
        public void AdvancedSearchThrowsExceptionForInvalidResultWidth()
        {
            // Act & Assert 
            ExceptionAssert.ThrowsArgGreaterThanOrEqualTo(() =>
            {
                Bing.AdvancedSearchBox("www.test.com", "Test", "322px", -1, 400, "Blue", "en-US");
            }, "resultWidth", "0");
        }

        [TestMethod]
        public void AdvancedSearchThrowsExceptionForInvalidResultHeight()
        {
            // Act & Assert 
            ExceptionAssert.ThrowsArgGreaterThanOrEqualTo(() =>
            {
                Bing.AdvancedSearchBox("www.test.com", "Test", "322px", 600, -1, "Blue", "en-US");
            }, "resultHeight", "0");
        }

        [TestMethod]
        public void AdvancedSearchReturnsCorrectMarkupWithSiteSearchBox() {
            // Arrange
            string expectedHtml = AdvancedBingSearchHtmlHeader +
                string.Format(AdvancedBingSearchHtmlTable, "322px", "Loading...") +
                string.Format(AdvancedBingSearchScriptHeader, "en-US", 600, 400, "Blue") +
                string.Format(AdvancedBingSiteSearchScript, "Test", "www.test.com") +
                string.Format(AdvancedBingSearchScriptFooter, "en-US") + AdvancedBingSearchFooter;

            // Act
            string actualHtml = Bing.AdvancedSearchBox("www.test.com", "Test", "322px", 600, 400, "Blue", "en-US").ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void AdvancedSearchSetAllParametersToNonDefaults() {
            // Arrange
            string expectedHtml = AdvancedBingSearchHtmlHeader +
                string.Format(AdvancedBingSearchHtmlTable, "500px", "잠시만 기다려 주세요.") +
                string.Format(AdvancedBingSearchScriptHeader, "ko-KR", 800, 600, "Red") +
                string.Format(AdvancedBingSiteSearchScript, "Test", "www.test.com") +
                string.Format(AdvancedBingSearchScriptFooter, "ko-KR") + AdvancedBingSearchFooter;

            // Act
            string actualHtml = Bing.AdvancedSearchBox("www.test.com", "Test", "500px", 800, 600, "Red", "ko-KR").ToString();
            
            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void AdvancedSearchSetSiteUrlButNotNameFallsBackToDefaultSiteTitle() {
            // Arrange
            string expectedHtml = AdvancedBingSearchHtmlHeader +
                string.Format(AdvancedBingSearchHtmlTable, "322px", "Loading...") +
                string.Format(AdvancedBingSearchScriptHeader, "en-US", 600, 400, "Blue") +
                string.Format(AdvancedBingSiteSearchScript, HelpersToolkitResources.BingSearch_DefaultSiteSearchText, "www.test.com") +
                string.Format(AdvancedBingSearchScriptFooter, "en-US") + AdvancedBingSearchFooter;

            // Act
            string actualHtml = Bing.AdvancedSearchBox("www.test.com", null, "322px", 600, 400, "Blue", "en-US").ToString();
            
            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void AdvancedSearchBadLocaleFallsBackToLoadingText() {
            // Arrange
            var encoding = Encoding.UTF8;
            string expectedHtml = AdvancedBingSearchHtmlHeader +
                string.Format(AdvancedBingSearchHtmlTable, "500px", "Loading...") +
                string.Format(AdvancedBingSearchScriptHeader, "baz", 800, 600, "Red") +
                string.Format(AdvancedBingSiteSearchScript, "Test", "www.test.com") +
                string.Format(AdvancedBingSearchScriptFooter, "baz") + AdvancedBingSearchFooter;

            // Act
            string actualHtml = Bing.AdvancedSearchBox("www.test.com", "Test", "500px", 800, 600, "Red", "baz").ToString();
            
            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void AdvancedSearchBoxEncodesScriptContent() {
            // Arrange
            var encoding = Encoding.Unicode;
            string expectedHtml = AdvancedBingSearchHtmlHeader +
                string.Format(AdvancedBingSearchHtmlTable, "500px", "Loading...") +
                string.Format(AdvancedBingSearchScriptHeader, "baz", 800, 600, "Red") +
                string.Format(AdvancedBingSiteSearchScript, "\\u0027Test\\u0027", "\\u0027www.test.com\\u0027") +
                string.Format(AdvancedBingSearchScriptFooter, "baz") + AdvancedBingSearchFooter;

            // Act
            string actualHtml = Bing.AdvancedSearchBox("'www.test.com'", "'Test'", "500px", 800, 600, "Red", "baz").ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxDoesNotContainSearchLocalWhenSiteUrlIsNull() {
            // Arrange
            var encoding = Encoding.UTF8;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;
            
            // Act
            var actualHtml = Bing.SearchBox(null, null, "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxDoesNotContainSearchLocalWhenSiteUrlIsEmpty() {
            // Arrange
            var encoding = Encoding.UTF8;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;
            
            // Act
            var actualHtml = Bing.SearchBox(String.Empty, String.Empty, "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }


        [TestMethod]
        public void SearchBoxUsesResponseEncodingToDetermineCodePage() {
            // Arrange
            var encoding = Encoding.GetEncoding(51932); //euc-jp
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;
            
            // Act
            var actualHtml = Bing.SearchBox(String.Empty, String.Empty, "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxUsesWidthToSetBingSearchTableSize() {
            // Arrange
            var encoding = Encoding.UTF8; 
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 609) + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing.SearchBox(String.Empty, String.Empty, "609px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxUsesWithSiteUrlProducesLocalSearchOptions() {
            // Arrange
            var encoding = Encoding.Default;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +  
                    String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "Search Site") + BasicBingSearchFooter;
            
            // Act
            var actualHtml = Bing.SearchBox("www.asp.net", String.Empty, "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxUsesWithSiteUrlAndSiteTitleProducesLocalSearchOptions()
        {
            // Arrange
            var encoding = Encoding.Default;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                    String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "Custom Search") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing.SearchBox("www.asp.net", "Custom Search", "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        [TestMethod]
        public void SearchBoxWithLocalSiteOptionUsesResponseEncoding() {
            // Arrange
            var encoding = Encoding.GetEncoding(1258);  //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                    String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "Search Site") + BasicBingSearchFooter;
            
            // Act
            var actualHtml = Bing.SearchBox("www.asp.net", String.Empty, "322px", GetContextForSearchBox(encoding)).ToString();

            // Assert
            Assert.AreEqual(expectedHtml, actualHtml);
        }

        private HttpContextBase GetContextForSearchBox(Encoding contentEncoding = null) {
            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> response = new Mock<HttpResponseBase>();
            response.Setup(c => c.ContentEncoding).Returns(contentEncoding ?? Encoding.Default);
            context.Setup(c => c.Response).Returns(response.Object);

            return context.Object;
        }

    }
}
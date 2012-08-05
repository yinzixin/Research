using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Web.Helpers.Test {
    [TestClass]
    public class LinkShareTest {
        private static LinkShareSite[] _allLinkShareSites = new[] { LinkShareSite.Delicious, LinkShareSite.Digg, LinkShareSite.GoogleBuzz, 
                LinkShareSite.Facebook, LinkShareSite.Reddit, LinkShareSite.StumbleUpon, LinkShareSite.Twitter};

        [TestMethod]
        public void RenderWithFacebookFirst_ReturnsHtmlWithFacebookAndThenOthersTest() {
            string pageTitle = "page1";
            string pageLinkBack = "page link back";
            string twitterUserName = String.Empty;
            string twitterTag = String.Empty;
            string actual;
            actual = LinkShare.GetHtml(pageTitle, pageLinkBack, twitterUserName, twitterTag, LinkShareSite.Facebook, LinkShareSite.All).ToString();
            Assert.IsTrue(actual.Contains("twitter.com"));
            int pos = actual.IndexOf("facebook.com");
            Assert.IsTrue(pos > 0);
            int pos2 = actual.IndexOf("reddit.com");
            Assert.IsTrue(pos2 > pos);
            pos2 = actual.IndexOf("digg.com");
            Assert.IsTrue(pos2 > pos);
        }

        [TestMethod]
        public void BitlyApiKeyThrowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => LinkShare.BitlyApiKey = null, "value");
        }

        [TestMethod]
        public void BitlyApiKeyUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            LinkShare.BitlyApiKey = value;

            // Assert
            Assert.AreEqual(LinkShare.BitlyApiKey, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[LinkShare._bitlyApiKey], value);
        }

        [TestMethod]
        public void BitlyLoginThrowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => LinkShare.BitlyLogin = null, "value");
        }

        [TestMethod]
        public void BitlyLoginUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            LinkShare.BitlyLogin = value;

            // Assert
            Assert.AreEqual(LinkShare.BitlyLogin, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[LinkShare._bitlyLogin], value);
        }

        [TestMethod]
        public void RenderWithNullPageTitle_ThrowsException() {
            ExceptionAssert.ThrowsArgNullOrEmpty(
                () => LinkShare.GetHtml(null),
                "pageTitle");
        }

        [TestMethod]
        public void Render_WithFacebook_Works() {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: LinkShareSite.Facebook).ToString();
            string expectedHTML =
                "<a href=\"http://www.facebook.com/sharer.php?u=www.foo.com&amp;t=page-title\" target=\"_blank\" title=\"Share on Facebook\"><img alt=\"Share on Facebook\" src=\"http://www.facebook.com/favicon.ico\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Share on Facebook\" /></a>";
            Assert.AreEqual(actualHTML, expectedHTML);
        }

        [TestMethod]
        public void Render_WithFacebookAndDigg_Works() {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: new LinkShareSite[] { LinkShareSite.Facebook, LinkShareSite.Digg }).ToString();
            string expectedHTML =
                "<a href=\"http://www.facebook.com/sharer.php?u=www.foo.com&amp;t=page-title\" target=\"_blank\" title=\"Share on Facebook\"><img alt=\"Share on Facebook\" src=\"http://www.facebook.com/favicon.ico\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Share on Facebook\" /></a><a href=\"http://digg.com/submit?url=www.foo.com&amp;title=page-title\" target=\"_blank\" title=\"Digg!\"><img alt=\"Digg!\" src=\"http://digg.com/img/badges/16x16-digg-guy.gif\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Digg!\" /></a>";
            Assert.AreEqual(actualHTML, expectedHTML);
        }


        [TestMethod]
        public void Render_WithFacebook_RendersAnchorTitle() {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: LinkShareSite.Facebook).ToString();

            Regex regex = new Regex("^<a.*title=\"(?'title'[^\"]+)\".*><img.*$");
            Match match = regex.Match(actualHTML);

            Assert.IsTrue(match.Groups.Count == 2);
            Assert.IsNotNull(match.Groups["title"]);
            Assert.AreEqual(match.Groups["title"].Value, "Share on Facebook");
        }

        [TestMethod]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenArgumentIsNull() {
            // Act and Assert
            var result = LinkShare.GetSitesInOrder(linkSites: null);

            CollectionAssert.AreEqual(result.ToList(), _allLinkShareSites);
        }

        [TestMethod]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenArgumentIEmpty() {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new LinkShareSite[] { });

            // Assert
            CollectionAssert.AreEqual(result.ToList(), _allLinkShareSites);
        }

        [TestMethod]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenAllIsFirstItem() {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new LinkShareSite[] { LinkShareSite.All, LinkShareSite.Reddit });

            // Assert
            CollectionAssert.AreEqual(result.ToList(), _allLinkShareSites);
        }

        [TestMethod]
        public void LinkShare_GetSitesInOrderReturnsSitesInOrderWhenAllIsNotFirstItem() {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new LinkShareSite[] { LinkShareSite.Reddit, LinkShareSite.Facebook, LinkShareSite.All });

            // Assert
            CollectionAssert.AreEqual(result.ToList(), new [] { LinkShareSite.Reddit, LinkShareSite.Facebook, LinkShareSite.Delicious, LinkShareSite.Digg, 
                LinkShareSite.GoogleBuzz, LinkShareSite.StumbleUpon, LinkShareSite.Twitter });
        }

    }
}
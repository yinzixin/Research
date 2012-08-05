using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;
using System.Collections.Generic;

namespace Microsoft.Web.Helpers {
    public static class LinkShare {
        internal static readonly object _bitlyApiKey = new object();
        internal static readonly object _bitlyLogin = new object();
        private static readonly Lazy<IEnumerable<LinkShareSite>> _allSites = new Lazy<IEnumerable<LinkShareSite>>(() =>
            from site in (LinkShareSite[])Enum.GetValues(typeof(LinkShareSite))
            where site != LinkShareSite.All
            select site
        );

        public static string BitlyApiKey {
            get {
                return ScopeStorage.CurrentScope[_bitlyApiKey] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_bitlyApiKey] = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "bitly uses the login and not LogOn")]
        public static string BitlyLogin {
            get {
                return ScopeStorage.CurrentScope[_bitlyLogin] as string;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_bitlyLogin] = value;
            }
        }

        public static IHtmlString GetHtml(string pageTitle,
            string pageLinkBack = null,
            string twitterUserName = null,
            string additionalTweetText = null,
            params LinkShareSite[] linkSites) {

            if (string.IsNullOrEmpty(pageTitle)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "pageTitle"), "pageTitle");
            }

            string shortenedUrl;
            ConstructPageLinkBack(ref pageLinkBack, out shortenedUrl);
            StringBuilder sb = new StringBuilder();

            foreach (var site in GetSitesInOrder(linkSites)) {
                switch (site) {
                    case LinkShareSite.Delicious:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://delicious.com/save?v=5&noui&jump=close&url={0}&title={1}", HttpUtility.UrlEncode(shortenedUrl), HttpUtility.UrlEncode(pageTitle)),
                            "http://static.delicious.com/img/delicious.16.gif",
                            "Add to del.icio.us"));
                        break;

                    case LinkShareSite.Digg:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://digg.com/submit?url={0}&title={1}", HttpUtility.UrlEncode(pageLinkBack), HttpUtility.UrlEncode(pageTitle)),
                            "http://digg.com/img/badges/16x16-digg-guy.gif",
                            "Digg!"));
                        break;

                    case LinkShareSite.Facebook:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://www.facebook.com/sharer.php?u={0}&t={1}", HttpUtility.UrlEncode(shortenedUrl), HttpUtility.UrlEncode(pageTitle)),
                            "http://www.facebook.com/favicon.ico",
                            "Share on Facebook"));
                        break;

                    case LinkShareSite.Reddit:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://reddit.com/submit?url={0}&title={1}", HttpUtility.UrlEncode(shortenedUrl), HttpUtility.UrlEncode(pageTitle)),
                            "http://www.Reddit.com/favicon.ico",
                            "Reddit!"));
                        break;

                    case LinkShareSite.StumbleUpon:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://www.stumbleupon.com/submit?url={0}&title={1}", HttpUtility.UrlEncode(pageLinkBack), HttpUtility.UrlEncode(pageTitle)),
                            "http://cdn.stumble-upon.com/images/16x16_su_round.gif",
                            "Stumble it!"));
                        break;

                    case LinkShareSite.GoogleBuzz:
                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://www.google.com/reader/link?url={0}&title={1}", HttpUtility.UrlEncode(shortenedUrl), HttpUtility.UrlEncode(pageTitle)),
                            "http://mail.google.com/mail/help/images/whatsnew/buzz.gif",
                            "Share on Google Buzz"));
                        break;

                    case LinkShareSite.Twitter:
                        string status = string.Empty;
                        if (!string.IsNullOrEmpty(twitterUserName))
                            status += ", (via @" + twitterUserName + ")";
                        if (!string.IsNullOrEmpty(additionalTweetText))
                            status += " " + additionalTweetText;
                        status = "{1}: {0}" + status;

                        sb.Append(BuildTag(string.Format(CultureInfo.InvariantCulture, "http://twitter.com/home/?status={0}", HttpUtility.UrlEncode(string.Format(CultureInfo.InvariantCulture, status, shortenedUrl, pageTitle))),
                            "http://twitter.com/favicon.ico",
                            "Share on Twitter"));
                        break;
                }
            }
            return new HtmlString(sb.ToString());
        }

        private static string BuildTag(string linkUrl, string imgUrl, string title) {
            TagBuilder aTag = new TagBuilder("a");
            aTag.MergeAttribute("href", linkUrl);
            aTag.MergeAttribute("title", title);
            TagBuilder imgTag = new TagBuilder("img");
            imgTag.MergeAttribute("src", imgUrl);
            imgTag.MergeAttribute("style", "border:0; height:16px; width:16px; margin:0 1px;");
            imgTag.MergeAttribute("alt", title);
            imgTag.MergeAttribute("title", title);
            aTag.MergeAttribute("target", "_blank");
            aTag.InnerHtml = imgTag.ToString(TagRenderMode.SelfClosing);
            return aTag.ToString();
        }

        private static string GetShortenedUrl(string pageLinkBack) {
            if (String.IsNullOrEmpty(BitlyLogin) || String.IsNullOrEmpty(BitlyApiKey)) {
                return pageLinkBack;
            }
            string encodedPageLinkBack = HttpUtility.UrlEncode(pageLinkBack);
            string key = "Bitly_pageLinkBack_" + BitlyApiKey + "_" + encodedPageLinkBack;
            string shortUrl = WebCache.Get(key) as string;
            if (shortUrl != null) {
                return shortUrl;
            }

            string bitlyReq = "http://api.bit.ly/v3/shorten?format=txt&longUrl=" + encodedPageLinkBack + "&login=" + BitlyLogin + "&apiKey=" + BitlyApiKey;
            try {
                shortUrl = GetWebResponse(bitlyReq);
            }
            catch (WebException) {
                return pageLinkBack;
            }
            if (shortUrl != null) {
                WebCache.Set(key, shortUrl);
                return shortUrl;
            }
            return pageLinkBack;
        }

        private static string GetWebResponse(string address) {
            WebRequest request = WebRequest.Create(address);
            request.Method = "GET";
            request.Timeout = 5 * 1000; //5 seconds
            using (var response = (HttpWebResponse)request.GetResponse()) {
                if (response.StatusCode != HttpStatusCode.OK) {
                    return null;
                }
                using (Stream stream = response.GetResponseStream()) {
                    using (MemoryStream memStream = new MemoryStream()) {
                        stream.CopyTo(memStream);
                        // Review: Should we use the ContentEncoding from response?
                        return Encoding.UTF8.GetString(memStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Returns an ordered list of LinkShareSite based on position of "All" parameter occurs in the list.
        /// </summary>
        /// <remarks>
        /// The LinkShareSite is accepted as a params array.
        /// In the event that no value is provided or the LinkShareSite.All is the first param, we display all the sites in the order they appear in the enum.
        /// If not, the items we look for the first occurence of LinkShareSite.All in the array. 
        /// The items that appear before this appear in the order they are specified. The All is replaced by all items in the enum that were not already specified by the user 
        /// in the order they appear in the enum.
        /// e.g.  sites = [] { Twitter, Facebook, Digg, All }
        /// Would result in returning {Twitter, Facebook, Digg, Delicious, GoogleBuzz, Reddit, StumbleUpon} 
        /// </remarks>
        internal static IEnumerable<LinkShareSite> GetSitesInOrder(LinkShareSite[] linkSites) {
            var allSites = _allSites.Value;
            if (linkSites == null || !linkSites.Any() || linkSites.First() == LinkShareSite.All) {
                // Show all sites
                return allSites;
            }
            var result = linkSites.TakeWhile(c => c != LinkShareSite.All).ToList();
            if (result.Count != linkSites.Length) {
                return Enumerable.Concat(result, allSites.Except(result));
            }
            else {
                return result;
            }
        }

        private static void ConstructPageLinkBack(ref string pageLinkBack, out string shortenedUrl) {
            HttpContext context = HttpContext.Current;
            if ((pageLinkBack == null) && (context != null)) {
                pageLinkBack = context.Request.Url.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);
            }
            shortenedUrl = GetShortenedUrl(pageLinkBack);
        }
    }

    public enum LinkShareSite {
        Delicious,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Digg", Justification = "Correctly uses brandname")]
        Digg,
        GoogleBuzz,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Facebook", Justification = "Correctly uses brandname")]
        Facebook,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reddit", Justification = "Correctly uses brandname")]
        Reddit,
        StumbleUpon,
        Twitter,
        All
    }
}

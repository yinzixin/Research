namespace Microsoft.Web.Helpers {
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Internal.Web.Utils;

    /// <summary>
    /// Used to display a users Xbox Gamer Card
    /// </summary>
    /// <remarks>
    /// GamerCard requires the use of iframes, which are XHTML 1.0 compliant, but not XHTML 1.1 compliant.
    /// While HTML5 is generally backwards compatible and includes iframes, it does not yet include the
    /// scrolling and frameborder attributes.  There is no CSS alternative for these that can be used on
    /// the page that contains the iframe, so this helper can't be fully HTML5 compliant.
    /// </remarks>
    /// <see cref="http://www.xbox.com/en-us/myxbox/embedgamercard.htm"/>
    public static class GamerCard {
        /// <summary>
        /// Renders the GamerCard on the page
        /// </summary>
        /// <param name="gamerTag">The gamertag of the user whose gamer card is being rendered.</param>
        /// <returns></returns>
        public static HtmlString GetHtml(string gamerTag) {
            if (string.IsNullOrEmpty(gamerTag)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "gamerTag");
            }

            TagBuilder iframe = new TagBuilder("iframe");
            iframe.MergeAttribute("src", string.Format(CultureInfo.InvariantCulture, "http://gamercard.xbox.com/{0}.card", HttpUtility.UrlPathEncode(gamerTag)));
            iframe.MergeAttribute("scrolling", "no");
            iframe.MergeAttribute("frameborder", "0");
            iframe.MergeAttribute("height","140");
            iframe.MergeAttribute("width","204");
            iframe.SetInnerText(gamerTag);
            return new HtmlString(iframe.ToString());
        }
    }
}
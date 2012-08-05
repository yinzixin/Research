using System.Globalization;
using System.Text;
using System.Web;
using System;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.Web.Helpers
{
    public static class Twitter
    {
        public static IHtmlString Search(string searchQuery, 
                                        int width = 250,
                                        int height = 300,
                                        string title = null,
                                        string caption = null,
                                        string backgroundShellColor = "#8ec1da",
                                        string shellColor = "#ffffff",
                                        string tweetsBackgroundColor = "#ffffff",
                                        string tweetsColor = "#444444",
                                        string tweetsLinksColor = "#1985b5",                                        
                                        bool scrollBar = false,
                                        bool loop = true,
                                        bool live = true,
                                        bool hashTags = true,
                                        bool timestamp = true,
                                        bool avatars = true,
                                        string behavior = "default",
                                        int searchInterval = 6000) 
        {
            if (string.IsNullOrEmpty(searchQuery)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "searchQuery"), "searchQuery");
            }
            searchQuery = HttpUtility.HtmlEncode(searchQuery);
            title = ((title == null) ? searchQuery : HttpUtility.HtmlEncode(title));
            caption = ((caption == null) ? searchQuery : HttpUtility.HtmlEncode(caption));

            string preFix = string.Format(CultureInfo.InvariantCulture, _TwitterSearchPrefixTemplate,  
                HttpUtility.JavaScriptStringEncode(searchQuery, addDoubleQuotes: true), 
                HttpUtility.JavaScriptStringEncode(title, addDoubleQuotes: true), 
                HttpUtility.JavaScriptStringEncode(caption, addDoubleQuotes: true));
            string middleString = GetTwitterOptionsString(width, height, backgroundShellColor, shellColor,
                                                    tweetsBackgroundColor, tweetsColor, tweetsLinksColor, 
                                                    scrollBar, loop, live, hashTags, timestamp, avatars, 
                                                    behavior, searchInterval);
            return new HtmlString(preFix + middleString + _TwitterSearchPostfix);
        }

        public static HtmlString Profile(string twitterUserName, 
                                         int width = 250,
                                         int height = 300,                                            
                                         string backgroundShellColor = "#333333",
                                         string shellColor = "#ffffff",
                                         string tweetsBackgroundColor = "#000000",
                                         string tweetsColor = "#ffffff",
                                         string tweetsLinksColor = "#4aed05",
                                         int numberOfTweets = 4,                            
                                         bool scrollBar = false,
                                         bool loop = false,
                                         bool live = false,
                                         bool hashTags = true,
                                         bool timestamp = true,
                                         bool avatars = false,
                                         string behavior = "all",
                                         int searchInterval = 6000)
        {
            if (string.IsNullOrEmpty(twitterUserName)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "twitterUserName"), "twitterUserName");
            }
            twitterUserName = HttpUtility.HtmlEncode(twitterUserName);
            if (numberOfTweets < 1) {
                throw new ArgumentOutOfRangeException("numberOfTweets", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 1));
            }
            string prefix = string.Format(CultureInfo.InvariantCulture, _TwitterProfilePrefixTemplate, numberOfTweets.ToString(CultureInfo.InvariantCulture));
            string middleString = GetTwitterOptionsString(width, height, backgroundShellColor, shellColor,
                                                    tweetsBackgroundColor, tweetsColor, tweetsLinksColor, 
                                                    scrollBar, loop, live, hashTags, timestamp, avatars, 
                                                    behavior, searchInterval);
            string postFix = string.Format(CultureInfo.InvariantCulture, _TwitterProfilePostfixTemplate, HttpUtility.JavaScriptStringEncode(twitterUserName, addDoubleQuotes: true));
            return new HtmlString(prefix + middleString + postFix);
        }

        private static string GetTwitterOptionsString(
            int width, int height,
            string backgroundShellColor, string shellColor,
            string tweetsBackgroundColor, string tweetsColor, string tweetsLinksColor,
            bool scrollBar, bool loop, bool live, bool hashTags,
            bool timestamp, bool avatars, string behavior, int searchInterval)
        {
            if (width < 0) {
                throw new ArgumentOutOfRangeException("width", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (height < 0) {
                throw new ArgumentOutOfRangeException("height", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (searchInterval < 1) {
                throw new ArgumentOutOfRangeException("searchInterval", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 1));
            }

            if (string.IsNullOrEmpty(backgroundShellColor)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "backgroundShellColor"), "backgroundShellColor");
            }
            if (string.IsNullOrEmpty(shellColor)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "shellColor"), "shellColor");
            }
            if (string.IsNullOrEmpty(tweetsBackgroundColor)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "tweetsBackgroundColor"), "tweetsBackgroundColor");
            }
            if (string.IsNullOrEmpty(tweetsColor)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "tweetsColor"), "tweetsColor");
            }
            if (string.IsNullOrEmpty(tweetsLinksColor)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "tweetsLinksColor"), "tweetsLinksColor");
            }
            if (string.IsNullOrEmpty(behavior)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "behavior"), "behavior");
            }
            return string.Format(CultureInfo.InvariantCulture, _TwitterCommonTemplate,
                                searchInterval.ToString(CultureInfo.InvariantCulture),
                                (width==0) ?  "'auto'" : width.ToString(CultureInfo.InvariantCulture),
                                height.ToString(CultureInfo.InvariantCulture),
                                HttpUtility.JavaScriptStringEncode(backgroundShellColor, addDoubleQuotes: true),
                                HttpUtility.JavaScriptStringEncode(shellColor, addDoubleQuotes: true),
                                HttpUtility.JavaScriptStringEncode(tweetsBackgroundColor, addDoubleQuotes: true),
                                HttpUtility.JavaScriptStringEncode(tweetsColor, addDoubleQuotes: true),
                                HttpUtility.JavaScriptStringEncode(tweetsLinksColor, addDoubleQuotes: true),
                                scrollBar ? "true" : "false",
                                loop ? "true" : "false",
                                live ? "true" : "false",
                                hashTags ? "true" : "false",
                                timestamp ? "true" : "false",
                                avatars ? "true" : "false",
                                HttpUtility.JavaScriptStringEncode(behavior, addDoubleQuotes: true));
        }
        const string _TwitterCommonTemplate = @"
        interval: {0},
        width: {1},
        height: {2},
        theme: {{
            shell: {{
                background: {3},
                color: {4}
            }},
            tweets: {{
                background: {5},
                color: {6},
                links: {7}
            }}
        }},
        features: {{
            scrollbar: {8},
            loop: {9},
            live: {10},
            hashtags: {11},
            timestamp: {12},
            avatars: {13},
            behavior: {14}
        }}";

        const string _TwitterSearchPrefixTemplate =
"<script src=\"http://widgets.twimg.com/j/2/widget.js\" type=\"text/javascript\" " + @"></script>
<script " + "type=\"text/javascript\" " + @">
    new TWTR.Widget({{
        version: 2,
        type: ""search"",
        search: {0},
        title: {1},
        subject: {2},";

        const string _TwitterSearchPostfix = @"
    }).render().start();
</script>";

        const string _TwitterProfilePrefixTemplate =
"<script src=\"http://widgets.twimg.com/j/2/widget.js\" type=\"text/javascript\" " + @"></script>
<script " + "type=\"text/javascript\" " + @">
    new TWTR.Widget({{
        version: 2,
        type: ""profile"",
        rpp: {0},";

        const string _TwitterProfilePostfixTemplate = @"
    }}).render().setUser({0}).start();
</script>";

    }
}

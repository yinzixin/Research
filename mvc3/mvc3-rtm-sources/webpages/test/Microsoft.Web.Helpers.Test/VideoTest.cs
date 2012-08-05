namespace Microsoft.Web.Helpers.Test {
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages.TestUtils;
    using Moq;

    [TestClass]
    public class VideoTest {

        private VirtualPathUtilityWrapper _pathUtility = new VirtualPathUtilityWrapper();

        [TestMethod]
        public void FlashCannotOverrideHtmlAttributes() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", htmlAttributes: new { cLASSid = "CanNotOverride" });
            }, "htmlAttributes", "Property \"cLASSid\" cannot be set through this argument.");
        }

        [TestMethod]
        public void FlashDefaults() {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf").ToString().Replace("\r\n", "");
            Assert.IsTrue(html.StartsWith(
                "<object classid=\"clsid:d27cdb6e-ae6d-11cf-96b8-444553540000\" " +
                "codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab\" type=\"application/x-oleobject\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"movie\" value=\"http://foo.bar.com/foo.swf\" />"));
            Assert.IsTrue(html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" />"));
            Assert.IsTrue(html.EndsWith("</object>"));
        }

        [TestMethod]
        public void FlashThrowsWhenPathIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Flash(GetContext(), _pathUtility, String.Empty);
            }, "path");
        }

        [TestMethod]
        public void FlashThrowsWhenPathIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Flash(GetContext(), _pathUtility, null);
            }, "path");
        }

        [TestMethod]
        public void FlashWithExposedOptions() {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", width: "100px", height: "100px",
            play: false, loop: false, menu: false, bgColor: "#000", quality: "Q", scale: "S", windowMode: "WM",
            baseUrl: "http://foo.bar.com/", version: "1.0.0.0", htmlAttributes: new { id = "fl" }, embedName: "efl").ToString().Replace("\r\n", "");

            Assert.IsTrue(html.StartsWith(
                "<object classid=\"clsid:d27cdb6e-ae6d-11cf-96b8-444553540000\" " +
                "codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=1,0,0,0\" " +
                "height=\"100px\" id=\"fl\" type=\"application/x-oleobject\" width=\"100px\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"play\" value=\"False\" />"));
            Assert.IsTrue(html.Contains("<param name=\"loop\" value=\"False\" />"));
            Assert.IsTrue(html.Contains("<param name=\"menu\" value=\"False\" />"));
            Assert.IsTrue(html.Contains("<param name=\"bgColor\" value=\"#000\" />"));
            Assert.IsTrue(html.Contains("<param name=\"quality\" value=\"Q\" />"));
            Assert.IsTrue(html.Contains("<param name=\"scale\" value=\"S\" />"));
            Assert.IsTrue(html.Contains("<param name=\"wmode\" value=\"WM\" />"));
            Assert.IsTrue(html.Contains("<param name=\"base\" value=\"http://foo.bar.com/\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.IsTrue(embed.Success);
            Assert.IsTrue(embed.Value.StartsWith("<embed src=\"http://foo.bar.com/foo.swf\" width=\"100px\" height=\"100px\" name=\"efl\" type=\"application/x-shockwave-flash\" "));
            Assert.IsTrue(embed.Value.Contains("play=\"False\""));
            Assert.IsTrue(embed.Value.Contains("loop=\"False\""));
            Assert.IsTrue(embed.Value.Contains("menu=\"False\""));
            Assert.IsTrue(embed.Value.Contains("bgColor=\"#000\""));
            Assert.IsTrue(embed.Value.Contains("quality=\"Q\""));
            Assert.IsTrue(embed.Value.Contains("scale=\"S\""));
            Assert.IsTrue(embed.Value.Contains("wmode=\"WM\""));
            Assert.IsTrue(embed.Value.Contains("base=\"http://foo.bar.com/\""));
        }

        [TestMethod]
        public void FlashWithUnexposedOptions() {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", options: new { X = "Y", Z = 123 }).ToString().Replace("\r\n", "");
            Assert.IsTrue(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.IsTrue(html.Contains("<param name=\"Z\" value=\"123\" />"));
            // note - can't guarantee order of optional params:
            Assert.IsTrue(
                html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" X=\"Y\" Z=\"123\" />") ||
                html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" Z=\"123\" X=\"Y\" />")
            );
        }

        [TestMethod]
        public void MediaPlayerCannotOverrideHtmlAttributes() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", htmlAttributes: new { cODEbase = "CanNotOverride" });
            }, "htmlAttributes", "Property \"cODEbase\" cannot be set through this argument.");
        }

        [TestMethod]
        public void MediaPlayerDefaults() {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv").ToString().Replace("\r\n", "");
            Assert.IsTrue(html.StartsWith(
                "<object classid=\"clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"URL\" value=\"http://foo.bar.com/foo.wmv\" />"));
            Assert.IsTrue(html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" />"));
            Assert.IsTrue(html.EndsWith("</object>"));
        }

        [TestMethod]
        public void MediaPlayerThrowsWhenPathIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.MediaPlayer(GetContext(), _pathUtility, String.Empty);
            }, "path");
        }

        [TestMethod]
        public void MediaPlayerThrowsWhenPathIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.MediaPlayer(GetContext(), _pathUtility, null);
            }, "path");
        }

        [TestMethod]
        public void MediaPlayerWithExposedOptions() {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", width: "100px", height: "100px",
            autoStart: false, playCount: 2, uiMode: "UIMODE", stretchToFit: true, enableContextMenu: false, mute: true,
            volume: 1, baseUrl: "http://foo.bar.com/", htmlAttributes: new { id = "mp" }, embedName: "emp").ToString().Replace("\r\n", "");
            Assert.IsTrue(html.StartsWith(
                "<object classid=\"clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6\" height=\"100px\" id=\"mp\" width=\"100px\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"URL\" value=\"http://foo.bar.com/foo.wmv\" />"));
            Assert.IsTrue(html.Contains("<param name=\"autoStart\" value=\"False\" />"));
            Assert.IsTrue(html.Contains("<param name=\"playCount\" value=\"2\" />"));
            Assert.IsTrue(html.Contains("<param name=\"uiMode\" value=\"UIMODE\" />"));
            Assert.IsTrue(html.Contains("<param name=\"stretchToFit\" value=\"True\" />"));
            Assert.IsTrue(html.Contains("<param name=\"enableContextMenu\" value=\"False\" />"));
            Assert.IsTrue(html.Contains("<param name=\"mute\" value=\"True\" />"));
            Assert.IsTrue(html.Contains("<param name=\"volume\" value=\"1\" />"));
            Assert.IsTrue(html.Contains("<param name=\"baseURL\" value=\"http://foo.bar.com/\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.IsTrue(embed.Success);
            Assert.IsTrue(embed.Value.StartsWith("<embed src=\"http://foo.bar.com/foo.wmv\" width=\"100px\" height=\"100px\" name=\"emp\" type=\"application/x-mplayer2\" "));
            Assert.IsTrue(embed.Value.Contains("autoStart=\"False\""));
            Assert.IsTrue(embed.Value.Contains("playCount=\"2\""));
            Assert.IsTrue(embed.Value.Contains("uiMode=\"UIMODE\""));
            Assert.IsTrue(embed.Value.Contains("stretchToFit=\"True\""));
            Assert.IsTrue(embed.Value.Contains("enableContextMenu=\"False\""));
            Assert.IsTrue(embed.Value.Contains("mute=\"True\""));
            Assert.IsTrue(embed.Value.Contains("volume=\"1\""));
            Assert.IsTrue(embed.Value.Contains("baseURL=\"http://foo.bar.com/\""));
        }

        [TestMethod]
        public void MediaPlayerWithUnexposedOptions() {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", options: new { X = "Y", Z = 123 }).ToString().Replace("\r\n", "");
            Assert.IsTrue(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.IsTrue(html.Contains("<param name=\"Z\" value=\"123\" />"));
            Assert.IsTrue(
                html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" X=\"Y\" Z=\"123\" />") ||
                html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" Z=\"123\" X=\"Y\" />")
            );
        }

        [TestMethod]
        public void SilverlightCannotOverrideHtmlAttributes() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", "100px",
                htmlAttributes: new { WIDTH = "CanNotOverride" });
            }, "htmlAttributes", "Property \"WIDTH\" cannot be set through this argument.");
        }

        [TestMethod]
        public void SilverlightDefaults() {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", "100px").ToString().Replace("\r\n", "");
            Assert.IsTrue(html.StartsWith(
                "<object data=\"data:application/x-silverlight-2,\" height=\"100px\" type=\"application/x-silverlight-2\" " +
                "width=\"100px\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"source\" value=\"http://foo.bar.com/foo.xap\" />"));
            Assert.IsTrue(html.Contains(
                "<a href=\"http://go.microsoft.com/fwlink/?LinkID=149156\" style=\"text-decoration:none\">" +
                "<img src=\"http://go.microsoft.com/fwlink?LinkId=108181\" alt=\"Get Microsoft Silverlight\" " +
                "style=\"border-style:none\"/></a>"));
            Assert.IsTrue(html.EndsWith("</object>"));
        }

        [TestMethod]
        public void SilverlightThrowsWhenPathIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, String.Empty, "100px", "100px");
            }, "path");
        }

        [TestMethod]
        public void SilverlightThrowsWhenPathIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, null, "100px", "100px");
            }, "path");
        }

        [TestMethod]
        public void SilverlightThrowsWhenHeightIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", String.Empty);
            }, "height");
        }

        [TestMethod]
        public void SilverlightThrowsWhenHeightIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", null);
            }, "height");
        }

        [TestMethod]
        public void SilverlightThrowsWhenWidthIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", String.Empty, "100px");
            }, "width");
        }

        [TestMethod]
        public void SilverlightThrowsWhenWidthIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", null, "100px");
            }, "width");
        }

        [TestMethod]
        public void SilverlightWithExposedOptions() {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", width: "85%", height: "85%",
            bgColor: "red", initParameters: "X=Y", minimumVersion: "1.0.0.0", autoUpgrade: false,
            htmlAttributes: new { id = "sl" }).ToString().Replace("\r\n", "");
            Assert.IsTrue(html.StartsWith(
                "<object data=\"data:application/x-silverlight-2,\" height=\"85%\" id=\"sl\" " +
                "type=\"application/x-silverlight-2\" width=\"85%\" >"
            ));
            Assert.IsTrue(html.Contains("<param name=\"background\" value=\"red\" />"));
            Assert.IsTrue(html.Contains("<param name=\"initparams\" value=\"X=Y\" />"));
            Assert.IsTrue(html.Contains("<param name=\"minruntimeversion\" value=\"1.0.0.0\" />"));
            Assert.IsTrue(html.Contains("<param name=\"autoUpgrade\" value=\"False\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.IsFalse(embed.Success);
        }

        [TestMethod]
        public void SilverlightWithUnexposedOptions() {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", width: "50px", height: "50px",
            options: new { X = "Y", Z = 123 } ).ToString().Replace("\r\n", "");
            Assert.IsTrue(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.IsTrue(html.Contains("<param name=\"Z\" value=\"123\" />"));
        }

        [TestMethod]
        public void ValidatePathResolvesExistingLocalPath() {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Mock<VirtualPathUtilityBase> pathUtility = new Mock<VirtualPathUtilityBase>();
            pathUtility.Setup(p => p.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns(path);
            pathUtility.Setup(p => p.ToAbsolute(It.IsAny<string>())).Returns(path);

            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(path);
            HttpContextBase context = GetContext(serverMock.Object);

            string html = Video.Flash(context, pathUtility.Object, "foo.bar").ToString();
            Assert.IsTrue(html.StartsWith("<object"));
            Assert.IsTrue(html.Contains(HttpUtility.HtmlAttributeEncode(HttpUtility.UrlPathEncode(path))));
        }

        [TestMethod]
        public void ValidatePathThrowsForNonExistingLocalPath() {
            string path = "c:\\does\\not\\exist.swf";
            Mock<VirtualPathUtilityBase> pathUtility = new Mock<VirtualPathUtilityBase>();
            pathUtility.Setup(p => p.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns(path);
            pathUtility.Setup(p => p.ToAbsolute(It.IsAny<string>())).Returns(path);

            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(path);
            HttpContextBase context = GetContext(serverMock.Object);

            ExceptionAssert.Throws<InvalidOperationException>(() => {
                Video.Flash(context, pathUtility.Object, "exist.swf");
            }, "The media file \"exist.swf\" does not exist.");
        }

        private static HttpContextBase GetContext(HttpServerUtilityBase serverUtility = null) {
            // simple mocked context - won't reference as long as path starts with 'http'
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Server).Returns(serverUtility);
            return contextMock.Object;
        }

    }

}

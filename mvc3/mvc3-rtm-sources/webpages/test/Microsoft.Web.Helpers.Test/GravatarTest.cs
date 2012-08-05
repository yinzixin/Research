namespace Microsoft.Web.Helpers.Test {
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.Helpers.Resources;
    using System.Web.WebPages.TestUtils;
    using System.Web.Helpers.Test;

    [TestClass]
    public class GravatarTest {

        [TestMethod]
        public void GetUrlDefaults() {
            string url = Gravatar.GetUrl("foo@bar.com");
            Assert.AreEqual("http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80", url);
        }

        [TestMethod]
        public void RenderEncodesDefaultImageUrl() {
            string render = Gravatar.GetHtml("foo@bar.com", defaultImage: "http://example.com/images/example.jpg").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;d=http%3a%2f%2fexample.com%2fimages%2fexample.jpg\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderLowerCasesEmail() {
            string render = Gravatar.GetHtml("FOO@BAR.COM").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderThrowsWhenEmailIsEmpty() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Gravatar.GetHtml(String.Empty);
            }, "email");
        }

        [TestMethod]
        public void RenderThrowsWhenEmailIsNull() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                Gravatar.GetHtml(null);
            }, "email");
        }

        [TestMethod]
        public void RenderThrowsWhenImageSizeIsLessThanZero() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Gravatar.GetHtml("foo@bar.com", imageSize: -1);
            }, "imageSize", HelpersToolkitResources.Gravatar_InvalidImageSize);
        }

        [TestMethod]
        public void RenderThrowsWhenImageSizeIsZero() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Gravatar.GetHtml("foo@bar.com", imageSize: 0);
            }, "imageSize", HelpersToolkitResources.Gravatar_InvalidImageSize);
        }

        [TestMethod]
        public void RenderThrowsWhenImageSizeIsGreaterThan512() {
            ExceptionAssert.ThrowsArgumentException(() => {
                Gravatar.GetHtml("foo@bar.com", imageSize: 513);
            }, "imageSize", HelpersToolkitResources.Gravatar_InvalidImageSize);
        }

        [TestMethod]
        public void RenderTrimsEmail() {
            string render = Gravatar.GetHtml(" \t foo@bar.com\t\r\n").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderUsesDefaultImage() {
            string render = Gravatar.GetHtml("foo@bar.com", defaultImage: "wavatar").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;d=wavatar\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderUsesImageSize() {
            string render = Gravatar.GetHtml("foo@bar.com", imageSize: 512).ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=512\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderUsesRating() {
            string render = Gravatar.GetHtml("foo@bar.com", rating: GravatarRating.G).ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;r=g\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderWithAttributes() {
            string render = Gravatar.GetHtml("foo@bar.com",
                attributes: new { id = "gravatar", alT = "<b>foo@bar.com</b>", srC="ignored" }).ToString();
            // beware of attributes ordering in tests
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alT=\"&lt;b>foo@bar.com&lt;/b>\" id=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderWithDefaults() {
            string render = Gravatar.GetHtml("foo@bar.com").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [TestMethod]
        public void RenderWithExtension() {
            string render = Gravatar.GetHtml("foo@bar.com", imageExtension: ".png").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8.png?s=80\" alt=\"gravatar\" />",
                render);

            render = Gravatar.GetHtml("foo@bar.com", imageExtension: "xyz").ToString();
            Assert.AreEqual(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8.xyz?s=80\" alt=\"gravatar\" />",
                render);
        }
    }
}

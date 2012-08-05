namespace Microsoft.Web.Helpers.Test {
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages.TestUtils;
    using System.Web.Helpers.Test;

    [TestClass]
    public class GamerCardTest {
        [TestMethod]
        public void RenderThrowsWhenGamertagIsEmpty() {
            // Act & Assert 
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                GamerCard.GetHtml(string.Empty);
            }, "gamerTag");
        }

        [TestMethod]
        public void RenderThrowsWhenGamertagIsNull() {
            // Act & Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                GamerCard.GetHtml(null);
            }, "gamerTag");
        }

        [TestMethod]
        public void RenderGeneratesProperMarkupWithSimpleGamertag() {
            // Arrange 
            string expectedHtml = "<iframe frameborder=\"0\" height=\"140\" scrolling=\"no\" src=\"http://gamercard.xbox.com/osbornm.card\" width=\"204\">osbornm</iframe>";

            // Act
            string html = GamerCard.GetHtml("osbornm").ToHtmlString();

            // Assert
            Assert.AreEqual(expectedHtml, html);
        }

        [TestMethod]
        public void RenderGeneratesProperMarkupWithComplexGamertag() {
            // Arrange 
            string expectedHtml = "<iframe frameborder=\"0\" height=\"140\" scrolling=\"no\" src=\"http://gamercard.xbox.com/matthew%20osborn&#39;s.card\" width=\"204\">matthew osborn&#39;s</iframe>";

            // Act
            string html = GamerCard.GetHtml("matthew osborn's").ToHtmlString();

            // Assert
            Assert.AreEqual(expectedHtml, html);
        }

    }
}

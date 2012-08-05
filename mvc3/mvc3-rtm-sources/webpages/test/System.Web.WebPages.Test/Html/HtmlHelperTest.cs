using System.Collections.Generic;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class HtmlHelperTest {
        [TestMethod]
        public void ValidationInputCssClassNameThrowsWhenAssignedNull() {
            // Act and Assert
            ExceptionAssert.ThrowsArgNull(() => HtmlHelper.ValidationInputCssClassName = null, "value");
        }

        [TestMethod]
        public void ValidationSummaryClassNameThrowsWhenAssignedNull() {
            // Act and Assert
            ExceptionAssert.ThrowsArgNull(() => HtmlHelper.ValidationSummaryClass = null, "value");
        }

        [TestMethod]
        public void EncodeObject() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());
            object text = "<br />" as object;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;br /&gt;", "Text is not being properly HTML-encoded.");
        }

        [TestMethod]
        public void EncodeObjectNull() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());
            object text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.AreEqual(String.Empty, encodedHtml);
        }

        [TestMethod]
        public void EncodeString() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());
            var text = "<br />";

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;br /&gt;", "Text is not being properly HTML-encoded.");
        }

        [TestMethod]
        public void EncodeStringNull() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());
            string text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.AreEqual("", encodedHtml);
        }

        [TestMethod]
        public void ObjectToDictionaryWithNullObjectReturnsEmptyDictionary() {
            // Arrange
            object dict = null;

            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(dict);

            Assert.IsNotNull(dictValues);
            Assert.AreEqual(0, dictValues.Count);
        }

        [TestMethod]
        public void ObjectToDictionaryWithPlainObjectTypeReturnsEmptyDictionary() {
            // Arrange
            object dict = new object();

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(dict);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(0, dictValues.Count);
        }

        [TestMethod]
        public void ObjectToDictionaryWithPrimitiveTypeLooksUpPublicProperties() {
            // Arrange
            object dict = "test";

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(dict);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(1, dictValues.Count);
            Assert.AreEqual(4, dictValues["Length"]);
        }

        [TestMethod]
        public void ObjectToDictionaryWithAnonymousTypeLooksUpProperties() {
            // Arrange
            object dict = new { test = "value", other = 1 };

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(dict);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(2, dictValues.Count);
            Assert.AreEqual("value", dictValues["test"]);
            Assert.AreEqual(1, dictValues["other"]);

        }

        [TestMethod]
        public void ObjectToDictionaryReturnsCaseInsensitiveDictionary() {
            // Arrange
            object dict = new { TEST = "value", oThEr = 1 };

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(dict);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(2, dictValues.Count);
            Assert.AreEqual("value", dictValues["test"]);
            Assert.AreEqual(1, dictValues["other"]);
        }

        [TestMethod]
        public void RawReturnsWrapperMarkup() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());
            string markup = "<b>bold</b>";

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(markup);

            // Assert
            Assert.AreEqual("<b>bold</b>", markupHtml.ToString());
            Assert.AreEqual("<b>bold</b>", markupHtml.ToHtmlString());
        }
    }
}

using System.Collections.Generic;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class TextAreaExtensionsTest {
        [TestMethod]
        public void TextAreaWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act and assert
            ExceptionAssert.ThrowsArgumentException(() => helper.TextArea(null), "name", "Value cannot be null or an empty string.");

            // Act and assert
            ExceptionAssert.ThrowsArgumentException(() => helper.TextArea(String.Empty), "name", "Value cannot be null or an empty string.");
        }

        [TestMethod]
        public void TextAreaWithDefaultRowsAndCols() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextArea("foo");

            // Assert
            Assert.AreEqual(@"<textarea cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>", html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithZeroRowsAndColumns() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextArea("foo", null, 0, 0, null);

            // Assert
            Assert.AreEqual(@"<textarea id=""foo"" name=""foo""></textarea>", html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithNonZeroRowsAndColumns() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextArea("foo", null, 4, 10, null);

            // Assert
            Assert.AreEqual(@"<textarea cols=""10"" id=""foo"" name=""foo"" rows=""4""></textarea>", html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithObjectAttributes() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "foo-value");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextArea("foo", new { attr = "value", cols = 6 });

            // Assert
            Assert.AreEqual(@"<textarea attr=""value"" cols=""6"" id=""foo"" name=""foo"" rows=""2"">foo-value</textarea>", html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithExplicitValue() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextArea("foo", "explicit-foo-value", new { attr = "attr-value", cols = 6 });

            // Assert
            Assert.AreEqual(@"<textarea attr=""attr-value"" cols=""6"" id=""foo"" name=""foo"" rows=""2"">explicit-foo-value</textarea>", 
                html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithDictionaryAttributes() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = new HtmlHelper(modelState);
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", attributes);

            // Assert
            Assert.AreEqual(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15"">explicit-foo-value</textarea>", 
                html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithNoValueAndObjectAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", attributes);

            // Assert
            Assert.AreEqual(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15""></textarea>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithNullValue() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = new HtmlHelper(modelState);
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", null, attributes);

            // Assert
            Assert.AreEqual(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15"">explicit-foo-value</textarea>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithError() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty);

            // Assert
            Assert.AreEqual(@"<textarea class=""field-validation-error"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void TextAreaWithErrorAndCustomCssClass() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty, new { @class = "my-css" });

            // Assert
            Assert.AreEqual(@"<textarea class=""field-validation-error my-css"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                html.ToHtmlString());
        }

        // [TestMethod]
        // Cant test this in multi-threaded
        public void TextAreaWithCustomErrorClass() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper.ValidationInputCssClassName = "custom-field-validation-error";
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty, new { @class = "my-css" });

            // Assert
            Assert.AreEqual(@"<textarea class=""custom-field-validation-error my-css"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                html.ToHtmlString());
        }
    }
}

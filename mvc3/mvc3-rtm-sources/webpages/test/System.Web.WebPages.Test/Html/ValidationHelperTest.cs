using System.Collections.Generic;
using System.Web.WebPages.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace System.Web.WebPages.Test {
    [TestClass]
    public class ValidationHelperTest {
        [TestMethod]
        public void ValidationMessageWithNoErrorResultsInNullString() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = htmlHelper.ValidationMessage("does-not-exist");

            Assert.IsNull(html);
        }

        [TestMethod]
        public void ValidationMessageAllowsEmptyModelName() {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();
            dictionary.AddError("test", "some error text");
            HtmlHelper htmlHelper = new HtmlHelper(dictionary);

            // Act 
            var html = htmlHelper.ValidationMessage("test");

            // Assert
            Assert.AreEqual(@"<span class=""field-validation-error"">some error text</span>", html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationMessageReturnsFirstError() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act 
            var html = htmlHelper.ValidationMessage("foo");

            // Assert
            Assert.AreEqual(@"<span class=""field-validation-error"">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationMessageReturnsNullForInvalidName() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("baz");

            // Assert
            Assert.IsNull(html, "html should be null if name is invalid.");
        }

        [TestMethod]
        public void ValidationMessageReturnsWithObjectAttributes() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("foo", new { attr = "attr-value" });

            // Assert
            Assert.AreEqual(@"<span attr=""attr-value"" class=""field-validation-error"">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationMessageReturnsWithCustomMessage() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Atc
            var html = htmlHelper.ValidationMessage("foo", "bar error");

            // Assert
            Assert.AreEqual(@"<span class=""field-validation-error"">bar error</span>", html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationMessageReturnsWithCustomMessageAndObjectAttributes() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("foo", "bar error", new { baz = "baz" });

            // Assert
            Assert.AreEqual(@"<span baz=""baz"" class=""field-validation-error"">bar error</span>", html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationMessageWithModelStateAndNoErrors() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("baz");

            // Assert
            Assert.IsNull(html, "html should be null if there are no errors");
        }

        [TestMethod]
        public void ValidationSummary() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary();

            // Assert
            Assert.AreEqual(@"<div class=""validation-summary-errors""><ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithMessage() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary("test message");

            // Assert
            Assert.AreEqual(@"<div class=""validation-summary-errors""><span>test message</span>
<ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithFormErrors() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithFormErrors());

            // Act
            var html = htmlHelper.ValidationSummary();

            // Assert
            Assert.AreEqual(@"<div class=""validation-summary-errors""><ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
<li>some form error &lt;1&gt;</li>
<li>some form error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithFormErrorsAndExcludeFieldErrors() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithFormErrors());

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.AreEqual(@"<div class=""validation-summary-errors""><ul>
<li>some form error &lt;1&gt;</li>
<li>some form error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }



        [TestMethod]
        public void ValidationSummaryWithObjectProperties() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary(new { attr = "attr-value", @class = "my-class" });

            // Assert
            Assert.AreEqual(@"<div attr=""attr-value"" class=""validation-summary-errors my-class""><ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithDictionary() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary(new Dictionary<string, object> 
                { { "attr", "attr-value"} , { "class", "my-class" }} );

            // Assert
            Assert.AreEqual(@"<div attr=""attr-value"" class=""validation-summary-errors my-class""><ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithDictionaryAndMessage() {
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary( "This is a message.", new Dictionary<string, object> 
                { { "attr", "attr-value" }, { "class", "my-class" } });

            // Assert
            Assert.AreEqual(@"<div attr=""attr-value"" class=""validation-summary-errors my-class""><span>This is a message.</span>
<ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        //[TestMethod]
        // Cant test this, as it sets a static property 
        public void ValidationSummaryWithCustomValidationSummaryClass() {
            // Arrange
            HtmlHelper.ValidationSummaryClass = "my-val-class";
            HtmlHelper htmlHelper = new HtmlHelper(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary("This is a message.", new Dictionary<string, object> { { "attr", "attr-value" }, { "class", "my-class" } });

            // Assert
            Assert.AreEqual(@"<div attr=""attr-value"" class=""my-val-class my-class""><span>This is a message.</span>
<ul>
<li>foo error &lt;1&gt;</li>
<li>foo error &lt;2&gt;</li>
<li>bar error &lt;1&gt;</li>
<li>bar error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        [TestMethod]
        public void ValidationSummaryWithNoErrorReturnsNull() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = htmlHelper.ValidationSummary();

            // Assert
            Assert.IsNull(html);
        }

        [TestMethod]
        public void ValidationSummaryWithNoFormErrorsAndExcludedFieldErrorsReturnsNull() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            modelState.AddError("bar", "error");

            HtmlHelper htmlHelper = new HtmlHelper(modelState);

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.IsNull(html);
        }

        [TestMethod]
        public void ValidationSummaryWithMultipleFormErrorsAndExcludedFieldErrors() {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddFormError("error <1>");
            modelState.AddFormError("error <2>");

            HtmlHelper htmlHelper = new HtmlHelper(modelState);

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.AreEqual(@"<div class=""validation-summary-errors""><ul>
<li>error &lt;1&gt;</li>
<li>error &lt;2&gt;</li>
</ul></div>"
                , html.ToHtmlString());
        }

        private static ModelStateDictionary GetModelStateWithErrors() {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "foo error <1>");
            modelState.AddError("foo", "foo error <2>");
            modelState.AddError("bar", "bar error <1>");
            modelState.AddError("bar", "bar error <2>");
            return modelState;
        }

        private static ModelStateDictionary GetModelStateWithFormErrors() {
            ModelStateDictionary modelState = GetModelStateWithErrors();
            modelState.AddFormError("some form error <1>");
            modelState.AddFormError("some form error <2>");
            return modelState;
        }
    }
}

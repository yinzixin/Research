using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Web.WebPages;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class InputHelperTest {
        private static readonly IDictionary<string, object> _attributesDictionary = new Dictionary<string, object> { { "baz", "BazValue" } };
        private static readonly object _attributesObject = new { baz = "BazValue" };

        [TestMethod]
        public void HiddenWithBinaryArrayValueRendersBase64EncodedValue() {
            // Arrange
            HtmlHelper htmlHelper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var result = htmlHelper.Hidden("ProductName", new Binary(new byte[] { 23, 43, 53 }));

            // Assert
            Assert.AreEqual("<input id=\"ProductName\" name=\"ProductName\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act & Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Hidden(String.Empty), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Hidden(null), "name");
        }

        [TestMethod]
        public void HiddenWithExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", "DefaultFoo");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithExplicitValueAndAttributesDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", "DefaultFoo", new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithExplicitValueAndObjectDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", "DefaultFoo", new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithExplicitValueNull() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", value: null);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value="""" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithModelValue() {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(model);

            // Act
            var html = helper.Hidden("foo");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithModelValueAndAttributesDictionary() {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(model);

            // Act
            var html = helper.Hidden("foo", null, new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithImplicitValueAndAttributesObject() {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(model);

            // Act
            var html = helper.Hidden("foo", null, new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithNameAndValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", "fooValue");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithExplicitOverwritesAttributeValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Hidden("foo", "fooValue", new { value = "barValue" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void HiddenWithModelValueOverwritesAttributeValue() {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "fooValue");
            HtmlHelper helper = new HtmlHelper(model);

            // Act
            var html = helper.Hidden("foo", null, new { value = "barValue" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        // Password

        [TestMethod]
        public void PasswordWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act & Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Password(String.Empty), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Password(null), "name");
        }

        [TestMethod]
        public void PasswordDictionaryOverridesImplicitParameters() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "Some Value", new { type = "fooType" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""Some Value"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordExplicitParametersOverrideDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "Some Value", new { value = "Another Value", name = "bar" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""password"" value=""Some Value"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithExplicitValueAndAttributesDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "DefaultFoo", new { baz = "BazValue" });

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithExplicitValueAndAttributesObject() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "DefaultFoo", new Dictionary<string, object> { { "baz", "BazValue" } });

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithExplicitValueNull() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", value: (string)null);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithImplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithImplicitValueAndAttributesDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", null, _attributesDictionary);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""password"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithImplicitValueAndAttributesObject() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", null, _attributesObject);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithNameAndValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.Password("foo", "fooValue");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""password"" value=""fooValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void PasswordWithNullNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act & Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Password(null), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.Password(String.Empty), "name");
        }

        //Input 
        [TestMethod]
        public void TextBoxDictionaryOverridesImplicitValues() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", new { type = "fooType" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxExplicitParametersOverrideDictionaryValues() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", new { value = "Some other value" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithDotReplacementForId() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo.bar.baz", null);

            // Assert
            Assert.AreEqual(@"<input id=""foo_bar_baz"" name=""foo.bar.baz"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act & Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.TextBox(null), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.TextBox(String.Empty), "name");

        }

        [TestMethod]
        public void TextBoxWithExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithExplicitValueAndAttributesDictionary() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", _attributesDictionary);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithExplicitValueAndAttributesObject() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", _attributesObject);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithExplicitValueNull() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextBox("foo", (string)null /* value */, (object)null);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithImplicitValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextBox("foo");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithImplicitValueAndAttributesDictionary() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextBox("foo", null, _attributesDictionary);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextBox("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [TestMethod]
        public void TextBoxWithImplicitValueAndAttributesObject() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.TextBox("foo", null, _attributesObject);

            // Assert
            Assert.AreEqual(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }


        [TestMethod]
        public void TextBoxWithNameAndValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.TextBox("foo", "fooValue");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooValue"" />", html.ToHtmlString());
        }
    }
}

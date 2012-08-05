using System;
using System.Collections.Generic;
using System.Web.WebPages;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class CheckBoxTest {

        [TestMethod]
        public void CheckboxWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act and assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.CheckBox(null), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.CheckBox(String.Empty), "name");
        }

        [TestMethod]
        public void CheckboxWithDefaultArguments() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckboxWithObjectAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", new { attr = "attr-value" });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckboxWithDictionaryAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", new Dictionary<string, object> { { "attr", "attr-value" } });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckboxWithExplicitChecked() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckboxWithModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", true);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckboxWithNonBooleanModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", Boolean.TrueString);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());


            modelState.SetModelValue("foo", new object());
            helper = new HtmlHelper(modelState);

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => helper.CheckBox("foo"), 
                "The parameter conversion from type \"System.Object\" to type \"System.Boolean\" failed because no " +
                "type converter can convert between these types.");
        }

        [TestMethod]
        public void CheckboxWithModelAndExplictValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());


            modelState.SetModelValue("foo", true);

            // Act
            html = helper.CheckBox("foo", false);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxWithCheckedHtmlAttribute() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", new { @checked = "checked" });

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxWithExplicitCheckedOverwritesHtmlAttribute() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", false, new { @checked = "checked" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxWithModelStateCheckedOverwritesHtmlAttribute() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", false, new { @checked = "checked" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxWithError() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxWithErrorAndCustomCss() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo", true, new { @class = "my-class" });

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" class=""field-validation-error my-class"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        //[TestMethod]
        // Can't test as it sets a static property
        // Review: Need to redo test once we fix set once property
        public void CheckBoxUsesCustomErrorClass() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            HtmlHelper.ValidationInputCssClassName = "my-error-class";
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.CheckBox("foo", true, new { @class = "my-class" });

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" class=""my-error-class my-class"" id=""foo"" name=""foo"" type=""checkbox"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void CheckBoxOverwritesImplicitAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.CheckBox("foo", true, new { type = "fooType", name = "bar" });

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""fooType"" />",
                html.ToHtmlString());
        }
    }
}


using System.Collections.Generic;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class RadioButtonTest {
        [TestMethod]
        public void RadioButtonWithEmptyNameThrows() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act and assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.RadioButton(null, null), "name");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.RadioButton(String.Empty, null), "name");
        }

        [TestMethod]
        public void RadioButtonWithDefaultArguments() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.RadioButton("foo", "bar", true);

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());

            html = helper.RadioButton("foo", "bar", false);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithObjectAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.RadioButton("foo", "bar", new { attr = "attr-value" });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithDictionaryAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.RadioButton("foo", "bar", new Dictionary<string, object> { { "attr", "attr-value" } });

            // Assert
            Assert.AreEqual(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonUsesModelStateToAssignChecked() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar");

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonUsesModelStateToRemoveChecked() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "not-a-bar");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar", new { @checked = "checked" } );

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithoutModelStateDoesNotAffectChecked() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.RadioButton("foo", "bar", new { @checked = "checked" });

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithNonStringModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new List<double>());
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar");

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithNonStringValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.RadioButton("foo", 2.53);

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""radio"" value=""2.53"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonWithExplicitChecked() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.RadioButton("foo", "not-bar", true);

            // Assert
            Assert.AreEqual(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""not-bar"" />",
                html.ToHtmlString());
        }

        [TestMethod]
        public void RadioButtonOverwritesImplicitAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.RadioButton("foo", "foo-value", new { value="bazValue", type = "fooType", name = "bar" });

            // Assert
            Assert.AreEqual(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""foo-value"" />",
                html.ToHtmlString());
        }
    }
}

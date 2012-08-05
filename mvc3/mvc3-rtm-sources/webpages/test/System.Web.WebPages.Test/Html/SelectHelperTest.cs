using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Html;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class SelectExtensionsTest {

        [TestMethod]
        public void DropDownListThrowsWithNoName() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act and assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.DropDownList(name: null, selectList: null), "name");
        }

        [TestMethod]
        public void DropDownListWithNoSelectedItem() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }


        [TestMethod]
        public void DropDownListWithDefaultOption() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", "select-one", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo"" name=""foo"">
<option value="""">select-one</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownListWithAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { attr = "attr-val", attr2 = "attr-val2" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" attr2=""attr-val2"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownListWithExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), "B", new Dictionary<string, object>{ {"attr",  "attr-val"} });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownWithModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { attr = "attr-val" } );

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownWithExplictAndModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), "B", new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownWithNonStringModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", 23);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void DropDownWithNonStringExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), new List<int>(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void DropDownWithErrors() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void DropDownListWithErrorsAndCustomClass() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error my-class"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownListWithEmptyOptionLabel() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error my-class"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownListWithObjectDictionaryAndTitle() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo", "Select One", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""my-class"" id=""foo"" name=""foo"">
<option value="""">Select One</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void DropDownListWithDotReplacementForId() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.DropDownList("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo_bar"" name=""foo.bar"">
<option value="""">Select One</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        // ListBox

        [TestMethod]
        public void ListBoxThrowsWithNoName() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act and assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => helper.ListBox(name: null, selectList: null), "name");
        }

        [TestMethod]
        public void ListBoxWithNoSelectedItem() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }


        [TestMethod]
        public void ListBoxWithDefaultOption() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", "select-one", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo"" name=""foo"">
<option value="""">select-one</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithAttributes() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val", attr2 = "attr-val2" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" attr2=""attr-val2"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), "B", new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithExplicitMultipleValuesAndNoMultiple() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new [] {"B", "C"}, new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithExplicitMultipleValuesAndMultiple() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new[] { "B", "C" }, 4, true);

            // Assert
            Assert.AreEqual(
                @"<select id=""foo"" multiple=""multiple"" name=""foo"" size=""4"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithMultipleModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new [] { "A", "C" });
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option selected=""selected"" value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithModelValueAndExplicitSelectItem() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new[] { "C", "D" });
            HtmlHelper helper = new HtmlHelper(modelState);
            var selectList = GetSelectList().ToList();
            selectList[1].Selected = true;

            // Act
            var html = helper.ListBox("foo", selectList, new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithMultiSelectAndMultipleModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new[] { "A", "C" });
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), null, 4, true);

            // Assert
            Assert.AreEqual(
    @"<select id=""foo"" multiple=""multiple"" name=""foo"" size=""4"">
<option selected=""selected"" value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithMultiSelectAndMultipleExplicitValues() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new[] { "A", "C" }, 4, true);

            // Assert
            Assert.AreEqual(
    @"<select id=""foo"" multiple=""multiple"" name=""foo"" size=""4"">
<option selected=""selected"" value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithMultiSelectAndExplitSelectValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());
            var selectList = GetSelectList().ToList();
            selectList.First().Selected = selectList.Last().Selected = true;

            // Act
            var html = helper.ListBox("foo", selectList, new[] { "B" }, 4, true);

            // Assert
            Assert.AreEqual(
    @"<select id=""foo"" multiple=""multiple"" name=""foo"" size=""4"">
<option selected=""selected"" value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option selected=""selected"" value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithExplictAndModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", defaultOption: null, selectList: GetSelectList(), 
                selectedValues: "B", htmlAttributes: new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option selected=""selected"" value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithErrorAndExplictAndModelState() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            modelState.AddError("foo", "test");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo_bar"" name=""foo.bar"">
<option value="""">Select One</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithNonStringModelValue() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", 23);
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void ListBoxWithNonStringExplicitValue() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new List<int>(), new { attr = "attr-val" });

            // Assert
            Assert.AreEqual(
                @"<select attr=""attr-val"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void ListBoxWithErrors() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());

        }

        [TestMethod]
        public void ListBoxWithErrorsAndCustomClass() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error my-class"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithEmptyOptionLabel() {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = new HtmlHelper(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""field-validation-error my-class"" id=""foo"" name=""foo"">
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithObjectDictionaryAndTitle() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo", "Select One", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.AreEqual(
                @"<select class=""my-class"" id=""foo"" name=""foo"">
<option value="""">Select One</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        [TestMethod]
        public void ListBoxWithDotReplacementForId() {
            // Arrange
            HtmlHelper helper = new HtmlHelper(new ModelStateDictionary());

            // Act
            var html = helper.ListBox("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.AreEqual(
                @"<select id=""foo_bar"" name=""foo.bar"">
<option value="""">Select One</option>
<option value=""A"">Alpha</option>
<option value=""B"">Bravo</option>
<option value=""C"">Charlie</option>
</select>",
                html.ToHtmlString());
        }

        private static IEnumerable<SelectListItem> GetSelectList() {
            yield return new SelectListItem() { Text = "Alpha", Value = "A"};
            yield return new SelectListItem() { Text = "Bravo", Value = "B"};
            yield return new SelectListItem() { Text = "Charlie", Value = "C" };
        }

    }
}

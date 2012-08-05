namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class DataAnnotationsModelMetadataProviderTestBase {
        protected abstract AssociatedMetadataProvider MakeProvider();

        [TestMethod]
        public void GetMetadataForPropertiesSetTypesAndPropertyNames() {
            // Arrange
            var provider = MakeProvider();

            // Act
            IEnumerable<ModelMetadata> result = provider.GetMetadataForProperties("foo", typeof(string));

            // Assert
            Assert.IsTrue(result.Any(m => m.ModelType == typeof(int)
                                          && m.PropertyName == "Length"
                                          && (int)m.Model == 3));
        }

        [TestMethod]
        public void GetMetadataForPropertySetsTypeAndPropertyName() {
            // Arrange
            var provider = MakeProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(string), "Length");

            // Assert
            Assert.AreEqual(typeof(int), result.ModelType);
            Assert.AreEqual("Length", result.PropertyName);
        }

        [TestMethod]
        public void GetMetadataForTypeSetsTypeWithNullPropertyName() {
            // Arrange
            var provider = MakeProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(string));

            // Assert
            Assert.AreEqual(typeof(string), result.ModelType);
            Assert.IsNull(result.PropertyName);
        }

        // [HiddenInput] tests

        class HiddenModel {
            public int NoAttribute { get; set; }

            [HiddenInput]
            public int DefaultHidden { get; set; }

            [HiddenInput(DisplayValue = false)]
            public int HiddenWithDisplayValueFalse { get; set; }

            [HiddenInput]
            [UIHint("CustomUIHint")]
            public int HiddenAndUIHint { get; set; }
        }

        [TestMethod]
        public void HiddenAttributeSetsTemplateHintAndHideSurroundingHtml() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            ModelMetadata noAttributeMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "NoAttribute");
            Assert.IsNull(noAttributeMetadata.TemplateHint);
            Assert.IsFalse(noAttributeMetadata.HideSurroundingHtml);

            ModelMetadata defaultHiddenMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "DefaultHidden");
            Assert.AreEqual("HiddenInput", defaultHiddenMetadata.TemplateHint);
            Assert.IsFalse(defaultHiddenMetadata.HideSurroundingHtml);

            ModelMetadata hiddenWithDisplayValueFalseMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "HiddenWithDisplayValueFalse");
            Assert.AreEqual("HiddenInput", hiddenWithDisplayValueFalseMetadata.TemplateHint);
            Assert.IsTrue(hiddenWithDisplayValueFalseMetadata.HideSurroundingHtml);

            // [UIHint] overrides the template hint from [Hidden]
            Assert.AreEqual("CustomUIHint", provider.GetMetadataForProperty(null, typeof(HiddenModel), "HiddenAndUIHint").TemplateHint);
        }

        // [UIHint] tests

        class UIHintModel {
            public int NoAttribute { get; set; }

            [UIHint("MyCustomTemplate")]
            public int DefaultUIHint { get; set; }

            [UIHint("MyMvcTemplate", "MVC")]
            public int MvcUIHint { get; set; }

            [UIHint("MyWebFormsTemplate", "WebForms")]
            public int NoMvcUIHint { get; set; }

            [UIHint("MyDefaultTemplate")]
            [UIHint("MyWebFormsTemplate", "WebForms")]
            [UIHint("MyMvcTemplate", "MVC")]
            public int MultipleUIHint { get; set; }
        }

        [TestMethod]
        public void UIHintAttributeSetsTemplateHint() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(UIHintModel), "NoAttribute").TemplateHint);
            Assert.AreEqual("MyCustomTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "DefaultUIHint").TemplateHint);
            Assert.AreEqual("MyMvcTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "MvcUIHint").TemplateHint);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(UIHintModel), "NoMvcUIHint").TemplateHint);

            Assert.AreEqual("MyMvcTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "MultipleUIHint").TemplateHint);
        }

        // [DataType] tests

        class DataTypeModel {
            public int NoAttribute { get; set; }

            [DataType(DataType.EmailAddress)]
            public int EmailAddressProperty { get; set; }

            [DataType("CustomDataType")]
            public int CustomDataTypeProperty { get; set; }
        }

        [TestMethod]
        public void DataTypeAttributeSetsDataTypeName() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DataTypeModel), "NoAttribute").DataTypeName);
            Assert.AreEqual("EmailAddress", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "EmailAddressProperty").DataTypeName);
            Assert.AreEqual("CustomDataType", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "CustomDataTypeProperty").DataTypeName);
        }

        // [ReadOnly] & [Editable] tests

        class ReadOnlyModel {
            public int NoAttributes { get; set; }

            [ReadOnly(true)]
            public int ReadOnlyAttribute { get; set; }

            [Editable(false)]
            public int EditableAttribute { get; set; }

            [ReadOnly(true)]
            [Editable(true)]
            public int BothAttributes { get; set; }    // Editable trumps ReadOnly
        }

        [TestMethod]
        public void ReadOnlyTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "NoAttributes").IsReadOnly);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "ReadOnlyAttribute").IsReadOnly);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "EditableAttribute").IsReadOnly);
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "BothAttributes").IsReadOnly);
        }

        // [DisplayFormat] tests

        class DisplayFormatModel {
            public int NoAttribute { get; set; }

            [DisplayFormat(NullDisplayText = "(null value)")]
            public int NullDisplayText { get; set; }

            [DisplayFormat(DataFormatString = "Data {0} format")]
            public int DisplayFormatString { get; set; }

            [DisplayFormat(DataFormatString = "Data {0} format", ApplyFormatInEditMode = true)]
            public int DisplayAndEditFormatString { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = true)]
            public int ConvertEmptyStringToNullTrue { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public int ConvertEmptyStringToNullFalse { get; set; }

            [DataType(DataType.Currency)]
            public int DataTypeWithoutDisplayFormatOverride { get; set; }

            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "format override")]
            public int DataTypeWithDisplayFormatOverride { get; set; }

            [DisplayFormat(HtmlEncode = true)]
            public int HtmlEncodeTrue { get; set; }

            [DisplayFormat(HtmlEncode = false)]
            public int HtmlEncodeFalse { get; set; }

            [DataType(DataType.Currency)]
            [DisplayFormat(HtmlEncode = false)]
            public int HtmlEncodeFalseWithDataType { get; set; }    // DataType trumps DisplayFormat.HtmlEncode
        }

        [TestMethod]
        public void DisplayFormatAttributetSetsNullDisplayText() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").NullDisplayText);
            Assert.AreEqual("(null value)", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NullDisplayText").NullDisplayText);
        }

        [TestMethod]
        public void DisplayFormatAttributeSetsDisplayFormatString() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").DisplayFormatString);
            Assert.AreEqual("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayFormatString").DisplayFormatString);
            Assert.AreEqual("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayAndEditFormatString").DisplayFormatString);
        }

        [TestMethod]
        public void DisplayFormatAttributeSetEditFormatString() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").EditFormatString);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayFormatString").EditFormatString);
            Assert.AreEqual("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayAndEditFormatString").EditFormatString);
        }

        [TestMethod]
        public void DisplayFormatAttributeSetsConvertEmptyStringToNull() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").ConvertEmptyStringToNull);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "ConvertEmptyStringToNullTrue").ConvertEmptyStringToNull);
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "ConvertEmptyStringToNullFalse").ConvertEmptyStringToNull);
        }

        [TestMethod]
        public void DataTypeWithoutDisplayFormatOverrideUsesDataTypesDisplayFormat() {
            // Arrange
            var provider = MakeProvider();

            // Act
            string result = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DataTypeWithoutDisplayFormatOverride").DisplayFormatString;

            // Assert
            Assert.AreEqual("{0:C}", result);    // Currency's default format string
        }

        [TestMethod]
        public void DataTypeWithDisplayFormatOverrideUsesDisplayFormatOverride() {
            // Arrange
            var provider = MakeProvider();

            // Act
            string result = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DataTypeWithDisplayFormatOverride").DisplayFormatString;

            // Assert
            Assert.AreEqual("format override", result);
        }

        [TestMethod]
        public void DataTypeInfluencedByDisplayFormatAttributeHtmlEncode() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").DataTypeName);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeTrue").DataTypeName);
            Assert.AreEqual("Html", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeFalse").DataTypeName);
            Assert.AreEqual("Currency", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeFalseWithDataType").DataTypeName);
        }

        // [ScaffoldColumn] tests

        class ScaffoldColumnModel {
            public int NoAttribute { get; set; }

            [ScaffoldColumn(true)]
            public int ScaffoldColumnTrue { get; set; }

            [ScaffoldColumn(false)]
            public int ScaffoldColumnFalse { get; set; }
        }

        [TestMethod]
        public void ScaffoldColumnAttributeSetsShowForDisplay() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "NoAttribute").ShowForDisplay);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnTrue").ShowForDisplay);
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnFalse").ShowForDisplay);
        }

        [TestMethod]
        public void ScaffoldColumnAttributeSetsShowForEdit() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "NoAttribute").ShowForEdit);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnTrue").ShowForEdit);
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnFalse").ShowForEdit);
        }

        // [DisplayColumn] tests

        [DisplayColumn("NoPropertyWithThisName")]
        class UnknownDisplayColumnModel { }

        [TestMethod]
        public void SimpleDisplayNameWithUnknownDisplayColumnThrows() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => provider.GetMetadataForType(() => new UnknownDisplayColumnModel(), typeof(UnknownDisplayColumnModel)).SimpleDisplayText,
                typeof(UnknownDisplayColumnModel).FullName + " has a DisplayColumn attribute for NoPropertyWithThisName, but property NoPropertyWithThisName does not exist.");
        }

        [DisplayColumn("WriteOnlyProperty")]
        class WriteOnlyDisplayColumnModel {
            public int WriteOnlyProperty { set { } }
        }

        [DisplayColumn("PrivateReadPublicWriteProperty")]
        class PrivateReadPublicWriteDisplayColumnModel {
            public int PrivateReadPublicWriteProperty { private get; set; }
        }

        [TestMethod]
        public void SimpleDisplayTextForTypeWithWriteOnlyDisplayColumnThrows() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => provider.GetMetadataForType(() => new WriteOnlyDisplayColumnModel(), typeof(WriteOnlyDisplayColumnModel)).SimpleDisplayText,
                typeof(WriteOnlyDisplayColumnModel).FullName + " has a DisplayColumn attribute for WriteOnlyProperty, but property WriteOnlyProperty does not have a public getter.");

            ExceptionHelper.ExpectInvalidOperationException(
                () => provider.GetMetadataForType(() => new PrivateReadPublicWriteDisplayColumnModel(), typeof(PrivateReadPublicWriteDisplayColumnModel)).SimpleDisplayText,
                typeof(PrivateReadPublicWriteDisplayColumnModel).FullName + " has a DisplayColumn attribute for PrivateReadPublicWriteProperty, but property PrivateReadPublicWriteProperty does not have a public getter.");
        }

        [DisplayColumn("DisplayColumnProperty")]
        class SimpleDisplayTextAttributeModel {
            public int FirstProperty { get { return 42; } }

            [ScaffoldColumn(false)]
            public string DisplayColumnProperty { get; set; }
        }

        class SimpleDisplayTextAttributeModelContainer {
            [DisplayFormat(NullDisplayText = "This is the null display text")]
            public SimpleDisplayTextAttributeModel Inner { get; set; }
        }

        [TestMethod]
        public void SimpleDisplayTextForNonNullClassWithNonNullDisplayColumnValue() {
            // Arrange
            string expected = "Custom property display value";
            var provider = MakeProvider();
            var model = new SimpleDisplayTextAttributeModel { DisplayColumnProperty = expected };
            var metadata = provider.GetMetadataForType(() => model, typeof(SimpleDisplayTextAttributeModel));

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SimpleDisplayTextForNullClassRevertsToDefaultBehavior() {
            // Arrange
            var provider = MakeProvider();
            var metadata = provider.GetMetadataForProperty(null, typeof(SimpleDisplayTextAttributeModelContainer), "Inner");

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual("This is the null display text", result);
        }

        [TestMethod]
        public void SimpleDisplayTextForNonNullClassWithNullDisplayColumnValueRevertsToDefaultBehavior() {
            // Arrange
            var provider = MakeProvider();
            var model = new SimpleDisplayTextAttributeModel();
            var metadata = provider.GetMetadataForType(() => model, typeof(SimpleDisplayTextAttributeModel));

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual("42", result);    // Falls back to the default logic of first property value
        }

        // [Required] tests

        class IsRequiredModel {
            public int NonNullableWithout { get; set; }

            public string NullableWithout { get; set; }

            [Required]
            public string NullableWith { get; set; }
        }

        [TestMethod]
        public void IsRequiredTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NonNullableWithout").IsRequired);
            Assert.IsFalse(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NullableWithout").IsRequired);
            Assert.IsTrue(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NullableWith").IsRequired);
        }

        // [Display] & [DisplayName] tests

        class DisplayModel {
            public int NoAttribute { get; set; }

            // Description

            [Display]
            public int DescriptionNotSet { get; set; }

            [Display(Description = "Description text")]
            public int DescriptionSet { get; set; }

            // DisplayName

            [DisplayName("Value from DisplayName")]
            public int DisplayNameAttributeNoDisplayAttribute { get; set; }

            [Display]
            public int DisplayAttributeNameNotSet { get; set; }

            [Display(Name = "Non empty name")]
            public int DisplayAttributeNonEmptyName { get; set; }

            [Display]
            [DisplayName("Value from DisplayName")]
            public int BothAttributesNameNotSet { get; set; }

            [Display(Name = "Value from Display")]
            [DisplayName("Value from DisplayName")]
            public int BothAttributes { get; set; }    // Display trumps DisplayName

            // Order

            [Display]
            public int OrderNotSet { get; set; }

            [Display(Order = 2112)]
            public int OrderSet { get; set; }

            // ShortDisplayName

            [Display]
            public int ShortNameNotSet { get; set; }

            [Display(ShortName = "Short name")]
            public int ShortNameSet { get; set; }

            // Watermark

            [Display]
            public int PromptNotSet { get; set; }

            [Display(Prompt = "Enter stuff here")]
            public int PromptSet { get; set; }
        }

        [TestMethod]
        public void DescriptionTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Description);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionNotSet").Description);
            Assert.AreEqual("Description text", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionSet").Description);
        }

        [TestMethod]
        public void DisplayNameTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").DisplayName);
            Assert.AreEqual("Value from DisplayName", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayNameAttributeNoDisplayAttribute").DisplayName);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayAttributeNameNotSet").DisplayName);
            Assert.AreEqual("Non empty name", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayAttributeNonEmptyName").DisplayName);
            Assert.AreEqual("Value from DisplayName", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothAttributesNameNotSet").DisplayName);
            Assert.AreEqual("Value from Display", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothAttributes").DisplayName);
        }

        [TestMethod]
        public void OrderTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.AreEqual(10000, provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Order);
            Assert.AreEqual(10000, provider.GetMetadataForProperty(null, typeof(DisplayModel), "OrderNotSet").Order);
            Assert.AreEqual(2112, provider.GetMetadataForProperty(null, typeof(DisplayModel), "OrderSet").Order);
        }

        [TestMethod]
        public void ShortDisplayNameTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").ShortDisplayName);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "ShortNameNotSet").ShortDisplayName);
            Assert.AreEqual("Short name", provider.GetMetadataForProperty(null, typeof(DisplayModel), "ShortNameSet").ShortDisplayName);
        }

        [TestMethod]
        public void WatermarkTests() {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Watermark);
            Assert.IsNull(provider.GetMetadataForProperty(null, typeof(DisplayModel), "PromptNotSet").Watermark);
            Assert.AreEqual("Enter stuff here", provider.GetMetadataForProperty(null, typeof(DisplayModel), "PromptSet").Watermark);
        }
    }
}

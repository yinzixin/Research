namespace Microsoft.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Web.Resources;

    public class CachedDataAnnotationsModelMetadata : CachedModelMetadata<CachedDataAnnotationsMetadataAttributes> {
        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
            : base(prototype, modelAccessor) {
        }

        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, IEnumerable<Attribute> attributes)
            : base(provider, containerType, modelType, propertyName, new CachedDataAnnotationsMetadataAttributes(attributes.ToArray())) {
        }

        protected override bool ComputeConvertEmptyStringToNull() {
            return PrototypeCache.DisplayFormat != null
                 ? PrototypeCache.DisplayFormat.ConvertEmptyStringToNull
                 : base.ComputeConvertEmptyStringToNull();
        }

        protected override string ComputeDataTypeName() {
            if (PrototypeCache.DataType != null) {
                return PrototypeCache.DataType.ToDataTypeName();
            }

            if (PrototypeCache.DisplayFormat != null && !PrototypeCache.DisplayFormat.HtmlEncode) {
                return DataTypeUtil.HtmlTypeName;
            }

            return base.ComputeDataTypeName();
        }

        protected override string ComputeDescription() {
            return PrototypeCache.Display != null
                 ? PrototypeCache.Display.GetDescription()
                 : base.ComputeDescription();
        }

        protected override string ComputeDisplayFormatString() {
            return PrototypeCache.DisplayFormat != null
                 ? PrototypeCache.DisplayFormat.DataFormatString
                 : base.ComputeDisplayFormatString();
        }

        protected override string ComputeDisplayName() {
            string result = null;

            if (PrototypeCache.Display != null) {
                result = PrototypeCache.Display.GetName();
            }

            if (result == null && PrototypeCache.DisplayName != null) {
                result = PrototypeCache.DisplayName.DisplayName;
            }

            return result ?? base.ComputeDisplayName();
        }

        protected override string ComputeEditFormatString() {
            if (PrototypeCache.DisplayFormat != null && PrototypeCache.DisplayFormat.ApplyFormatInEditMode) {
                return PrototypeCache.DisplayFormat.DataFormatString;
            }

            return base.ComputeEditFormatString();
        }

        protected override bool ComputeHideSurroundingHtml() {
            return PrototypeCache.HiddenInput != null
                 ? !PrototypeCache.HiddenInput.DisplayValue
                 : base.ComputeHideSurroundingHtml();
        }

        protected override bool ComputeIsReadOnly() {
            if (PrototypeCache.Editable != null) {
                return !PrototypeCache.Editable.AllowEdit;
            }

            if (PrototypeCache.ReadOnly != null) {
                return PrototypeCache.ReadOnly.IsReadOnly;
            }

            return base.ComputeIsReadOnly();
        }

        protected override bool ComputeIsRequired() {
            return PrototypeCache.Required != null
                 ? true
                 : base.ComputeIsRequired();
        }

        protected override string ComputeNullDisplayText() {
            return PrototypeCache.DisplayFormat != null
                 ? PrototypeCache.DisplayFormat.NullDisplayText
                 : base.ComputeNullDisplayText();
        }

        protected override int ComputeOrder() {
            int? result = null;

            if (PrototypeCache.Display != null) {
                result = PrototypeCache.Display.GetOrder();
            }

            return result ?? base.ComputeOrder();
        }

        protected override string ComputeShortDisplayName() {
            return PrototypeCache.Display != null
                 ? PrototypeCache.Display.GetShortName()
                 : base.ComputeShortDisplayName();
        }

        protected override bool ComputeShowForDisplay() {
            return PrototypeCache.ScaffoldColumn != null
                 ? PrototypeCache.ScaffoldColumn.Scaffold
                 : base.ComputeShowForDisplay();
        }

        protected override bool ComputeShowForEdit() {
            return PrototypeCache.ScaffoldColumn != null
                 ? PrototypeCache.ScaffoldColumn.Scaffold
                 : base.ComputeShowForEdit();
        }

        protected override string ComputeSimpleDisplayText() {
            if (Model != null) {
                if (PrototypeCache.DisplayColumn != null && !String.IsNullOrEmpty(PrototypeCache.DisplayColumn.DisplayColumn)) {
                    PropertyInfo displayColumnProperty = ModelType.GetProperty(PrototypeCache.DisplayColumn.DisplayColumn, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                    ValidateDisplayColumnAttribute(PrototypeCache.DisplayColumn, displayColumnProperty, ModelType);

                    object simpleDisplayTextValue = displayColumnProperty.GetValue(Model, new object[0]);
                    if (simpleDisplayTextValue != null) {
                        return simpleDisplayTextValue.ToString();
                    }
                }
            }

            return base.ComputeSimpleDisplayText();
        }

        protected override string ComputeTemplateHint() {
            if (PrototypeCache.UIHint != null) {
                return PrototypeCache.UIHint.UIHint;
            }

            if (PrototypeCache.HiddenInput != null) {
                return "HiddenInput";
            }

            return base.ComputeTemplateHint();
        }

        protected override string ComputeWatermark() {
            return PrototypeCache.Display != null
                 ? PrototypeCache.Display.GetPrompt()
                 : base.ComputeWatermark();
        }

        private static void ValidateDisplayColumnAttribute(DisplayColumnAttribute displayColumnAttribute, PropertyInfo displayColumnProperty, Type modelType) {
            if (displayColumnProperty == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnknownProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn
                    )
                );
            }
            if (displayColumnProperty.GetGetMethod() == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnreadableProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn
                    )
                );
            }
        }
    }

    // This is an exact copy of the DataTypeUtil class in System.Web.Mvc.
    // This class can be removed once CachedDataAnnotationsModelMetadata gets moved into System.Web.Mvc
    internal static class DataTypeUtil {
        internal static readonly string CurrencyTypeName = DataType.Currency.ToString();
        internal static readonly string DateTypeName = DataType.Date.ToString();
        internal static readonly string DateTimeTypeName = DataType.DateTime.ToString();
        internal static readonly string DurationTypeName = DataType.Duration.ToString();
        internal static readonly string EmailAddressTypeName = DataType.EmailAddress.ToString();
        internal static readonly string HtmlTypeName = DataType.Html.ToString();
        internal static readonly string ImageUrlTypeName = DataType.ImageUrl.ToString();
        internal static readonly string MultiLineTextTypeName = DataType.MultilineText.ToString();
        internal static readonly string PasswordTypeName = DataType.Password.ToString();
        internal static readonly string PhoneNumberTypeName = DataType.PhoneNumber.ToString();
        internal static readonly string TextTypeName = DataType.Text.ToString();
        internal static readonly string TimeTypeName = DataType.Time.ToString();
        internal static readonly string UrlTypeName = DataType.Url.ToString();

        // This is a faster version of GetDataTypeName(). It internally calls ToString() on the enum
        // value, which can be quite slow because of value verification.
        internal static string ToDataTypeName(this DataTypeAttribute attribute, Func<DataTypeAttribute, Boolean> isDataType = null) {
            if (isDataType == null) {
                isDataType = t => t.GetType().Equals(typeof(DataTypeAttribute));
            }

            // GetDataTypeName is virtual, so this is only safe if they haven't derived from DataTypeAttribute.
            // However, if they derive from DataTypeAttribute, they can help their own perf by overriding GetDataTypeName
            // and returning an appropriate string without invoking the ToString() on the enum.
            if (isDataType(attribute)) {
                switch (attribute.DataType) {
                    case DataType.Currency:
                        return CurrencyTypeName;
                    case DataType.Date:
                        return DateTypeName;
                    case DataType.DateTime:
                        return DateTimeTypeName;
                    case DataType.Duration:
                        return DurationTypeName;
                    case DataType.EmailAddress:
                        return EmailAddressTypeName;
                    case DataType.Html:
                        return HtmlTypeName;
                    case DataType.ImageUrl:
                        return ImageUrlTypeName;
                    case DataType.MultilineText:
                        return MultiLineTextTypeName;
                    case DataType.Password:
                        return PasswordTypeName;
                    case DataType.PhoneNumber:
                        return PhoneNumberTypeName;
                    case DataType.Text:
                        return TextTypeName;
                    case DataType.Time:
                        return TimeTypeName;
                    case DataType.Url:
                        return UrlTypeName;
                }
            }

            return attribute.GetDataTypeName();
        }
    }
}

namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DataAnnotationsModelValidatorProviderTest {

        // Validation attribute adapter registration

        private class MyValidationAttribute : ValidationAttribute {
            public override bool IsValid(object value) {
                throw new NotImplementedException();
            }
        }

        private class MyValidationAttributeAdapter : DataAnnotationsModelValidator<MyValidationAttribute> {
            public MyValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, MyValidationAttribute attribute)
                : base(metadata, context, attribute) { }
        }

        private class MyValidationAttributeAdapterBadCtor : ModelValidator {
            public MyValidationAttributeAdapterBadCtor(ModelMetadata metadata, ControllerContext context)
                : base(metadata, context) { }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                throw new NotImplementedException();
            }
        }

        private class MyDefaultValidationAttributeAdapter : DataAnnotationsModelValidator {
            public MyDefaultValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
                : base(metadata, context, attribute) { }
        }

        [MyValidation]
        private class MyValidatedClass { }

        [TestMethod]
        public void RegisterAdapter() {
            var oldFactories = DataAnnotationsModelValidatorProvider.AttributeFactories;

            try {
                // Arrange
                DataAnnotationsModelValidatorProvider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();

                // Act
                DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapter));

                // Assert
                var type = DataAnnotationsModelValidatorProvider.AttributeFactories.Keys.Single();
                Assert.AreEqual(typeof(MyValidationAttribute), type);

                var factory = DataAnnotationsModelValidatorProvider.AttributeFactories.Values.Single();
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
                var context = new ControllerContext();
                var attribute = new MyValidationAttribute();
                var validator = factory(metadata, context, attribute);
                Assert.AreEqual(typeof(MyValidationAttributeAdapter), validator.GetType());
            }
            finally {
                DataAnnotationsModelValidatorProvider.AttributeFactories = oldFactories;
            }
        }

        [TestMethod]
        public void RegisterAdapterGuardClauses() {
            // Attribute type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(null, typeof(MyValidationAttributeAdapter)),
                "attributeType");

            // Adapter type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(object), typeof(MyValidationAttributeAdapter)),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: attributeType");

            // Adapter must derive from ModelValidator
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types System.Web.Mvc.ModelMetadata, System.Web.Mvc.ControllerContext, and System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttribute\r\nParameter name: adapterType");
        }

        [TestMethod]
        public void RegisterAdapterFactory() {
            var oldFactories = DataAnnotationsModelValidatorProvider.AttributeFactories;

            try {
                // Arrange
                DataAnnotationsModelValidatorProvider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();
                DataAnnotationsModelValidationFactory factory = delegate { return null; };

                // Act
                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(MyValidationAttribute), factory);

                // Assert
                var type = DataAnnotationsModelValidatorProvider.AttributeFactories.Keys.Single();
                Assert.AreEqual(typeof(MyValidationAttribute), type);
                Assert.AreSame(factory, DataAnnotationsModelValidatorProvider.AttributeFactories.Values.Single());
            }
            finally {
                DataAnnotationsModelValidatorProvider.AttributeFactories = oldFactories;
            }
        }

        [TestMethod]
        public void RegisterAdapterFactoryGuardClauses() {
            DataAnnotationsModelValidationFactory factory = (metadata, context, attribute) => null;

            // Attribute type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(null, factory),
                "attributeType");

            // Factory cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(MyValidationAttribute), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(object), factory),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: attributeType");
        }

        [TestMethod]
        public void RegisterDefaultAdapter() {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultAttributeFactory;

            try {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatedClass));
                var context = new ControllerContext();
                DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(MyDefaultValidationAttributeAdapter));

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.AreEqual(typeof(MyDefaultValidationAttributeAdapter), result.GetType());
            }
            finally {
                DataAnnotationsModelValidatorProvider.DefaultAttributeFactory = oldFactory;
            }
        }

        [TestMethod]
        public void RegisterDefaultAdapterGuardClauses() {
            // Adapter type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(MyValidationAttributeAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types System.Web.Mvc.ModelMetadata, System.Web.Mvc.ControllerContext, and System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: adapterType");
        }

        [TestMethod]
        public void RegisterDefaultAdapterFactory() {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultAttributeFactory;

            try {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatedClass));
                var context = new ControllerContext();
                ModelValidator validator = new Mock<ModelValidator>(metadata, context).Object;
                DataAnnotationsModelValidationFactory factory = delegate { return validator; };
                DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(factory);

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.AreSame(validator, result);
            }
            finally {
                DataAnnotationsModelValidatorProvider.DefaultAttributeFactory = oldFactory;
            }
        }

        [TestMethod]
        public void RegisterDefaultAdapterFactoryGuardClauses() {
            // Factory cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(null),
                "factory");
        }

        // IValidatableObject adapter registration

        private class MyValidatableAdapter : ModelValidator {
            public MyValidatableAdapter(ModelMetadata metadata, ControllerContext context)
                : base(metadata, context) { }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                throw new NotImplementedException();
            }
        }

        private class MyValidatableAdapterBadCtor : ModelValidator {
            public MyValidatableAdapterBadCtor(ModelMetadata metadata, ControllerContext context, int unused)
                : base(metadata, context) { }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                throw new NotImplementedException();
            }
        }

        private class MyValidatableClass : IValidatableObject {
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void RegisterValidatableObjectAdapter() {
            var oldFactories = DataAnnotationsModelValidatorProvider.ValidatableFactories;

            try {
                // Arrange
                DataAnnotationsModelValidatorProvider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
                IValidatableObject validatable = new Mock<IValidatableObject>().Object;

                // Act
                DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(validatable.GetType(), typeof(MyValidatableAdapter));

                // Assert
                var type = DataAnnotationsModelValidatorProvider.ValidatableFactories.Keys.Single();
                Assert.AreEqual(validatable.GetType(), type);

                var factory = DataAnnotationsModelValidatorProvider.ValidatableFactories.Values.Single();
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
                var context = new ControllerContext();
                var validator = factory(metadata, context);
                Assert.AreEqual(typeof(MyValidatableAdapter), validator.GetType());
            }
            finally {
                DataAnnotationsModelValidatorProvider.ValidatableFactories = oldFactories;
            }
        }

        [TestMethod]
        public void RegisterValidatableObjectAdapterGuardClauses() {
            // Attribute type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(null, typeof(MyValidatableAdapter)),
                "modelType");

            // Adapter type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(object), typeof(MyValidatableAdapter)),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.IValidatableObject\r\nParameter name: modelType");

            // Adapter must derive from ModelValidator
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(MyValidatableAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types System.Web.Mvc.ModelMetadata and System.Web.Mvc.ControllerContext.\r\nParameter name: adapterType");
        }

        [TestMethod]
        public void RegisterValidatableObjectAdapterFactory() {
            var oldFactories = DataAnnotationsModelValidatorProvider.ValidatableFactories;

            try {
                // Arrange
                DataAnnotationsModelValidatorProvider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
                DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return null; };

                // Act
                DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), factory);

                // Assert
                var type = DataAnnotationsModelValidatorProvider.ValidatableFactories.Keys.Single();
                Assert.AreEqual(typeof(MyValidatableClass), type);
                Assert.AreSame(factory, DataAnnotationsModelValidatorProvider.ValidatableFactories.Values.Single());
            }
            finally {
                DataAnnotationsModelValidatorProvider.ValidatableFactories = oldFactories;
            }
        }

        [TestMethod]
        public void RegisterValidatableObjectAdapterFactoryGuardClauses() {
            DataAnnotationsValidatableObjectAdapterFactory factory = (metadata, context) => null;

            // Attribute type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(null, factory),
                "modelType");

            // Factory cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(object), factory),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.IValidatableObject\r\nParameter name: modelType");
        }

        [TestMethod]
        public void RegisterDefaultValidatableObjectAdapter() {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultValidatableFactory;

            try {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatableClass));
                var context = new ControllerContext();
                DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapter));

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.AreEqual(typeof(MyValidatableAdapter), result.GetType());
            }
            finally {
                DataAnnotationsModelValidatorProvider.DefaultValidatableFactory = oldFactory;
            }
        }

        [TestMethod]
        public void RegisterDefaultValidatableObjectAdapterGuardClauses() {
            // Adapter type cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            ExceptionHelper.ExpectArgumentException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types System.Web.Mvc.ModelMetadata and System.Web.Mvc.ControllerContext.\r\nParameter name: adapterType");
        }

        [TestMethod]
        public void RegisterDefaultValidatableObjectAdapterFactory() {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultValidatableFactory;

            try {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatableClass));
                var context = new ControllerContext();
                ModelValidator validator = new Mock<ModelValidator>(metadata, context).Object;
                DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return validator; };
                DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(factory);

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.AreSame(validator, result);
            }
            finally {
                DataAnnotationsModelValidatorProvider.DefaultValidatableFactory = oldFactory;
            }
        }

        [TestMethod]
        public void RegisterDefaultValidatableObjectAdapterFactoryGuardClauses() {
            // Factory cannot be null
            ExceptionHelper.ExpectArgumentNullException(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(null),
                "factory");
        }

        // Pre-configured adapters

        [TestMethod]
        public void AdapterForRangeAttributeRegistered() {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
            var context = new ControllerContext();
            var adapters = DataAnnotationsModelValidatorProvider.AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == typeof(RangeAttribute)).Value;

            // Act
            var adapter = adapterFactory(metadata, context, new RangeAttribute(1, 100));

            // Assert
            Assert.IsInstanceOfType(adapter, typeof(RangeAttributeAdapter));
        }

        [TestMethod]
        public void AdapterForRegularExpressionAttributeRegistered() {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
            var context = new ControllerContext();
            var adapters = DataAnnotationsModelValidatorProvider.AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == typeof(RegularExpressionAttribute)).Value;

            // Act
            var adapter = adapterFactory(metadata, context, new RegularExpressionAttribute("abc"));

            // Assert
            Assert.IsInstanceOfType(adapter, typeof(RegularExpressionAttributeAdapter));
        }

        [TestMethod]
        public void AdapterForRequiredAttributeRegistered() {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
            var context = new ControllerContext();
            var adapters = DataAnnotationsModelValidatorProvider.AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == typeof(RequiredAttribute)).Value;

            // Act
            var adapter = adapterFactory(metadata, context, new RequiredAttribute());

            // Assert
            Assert.IsInstanceOfType(adapter, typeof(RequiredAttributeAdapter));
        }

        [TestMethod]
        public void AdapterForStringLengthAttributeRegistered() {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
            var context = new ControllerContext();
            var adapters = DataAnnotationsModelValidatorProvider.AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == typeof(StringLengthAttribute)).Value;

            // Act
            var adapter = adapterFactory(metadata, context, new StringLengthAttribute(6));

            // Assert
            Assert.IsInstanceOfType(adapter, typeof(StringLengthAttributeAdapter));
        }

        // Default adapter factory for unknown attribute type

        [TestMethod]
        public void UnknownValidationAttributeGetsDefaultAdapter() {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(DummyClassWithDummyValidationAttribute));

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            var validator = validators.Single();
            Assert.AreEqual(typeof(DataAnnotationsModelValidator), validator.GetType());
        }

        private class DummyValidationAttribute : ValidationAttribute { }

        [DummyValidation]
        private class DummyClassWithDummyValidationAttribute { }

        // Default IValidatableObject adapter factory

        [TestMethod]
        public void IValidatableObjectGetsAValidator() {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var mockValidatable = new Mock<IValidatableObject>();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, mockValidatable.Object.GetType());

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            Assert.AreEqual(1, validators.Count());
        }

        // Implicit [Required] attribute

        [TestMethod]
        public void ReferenceTypesDontGetImplicitRequiredAttribute() {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            Assert.AreEqual(0, validators.Count());
        }

        [TestMethod]
        public void NonNullableValueTypesGetImplicitRequiredAttribute() {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithoutAttribute");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule rule = validator.GetClientValidationRules().Single();
            Assert.AreEqual(typeof(ModelClientValidationRequiredRule), rule.GetType());
        }

        [TestMethod]
        public void NonNullableValueTypesWithExplicitRequiredAttributeDoesntGetImplictRequiredAttribute() {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithAttribute");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule rule = validator.GetClientValidationRules().Single();
            Assert.AreEqual(typeof(ModelClientValidationRequiredRule), rule.GetType());
            Assert.AreEqual("Custom Required Message", rule.ErrorMessage);
        }

        [TestMethod]
        public void NonNullableValueTypeDoesntGetImplicitRequiredAttributeWhenFlagIsOff() {
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            try {
                // Arrange
                var provider = new DataAnnotationsModelValidatorProvider();
                var context = new ControllerContext();
                var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithoutAttribute");

                // Act
                IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

                // Assert
                Assert.AreEqual(0, validators.Count());
            }
            finally {
                DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = true;
            }
        }

        private class DummyRequiredAttributeHelperClass {
            [Required(ErrorMessage = "Custom Required Message")]
            public int WithAttribute { get; set; }

            public int WithoutAttribute { get; set; }
        }

        // Integration with metadata system

        [TestMethod]
        public void DoesNotReadPropertyValue() {
            // Arrange
            ObservableModel model = new ObservableModel();
            ModelMetadata metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForProperty(() => model.TheProperty, typeof(ObservableModel), "TheProperty");
            ControllerContext controllerContext = new ControllerContext();

            // Act
            ModelValidator[] validators = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, controllerContext).ToArray();
            ModelValidationResult[] results = validators.SelectMany(o => o.Validate(model)).ToArray();

            // Assert
            Assert.AreEqual(0, validators.Length, "Provider shouldn't have returned any validators.");
            Assert.IsFalse(model.PropertyWasRead(), "Property should not have been read by provider or validator.");
        }

        private class ObservableModel {
            private bool _propertyWasRead;

            public string TheProperty {
                get {
                    _propertyWasRead = true;
                    return "Hello";
                }
            }

            public bool PropertyWasRead() {
                return _propertyWasRead;
            }
        }

        private class BaseModel {
            public virtual string MyProperty { get; set; }
        }

        private class DerivedModel : BaseModel {
            [StringLength(10)]
            public override string MyProperty {
                get {
                    return base.MyProperty;
                }
                set {
                    base.MyProperty = value;
                }
            }
        }

        [TestMethod]
        public void GetValidatorsReturnsClientValidatorForDerivedTypeAppliedAgainstBaseTypeViaFromLambdaExpression() { // Dev10 Bug #868619
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var viewdata = new ViewDataDictionary<DerivedModel>();
            var metadata = ModelMetadata.FromLambdaExpression(m => m.MyProperty, viewdata); // Bug is in FromLambdaExpression

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule clientRule = validator.GetClientValidationRules().Single();
            Assert.IsInstanceOfType(clientRule, typeof(ModelClientValidationStringLengthRule));
            ModelClientValidationStringLengthRule stringLengthRule = (ModelClientValidationStringLengthRule)clientRule;
            Assert.AreEqual(10, stringLengthRule.ValidationParameters["max"]);
        }
    }
}

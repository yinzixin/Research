namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ModelMetadataTest {

        // Guard clauses

        [TestMethod]
        public void NullProviderThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new ModelMetadata(null /* provider */, null /* containerType */, null /* model */, typeof(object), null /* propertyName */),
                "provider");
        }

        [TestMethod]
        public void NullTypeThrows() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new ModelMetadata(provider.Object, null /* containerType */, null /* model */, null /* modelType */, null /* propertyName */),
                "modelType");
        }

        // Constructor

        [TestMethod]
        public void DefaultValues() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(Exception), () => "model", typeof(string), "propertyName");

            // Assert
            Assert.AreEqual(typeof(Exception), metadata.ContainerType);
            Assert.IsTrue(metadata.ConvertEmptyStringToNull);
            Assert.IsNull(metadata.DataTypeName);
            Assert.IsNull(metadata.Description);
            Assert.IsNull(metadata.DisplayFormatString);
            Assert.IsNull(metadata.DisplayName);
            Assert.IsNull(metadata.EditFormatString);
            Assert.IsFalse(metadata.HideSurroundingHtml);
            Assert.AreEqual("model", metadata.Model);
            Assert.AreEqual(typeof(string), metadata.ModelType);
            Assert.IsNull(metadata.NullDisplayText);
            Assert.AreEqual(10000, metadata.Order);
            Assert.AreEqual("propertyName", metadata.PropertyName);
            Assert.IsFalse(metadata.IsReadOnly);
            Assert.IsTrue(metadata.RequestValidationEnabled);
            Assert.IsNull(metadata.ShortDisplayName);
            Assert.IsTrue(metadata.ShowForDisplay);
            Assert.IsTrue(metadata.ShowForEdit);
            Assert.IsNull(metadata.TemplateHint);
            Assert.IsNull(metadata.Watermark);
        }

        // IsComplexType

        struct IsComplexTypeModel { }

        [TestMethod]
        public void IsComplexTypeTests() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(Object), null).IsComplexType);
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsComplexType);
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsComplexType);
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsComplexType);
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsComplexType);
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(IsComplexTypeModel), null).IsComplexType);
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(Nullable<IsComplexTypeModel>), null).IsComplexType);
        }

        // IsNullableValueType

        [TestMethod]
        public void IsNullableValueTypeTests() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsNullableValueType);
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsNullableValueType);
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsNullableValueType);
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsNullableValueType);
        }

        // IsRequired

        [TestMethod]
        public void IsRequiredTests() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsRequired);         // Reference type not required
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsRequired);    // Interface not required
            Assert.IsFalse(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsRequired);  // Nullable value type not required
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsRequired);             // Value type required
            Assert.IsTrue(new ModelMetadata(provider.Object, null, null, typeof(DayOfWeek), null).IsRequired);       // Enum (implicit value type) is required
        }

        // Properties

        [TestMethod]
        public void PropertiesCallsProvider() {
            // Arrange
            Type modelType = typeof(string);
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                    .Returns(propertyMetadata)
                    .Verifiable();

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            CollectionAssert.AreEqual(propertyMetadata, result.ToArray());
            provider.Verify();
        }

        [TestMethod]
        public void PropertiesUsesRealModelTypeRatherThanPassedModelType() {
            // Arrange
            string model = "String Value";
            Expression<Func<object, object>> accessor = _ => model;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(accessor, new ViewDataDictionary<object>());

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.AreEqual("Length", result.Single().PropertyName);
        }

        [TestMethod]
        public void PropertiesAreSortedByOrder() {
            // Arrange
            Type modelType = typeof(string);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata> {
                new ModelMetadata(provider.Object, null, () => 1, typeof(int), null) { Order = 20 },
                new ModelMetadata(provider.Object, null, () => 2, typeof(int), null) { Order = 30 },
                new ModelMetadata(provider.Object, null, () => 3, typeof(int), null) { Order = 10 },
            };
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                    .Returns(propertyMetadata)
                    .Verifiable();

            // Act
            List<ModelMetadata> result = metadata.Properties.ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[0].Model);
            Assert.AreEqual(1, result[1].Model);
            Assert.AreEqual(2, result[2].Model);
        }

        [TestMethod]
        public void PropertiesListGetsResetWhenModelGetsReset() { // Dev10 Bug #923263
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();
            var metadata = new ModelMetadata(provider, null, () => new Class1(), typeof(Class1), null);

            // Act
            ModelMetadata[] originalProps = metadata.Properties.ToArray();
            metadata.Model = new Class2();
            ModelMetadata[] newProps = metadata.Properties.ToArray();

            // Assert
            Assert.AreEqual(1, originalProps.Length);
            Assert.AreEqual(typeof(string), originalProps[0].ModelType);
            Assert.AreEqual("Prop1", originalProps[0].PropertyName);
            Assert.AreEqual(1, newProps.Length);
            Assert.AreEqual(typeof(int), newProps[0].ModelType);
            Assert.AreEqual("Prop2", newProps[0].PropertyName);
        }

        class Class1 {
            public string Prop1 { get; set; }
        }

        class Class2 {
            public int Prop2 { get; set; }
        }

        // SimpleDisplayText

        [TestMethod]
        public void SimpleDisplayTextReturnsNullDisplayTextForNullModel() {
            // Arrange
            string nullText = "(null)";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null) { NullDisplayText = nullText };

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(nullText, result);
        }

       private class SimpleDisplayTextModelWithToString {
            public override string ToString() {
                return "Custom ToString Value";
            }
        }

        [TestMethod]
        public void SimpleDisplayTextReturnsToStringValueWhenOverridden() {
            // Arrange
            SimpleDisplayTextModelWithToString model = new SimpleDisplayTextModelWithToString();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(model.ToString(), result);
        }

       private class SimpleDisplayTextModelWithoutToString {
            public string FirstProperty { get; set; }

            public int SecondProperty { get; set; }
        }

        [TestMethod]
        public void SimpleDisplayTextReturnsFirstPropertyValueForNonNullModel() {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString {
                FirstProperty = "First Property Value"
            };
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(model.FirstProperty, result);
        }

        [TestMethod]
        public void SimpleDisplayTextReturnsFirstPropertyNullDisplayTextForNonNullModelWithNullDisplayColumnPropertyValue() {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString();
            EmptyModelMetadataProvider propertyProvider = new EmptyModelMetadataProvider();
            ModelMetadata propertyMetadata = propertyProvider.GetMetadataForProperty(() => model.FirstProperty, typeof(SimpleDisplayTextModelWithoutToString), "FirstProperty");
            propertyMetadata.NullDisplayText = "Null Display Text";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            provider.Setup(p => p.GetMetadataForProperties(model, typeof(SimpleDisplayTextModelWithoutToString)))
                    .Returns(new[] { propertyMetadata });
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(propertyMetadata.NullDisplayText, result);
        }

       private class SimpleDisplayTextModelWithNoProperties { }

        [TestMethod]
        public void SimpleDisplayTextReturnsEmptyStringForNonNullModelWithNoVisibleProperties() {
            // Arrange
            SimpleDisplayTextModelWithNoProperties model = new SimpleDisplayTextModelWithNoProperties();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithNoProperties), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(String.Empty, result);
        }

        private class ObjectWithToStringOverride {
            private string _toStringValue;

            public ObjectWithToStringOverride(string toStringValue) {
                _toStringValue = toStringValue;
            }

            public override string ToString() {
                return _toStringValue;
            }
        }

        [TestMethod]
        public void SimpleDisplayTextReturnsToStringOfModelForNonNullModel() {
            // Arrange
            string toStringText = "text from ToString()";
            ObjectWithToStringOverride model = new ObjectWithToStringOverride(toStringText);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(ObjectWithToStringOverride), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.AreEqual(toStringText, result);
        }

        // FromStringExpression()

        [TestMethod]
        public void FromStringExpressionGuardClauses() {
            // Null expression throws
            ExceptionHelper.ExpectArgumentNullException(
                () => ModelMetadata.FromStringExpression(null, new ViewDataDictionary()),
                "expression");

            // Null view data dictionary throws
            ExceptionHelper.ExpectArgumentNullException(
                () => ModelMetadata.FromStringExpression("expression", null),
                "viewData");
        }

        [TestMethod]
        public void FromStringExpressionEmptyExpressionReturnsExistingModelMetadata() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);
                ViewDataDictionary viewData = new ViewDataDictionary();
                viewData.ModelMetadata = metadata;

                // Act
                ModelMetadata result = ModelMetadata.FromStringExpression(String.Empty, viewData);

                // Assert
                Assert.AreSame(metadata, result);
            }
        }

        [TestMethod]
        public void FromStringExpressionItemNotFoundInViewData() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ViewDataDictionary viewData = new ViewDataDictionary();
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.IsNull(accessor);
                            Assert.AreEqual(typeof(string), type);    // Don't know the type, must fall back on string
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromStringExpression("UnknownObject", viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromStringExpressionNullItemFoundAtRootOfViewData() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ViewDataDictionary viewData = new ViewDataDictionary();
                viewData["Object"] = null;
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.IsNull(accessor());
                            Assert.AreEqual(typeof(string), type);    // Don't know the type, must fall back on string
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromStringExpression("Object", viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromStringExpressionNonNullItemFoundAtRootOfViewData() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                object model = new object();
                ViewDataDictionary viewData = new ViewDataDictionary();
                viewData["Object"] = model;
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreSame(model, accessor());
                            Assert.AreEqual(typeof(object), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromStringExpression("Object", viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromStringExpressionNullItemFoundOnPropertyOfItemInViewData() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyModelContainer model = new DummyModelContainer();
                ViewDataDictionary viewData = new ViewDataDictionary();
                viewData["Object"] = model;
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.IsNull(accessor());
                            Assert.AreEqual(typeof(DummyModelContainer), type);
                            Assert.AreEqual("Model", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromStringExpression("Object.Model", viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromStringExpressionNonNullItemFoundOnPropertyOfItemInViewData() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyModelContainer model = new DummyModelContainer { Model = new DummyContactModel() };
                ViewDataDictionary viewData = new ViewDataDictionary();
                viewData["Object"] = model;
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.AreSame(model.Model, accessor());
                            Assert.AreEqual(typeof(DummyModelContainer), type);
                            Assert.AreEqual("Model", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromStringExpression("Object.Model", viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromStringExpressionWithNullModelButValidModelMetadataShouldReturnProperPropertyMetadata() {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(DummyContactModel));

            // Act
            ModelMetadata result = ModelMetadata.FromStringExpression("NullableIntValue", viewData);

            // Assert
            Assert.IsNull(result.Model);
            Assert.AreEqual(typeof(Nullable<int>), result.ModelType);
            Assert.AreEqual("NullableIntValue", result.PropertyName);
            Assert.AreEqual(typeof(DummyContactModel), result.ContainerType);
        }

        [TestMethod]
        public void FromStringExpressionValueInModelProperty() {
            // Arrange
            DummyContactModel model = new DummyContactModel { FirstName = "John" };
            ViewDataDictionary viewData = new ViewDataDictionary(model);

            // Act
            ModelMetadata metadata = ModelMetadata.FromStringExpression("FirstName", viewData);

            // Assert
            Assert.AreEqual("John", metadata.Model);
        }

        [TestMethod]
        public void FromStringExpressionValueInViewDataOverridesValueFromModelProperty() {
            // Arrange
            DummyContactModel model = new DummyContactModel { FirstName = "John" };
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            viewData["FirstName"] = "Jim";

            // Act
            ModelMetadata metadata = ModelMetadata.FromStringExpression("FirstName", viewData);

            // Assert
            Assert.AreEqual("Jim", metadata.Model);
        }

        // FromLambdaExpression()

        [TestMethod]
        public void FromLambdaExpressionGuardClauseTests() {
            // Null expression throws
            ExceptionHelper.ExpectArgumentNullException(
                () => ModelMetadata.FromLambdaExpression<string, object>(null, new ViewDataDictionary<string>()),
                "expression");

            // Null view data throws
            ExceptionHelper.ExpectArgumentNullException(
                () => ModelMetadata.FromLambdaExpression<string, object>(m => m, null),
                "viewData");

            // Unsupported expression type throws
            ExceptionHelper.ExpectInvalidOperationException(
                () => ModelMetadata.FromLambdaExpression<string, object>(m => new Object(), new ViewDataDictionary<string>()),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [TestMethod]
        public void FromLambdaExpressionModelIdentityExpressionReturnsExistingModelMetadata() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);
                ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
                viewData.ModelMetadata = metadata;

                // Act
                ModelMetadata result = ModelMetadata.FromLambdaExpression<object, object>(m => m, viewData);

                // Assert
                Assert.AreSame(metadata, result);
            }
        }

        [TestMethod]
        public void FromLambdaExpressionPropertyExpressionFromParameter() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel { FirstName = "Test" };
                ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.AreEqual("Test", accessor());
                            Assert.AreEqual(typeof(DummyContactModel), type);
                            Assert.AreEqual("FirstName", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m.FirstName, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionPropertyExpressionFromClosureValue() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel { FirstName = "Test" };
                ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.AreEqual("Test", accessor());
                            Assert.AreEqual(typeof(DummyContactModel), type);
                            Assert.AreEqual("FirstName", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<object, string>(m => model.FirstName, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionFieldExpressionFromParameter() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel { IntField = 42 };
                ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreEqual(42, accessor());
                            Assert.AreEqual(typeof(int), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DummyContactModel, int>(m => m.IntField, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionFieldExpressionFromFieldOfClosureValue() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel { IntField = 42 };
                ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreEqual(42, accessor());
                            Assert.AreEqual(typeof(int), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<object, int>(m => model.IntField, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionFieldExpressionFromClosureValue() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel();
                ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreSame(model, accessor());
                            Assert.AreEqual(typeof(DummyContactModel), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<object, DummyContactModel>(m => model, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionSingleParameterClassIndexer() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel();
                ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreEqual("Indexed into 42", accessor());
                            Assert.AreEqual(typeof(string), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m[42], viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionSingleDimensionArrayIndex() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                DummyContactModel model = new DummyContactModel { Array = new[] { 4, 8, 15, 16, 23, 42 } };
                ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
                provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                        .Callback<Func<object>, Type>((accessor, type) =>
                        {
                            Assert.AreEqual(16, accessor());
                            Assert.AreEqual(typeof(int), type);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DummyContactModel, int>(m => m.Array[3], viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionNullReferenceExceptionsInPropertyExpressionPreserveAllExpressionInformation() {
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>();
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.IsNull(accessor());
                            Assert.AreEqual(typeof(DummyContactModel), type);
                            Assert.AreEqual("FirstName", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m.FirstName, viewData);

                // Assert
                provider.Verify();
            }
        }

        [TestMethod]
        public void FromLambdaExpressionSetsContainerTypeToDerivedMostType() { // Dev10 Bug #868619
            using (MockModelMetadataProvider provider = new MockModelMetadataProvider()) {
                // Arrange
                ViewDataDictionary<DerivedModel> viewData = new ViewDataDictionary<DerivedModel>();
                provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                        .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                        {
                            Assert.IsNull(accessor());
                            Assert.AreEqual(typeof(DerivedModel), type);
                            Assert.AreEqual("MyProperty", propertyName);
                        })
                        .Returns(() => null)
                        .Verifiable();

                // Act
                ModelMetadata.FromLambdaExpression<DerivedModel, string>(m => m.MyProperty, viewData);

                // Assert
                provider.Verify();
            }
        }

        private class BaseModel {
            public virtual string MyProperty { get; set; }
        }

        private class DerivedModel : BaseModel {
            [Required]
            public override string MyProperty {
                get {
                    return base.MyProperty;
                }
                set {
                    base.MyProperty = value;
                }
            }
        }

        // GetDisplayName()

        [TestMethod]
        public void ReturnsDisplayNameWhenSet() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName") { DisplayName = "Display Name" };

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.AreEqual("Display Name", result);
        }

        [TestMethod]
        public void ReturnsPropertyNameWhenSetAndDisplayNameIsNull() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName");

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.AreEqual("PropertyName", result);
        }

        [TestMethod]
        public void ReturnsTypeNameWhenPropertyNameAndDisplayNameAreNull() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.AreEqual("Object", result);
        }

        // Helpers

        private class MockModelMetadataProvider : Mock<ModelMetadataProvider>, IDisposable {
            ModelMetadataProvider original;

            public MockModelMetadataProvider() {
                original = ModelMetadataProviders.Current;
                ModelMetadataProviders.Current = this.Object;
            }

            public void Dispose() {
                ModelMetadataProviders.Current = original;
            }
        }

        private class DummyContactModel {
            public int IntField = 0;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> NullableIntValue { get; set; }
            public int[] Array { get; set; }
            public string this[int index] { get { return "Indexed into " + index; } }
        }

       private class DummyModelContainer {
            public DummyContactModel Model { get; set; }
        }

    }
}

namespace Microsoft.Web.Mvc.Test {
    using System;
    using System.Runtime.Serialization;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.Mvc;
    using Microsoft.Web.UnitTestUtil;
    using Moq;

    [TestClass]
    public class DeserializeAttributeTest {

        [TestMethod]
        public void BinderReturnsDeserializedValue() {
            // Arrange
            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Deserialize("some-value", SerializationMode.EncryptedAndSigned)).Returns(42);
            DeserializeAttribute attr = new DeserializeAttribute(SerializationMode.EncryptedAndSigned) { Serializer = mockSerializer.Object };

            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext() {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider() {
                    { "someKey", "some-value" }
                }
            };

            // Act
            object retVal = binder.BindModel(null, mbContext);

            // Assert
            Assert.AreEqual(42, retVal, "Object was not properly deserialized.");
        }

        [TestMethod]
        public void BinderReturnsNullIfValueProviderDoesNotContainKey() {
            // Arrange
            DeserializeAttribute attr = new DeserializeAttribute();
            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext() {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            object retVal = binder.BindModel(null, mbContext);

            // Assert
            Assert.IsNull(retVal, "Binder should return null if no data was present.");
        }

        [TestMethod]
        public void BinderThrowsIfBindingContextIsNull() {
            // Arrange
            DeserializeAttribute attr = new DeserializeAttribute();
            IModelBinder binder = attr.GetBinder();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    binder.BindModel(null, null);
                }, "bindingContext");
        }

        [TestMethod]
        public void BinderThrowsIfDataCorrupt() {
            // Arrange
            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Deserialize(It.IsAny<string>(), It.IsAny<SerializationMode>())).Throws(new SerializationException());
            DeserializeAttribute attr = new DeserializeAttribute() { Serializer = mockSerializer.Object };

            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext() {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider() {
                    { "someKey", "This data is corrupted." }
                }
            };

            // Act & assert
            Exception exception = ExceptionHelper.ExpectException<SerializationException>(
                delegate {
                    binder.BindModel(null, mbContext);
                });
        }

        [TestMethod]
        public void ModeDefaultsToSigned() {
            // Arrange
            DeserializeAttribute attr = new DeserializeAttribute();

            // Act
            SerializationMode defaultMode = attr.Mode;

            // Assert
            Assert.AreEqual(SerializationMode.Signed, defaultMode);
        }

    }
}

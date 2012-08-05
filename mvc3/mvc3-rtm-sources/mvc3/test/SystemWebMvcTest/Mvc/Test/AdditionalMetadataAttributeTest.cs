namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AdditionalMetadataAttributeTest {

        [TestMethod]
        public void GuardClauses() {
            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new AdditionalMetadataAttribute(null, new object()),
                "name");

            AdditionalMetadataAttribute attr = new AdditionalMetadataAttribute("key", null);
            ExceptionHelper.ExpectArgumentNullException(
                () => attr.OnMetadataCreated(null),
                "metadata");
        }

        [TestMethod]
        public void OnMetaDataCreatedSetsAdditionalValue() {
            // Arrange
            string name = "name";
            object value = new object();

            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), null);
            AdditionalMetadataAttribute attr = new AdditionalMetadataAttribute(name, value);

            // Act
            attr.OnMetadataCreated(modelMetadata);

            // Assert
            Assert.AreEqual(modelMetadata.AdditionalValues[name], value);
            Assert.AreEqual(attr.Name, name);
            Assert.AreEqual(attr.Value, value);
        }

        [TestMethod]
        public void MultipleAttributesCanSetValuesOnMetadata() {
            // Arrange
            string name1 = "name1";
            string name2 = "name2";

            object value1 = new object();
            object value2 = new object();
            object value3 = new object();

            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), null);
            AdditionalMetadataAttribute attr1 = new AdditionalMetadataAttribute(name1, value1);
            AdditionalMetadataAttribute attr2 = new AdditionalMetadataAttribute(name2, value2);
            AdditionalMetadataAttribute attr3 = new AdditionalMetadataAttribute(name1, value3);

            // Act
            attr1.OnMetadataCreated(modelMetadata);
            attr2.OnMetadataCreated(modelMetadata);
            attr3.OnMetadataCreated(modelMetadata);

            // Assert
            Assert.AreEqual(2, modelMetadata.AdditionalValues.Count);
            Assert.AreEqual(modelMetadata.AdditionalValues[name1], value3);
            Assert.AreEqual(modelMetadata.AdditionalValues[name2], value2);

            Assert.AreNotEqual(attr1.TypeId, attr2.TypeId);
            Assert.AreNotEqual(attr2.TypeId, attr3.TypeId);
            Assert.AreNotEqual(attr3.TypeId, attr1.TypeId);
        }
    }
}

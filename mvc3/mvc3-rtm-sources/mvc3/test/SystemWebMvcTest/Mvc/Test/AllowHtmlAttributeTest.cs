namespace System.Web.Mvc.Test {
    using System;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AllowHtmlAttributeTest {

        [TestMethod]
        public void OnMetadataCreated_ThrowsIfMetadataIsNull() {
            // Arrange
            AllowHtmlAttribute attr = new AllowHtmlAttribute();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    attr.OnMetadataCreated(null);
                }, "metadata");
        }

        [TestMethod]
        public void OnMetadataCreated() {
            // Arrange
            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), "SomeProperty");
            AllowHtmlAttribute attr = new AllowHtmlAttribute();

            // Act
            bool originalValue = modelMetadata.RequestValidationEnabled;
            attr.OnMetadataCreated(modelMetadata);
            bool newValue = modelMetadata.RequestValidationEnabled;

            // Assert
            Assert.IsTrue(originalValue, "RequestValidationEnabled should have defaulted to 'true'.");
            Assert.IsFalse(newValue, "RequestValidationEnabled should have been set to 'false' by this attribute.");
        }

    }
}

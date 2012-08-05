namespace Microsoft.Web.Mvc.Test {
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EmailAddressAttribueTest {
        [TestMethod]
        public void ClientRule() {
            // Arrange
            var attribute = new EmailAddressAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.AreEqual("email", clientRule.ValidationType);
            Assert.AreEqual("The PropertyName field is not a valid e-mail address.", clientRule.ErrorMessage);
            Assert.AreEqual(0, clientRule.ValidationParameters.Count);
        }

        [TestMethod]
        public void IsValidTests() {
            // Arrange
            var attribute = new EmailAddressAttribute();

            // Act & Assert
            Assert.IsTrue(attribute.IsValid(null)); // Optional values are always valid
            Assert.IsTrue(attribute.IsValid("joe@contoso.com"));
            Assert.IsTrue(attribute.IsValid("joe%fred@contoso.com"));
            Assert.IsFalse(attribute.IsValid("joe"));
            Assert.IsFalse(attribute.IsValid("joe@"));
            Assert.IsFalse(attribute.IsValid("joe@contoso"));
        }
    }
}

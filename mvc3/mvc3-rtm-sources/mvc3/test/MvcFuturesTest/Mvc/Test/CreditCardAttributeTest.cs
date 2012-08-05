namespace Microsoft.Web.Mvc.Test {
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CreditCardAttributeTest {
        [TestMethod]
        public void ClientRule() {
            // Arrange
            var attribute = new CreditCardAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.AreEqual("creditcard", clientRule.ValidationType);
            Assert.AreEqual("The PropertyName field is not a valid credit card number.", clientRule.ErrorMessage);
            Assert.AreEqual(0, clientRule.ValidationParameters.Count);
        }

        [TestMethod]
        public void IsValidTests() {
            // Arrange
            var attribute = new CreditCardAttribute();

            // Act & Assert
            Assert.IsTrue(attribute.IsValid(null));                  // Optional values are always valid
            Assert.IsTrue(attribute.IsValid("0000000000000000"));    // Simplest valid value
            Assert.IsTrue(attribute.IsValid("1234567890123452"));    // Good checksum
            Assert.IsTrue(attribute.IsValid("1234-5678-9012-3452")); // Good checksum, with dashes
            Assert.IsFalse(attribute.IsValid("0000000000000001"));   // Bad checksum
            Assert.IsFalse(attribute.IsValid(0));                    // Non-string
        }
    }
}

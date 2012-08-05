namespace Microsoft.Web.Mvc.Test {
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UrlAttributeTest {
        [TestMethod]
        public void ClientRule() {
            // Arrange
            var attribute = new UrlAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.AreEqual("url", clientRule.ValidationType);
            Assert.AreEqual("The PropertyName field is not a valid fully-qualified http, https, or ftp URL.", clientRule.ErrorMessage);
            Assert.AreEqual(0, clientRule.ValidationParameters.Count);
        }

        [TestMethod]
        public void IsValidTests() {
            // Arrange
            var attribute = new UrlAttribute();

            // Act & Assert
            Assert.IsTrue(attribute.IsValid(null));  // Optional values are always valid
            Assert.IsTrue(attribute.IsValid("http://foo.bar"));
            Assert.IsTrue(attribute.IsValid("https://foo.bar"));
            Assert.IsTrue(attribute.IsValid("ftp://foo.bar"));
            Assert.IsFalse(attribute.IsValid("file:///foo.bar"));
            Assert.IsFalse(attribute.IsValid("http://user%password@foo.bar/"));
            Assert.IsFalse(attribute.IsValid("foo.png"));
            Assert.IsFalse(attribute.IsValid("\0foo.png"));  // Illegal character
        }
    }
}

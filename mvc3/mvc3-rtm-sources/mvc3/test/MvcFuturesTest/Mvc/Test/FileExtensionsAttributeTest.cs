namespace Microsoft.Web.Mvc.Test {
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FileExtensionsAttributeTest {
        [TestMethod]
        public void DefaultExtensions() {
            Assert.AreEqual("png,jpg,jpeg,gif", new FileExtensionsAttribute().Extensions);
        }

        [TestMethod]
        public void ClientRule() {
            // Arrange
            var attribute = new FileExtensionsAttribute { Extensions = " FoO, .bar,baz " };
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.AreEqual("accept", clientRule.ValidationType);
            Assert.AreEqual("The PropertyName field only accepts files with the following extensions: .foo, .bar, .baz", clientRule.ErrorMessage);
            Assert.AreEqual(1, clientRule.ValidationParameters.Count);
            Assert.AreEqual("foo,bar,baz", clientRule.ValidationParameters["exts"]);
        }

        [TestMethod]
        public void IsValidTests() {
            // Arrange
            var attribute = new FileExtensionsAttribute();

            // Act & Assert
            Assert.IsTrue(attribute.IsValid(null));  // Optional values are always valid
            Assert.IsTrue(attribute.IsValid("foo.png"));
            Assert.IsTrue(attribute.IsValid("foo.jpeg"));
            Assert.IsTrue(attribute.IsValid("foo.jpg"));
            Assert.IsTrue(attribute.IsValid("foo.gif"));
            Assert.IsTrue(attribute.IsValid(@"C:\Foo\bar.jpg"));
            Assert.IsFalse(attribute.IsValid("foo"));
            Assert.IsFalse(attribute.IsValid("foo.png.pif"));
            Assert.IsFalse(attribute.IsValid(@"C:\foo.png\bar"));
            Assert.IsFalse(attribute.IsValid("\0foo.png"));  // Illegal character
        }
    }
}

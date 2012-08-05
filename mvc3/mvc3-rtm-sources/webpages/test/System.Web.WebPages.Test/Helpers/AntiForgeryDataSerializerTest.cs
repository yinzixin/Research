namespace System.Web.Helpers.Test {
    using System;
    using System.Web.Mvc;
    using System.Web.WebPages.TestUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AntiForgeryDataSerializerTest {
        [TestMethod]
        public void GuardClauses() {
            // Arrange
            AntiForgeryDataSerializer serializer = new AntiForgeryDataSerializer();

            // Act & assert
            ExceptionAssert.ThrowsArgNull(
                () => serializer.Serialize(null),
                "token"
            );
            ExceptionAssert.ThrowsArgNullOrEmpty(
                () => serializer.Deserialize(null),
                "serializedToken"
            );
            ExceptionAssert.ThrowsArgNullOrEmpty(
                () => serializer.Deserialize(String.Empty),
                "serializedToken"
            );
            ExceptionAssert.Throws<HttpAntiForgeryException>(
                () => serializer.Deserialize("Corrupted Base-64 Value"),
                "A required anti-forgery token was not supplied or was invalid."
            );
        }

        [TestMethod]
        public void CanRoundTripData() {
            // Arrange
            AntiForgeryDataSerializer serializer = new AntiForgeryDataSerializer {
                Decoder = value => Convert.FromBase64String(value),
                Encoder = bytes => Convert.ToBase64String(bytes),
            };
            AntiForgeryData input = new AntiForgeryData {
                Salt = "The Salt",
                Username = "The Username",
                Value = "The Value",
                CreationDate = DateTime.Now,
            };

            // Act
            AntiForgeryData output = serializer.Deserialize(serializer.Serialize(input));

            // Assert
            Assert.IsNotNull(output);
            Assert.AreEqual(input.Salt, output.Salt);
            Assert.AreEqual(input.Username, output.Username);
            Assert.AreEqual(input.Value, output.Value);
            Assert.AreEqual(input.CreationDate, output.CreationDate);
        }
    }
}

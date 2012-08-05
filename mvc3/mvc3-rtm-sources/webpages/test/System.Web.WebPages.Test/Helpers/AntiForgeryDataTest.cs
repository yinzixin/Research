namespace System.Web.Helpers.Test {
    using System;
    using System.Security.Principal;
    using System.Web.WebPages.TestUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AntiForgeryDataTest {

        [TestMethod]
        public void CopyConstructor() {
            // Arrange
            AntiForgeryData originalToken = new AntiForgeryData() {
                CreationDate = DateTime.Now,
                Salt = "some salt",
                Value = "some value"
            };

            // Act
            AntiForgeryData newToken = new AntiForgeryData(originalToken);

            // Assert
            Assert.AreEqual(originalToken.CreationDate, newToken.CreationDate);
            Assert.AreEqual(originalToken.Salt, newToken.Salt);
            Assert.AreEqual(originalToken.Value, newToken.Value);
        }

        [TestMethod]
        public void CopyConstructorThrowsIfTokenIsNull() {
            // Act & Assert
            ExceptionAssert.ThrowsArgNull(
                delegate {
                    new AntiForgeryData(null);
                }, "token");
        }

        [TestMethod]
        public void CreationDateProperty() {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            var now = DateTime.UtcNow;
            token.CreationDate = now;
            Assert.AreEqual(now, token.CreationDate);
        }

        [TestMethod]
        public void GetAntiForgeryTokenNameReturnsEncodedCookieNameIfAppPathIsNotEmpty() {
            // Arrange    
            // the string below (as UTF-8 bytes) base64-encodes to "Pz4/Pj8+Pz4/Pj8+Pz4/Pg=="
            string original = "?>?>?>?>?>?>?>?>";

            // Act
            string tokenName = AntiForgeryData.GetAntiForgeryTokenName(original);

            // Assert
            Assert.AreEqual("__RequestVerificationToken_Pz4-Pj8.Pz4-Pj8.Pz4-Pg__", tokenName);
        }

        [TestMethod]
        public void GetAntiForgeryTokenNameReturnsFieldNameIfAppPathIsNull() {
            // Act
            string tokenName = AntiForgeryData.GetAntiForgeryTokenName(null);

            // Assert
            Assert.AreEqual("__RequestVerificationToken", tokenName);
        }

        [TestMethod]
        public void GetUsername_ReturnsEmptyStringIfIdentityIsNull() {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns((IIdentity)null);

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.AreEqual("", username);
        }

        [TestMethod]
        public void GetUsername_ReturnsEmptyStringIfPrincipalIsNull() {
            // Act
            string username = AntiForgeryData.GetUsername(null);

            // Assert
            Assert.AreEqual("", username);
        }

        [TestMethod]
        public void GetUsername_ReturnsEmptyStringIfUserNotAuthenticated() {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity.IsAuthenticated).Returns(false);
            mockPrincipal.Setup(o => o.Identity.Name).Returns("SampleName");

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.AreEqual("", username);
        }

        [TestMethod]
        public void GetUsername_ReturnsUsernameIfUserIsAuthenticated() {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity.IsAuthenticated).Returns(true);
            mockPrincipal.Setup(o => o.Identity.Name).Returns("SampleName");

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.AreEqual("SampleName", username);
        }

        [TestMethod]
        public void NewToken() {
            // Act
            AntiForgeryData token = AntiForgeryData.NewToken();

            // Assert
            int valueLength = Convert.FromBase64String(token.Value).Length;
            Assert.AreEqual(16, valueLength, "Value was not of the correct length.");
            Assert.AreNotEqual(default(DateTime), token.CreationDate, "Creation date should have been initialized.");
        }

        [TestMethod]
        public void SaltProperty() {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            Assert.AreEqual(String.Empty, token.Salt);
            token.Salt = null;
            Assert.AreEqual(String.Empty, token.Salt);
            token.Salt = String.Empty;
            Assert.AreEqual(String.Empty, token.Salt);
        }

        [TestMethod]
        public void ValueProperty() {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            Assert.AreEqual(String.Empty, token.Value);
            token.Value = null;
            Assert.AreEqual(String.Empty, token.Value);
            token.Value = String.Empty;
            Assert.AreEqual(String.Empty, token.Value);
        }
    }
}

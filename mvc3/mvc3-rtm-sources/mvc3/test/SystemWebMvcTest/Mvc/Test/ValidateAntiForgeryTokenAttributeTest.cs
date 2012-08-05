namespace System.Web.Mvc.Test {
    using System;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ValidateAntiForgeryTokenAttributeTest {

        [TestMethod]
        public void OnAuthorization_ThrowsIfFilterContextIsNull() {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    attribute.OnAuthorization(null);
                }, "filterContext");
        }

        [TestMethod]
        public void OnAuthorization_ForwardsAttributes() {
            // Arrange
            HttpContextBase context = new Mock<HttpContextBase>().Object;
            Mock<AuthorizationContext> authorizationContextMock = new Mock<AuthorizationContext>();
            authorizationContextMock.SetupGet(ac => ac.HttpContext).Returns(context);
            bool validateCalled = false;
            Action<HttpContextBase, string> validateMethod = (c, s) => {
                Assert.AreSame(context, c);
                Assert.AreEqual("some salt", s);
                validateCalled = true;
            };
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute(validateMethod) {
                Salt = "some salt"
            };

            // Act
            attribute.OnAuthorization(authorizationContextMock.Object);

            // Assert
            Assert.IsTrue(validateCalled);
        }

        [TestMethod]
        public void SaltProperty() {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            MemberHelper.TestStringProperty(attribute, "Salt", String.Empty);
        }

        [TestMethod]
        public void ValidateThunk_DefaultsToAntiForgeryMethod() {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            Assert.AreEqual(AntiForgery.Validate, attribute.ValidateAction);
        }
    }
}

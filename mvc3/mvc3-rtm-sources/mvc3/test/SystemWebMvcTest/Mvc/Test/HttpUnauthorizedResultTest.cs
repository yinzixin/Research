namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HttpUnauthorizedResultTest {

        [TestMethod]
        public void ExecuteResult() {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 401).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusDescription = "Some description").Verifiable();

            HttpUnauthorizedResult result = new HttpUnauthorizedResult("Some description");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [TestMethod]
        public void StatusCode() {
            Assert.AreEqual(401, new HttpUnauthorizedResult().StatusCode);
        }
    }
}

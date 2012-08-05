namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HttpStatusCodeResultTest {

        [TestMethod]
        public void ExecuteResult() {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 666).Verifiable();

            HttpStatusCodeResult result = new HttpStatusCodeResult(666);

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [TestMethod]
        public void ExecuteResultWithDescription() {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 666).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusDescription = "Foo Bar").Verifiable();
            HttpStatusCodeResult result = new HttpStatusCodeResult(666, "Foo Bar");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [TestMethod]
        public void ExecuteResultWithNullContextThrows() {
            ExceptionHelper.ExpectArgumentNullException(delegate {
                new HttpStatusCodeResult(1).ExecuteResult(context: null);
            }, "context");
        }

        [TestMethod]
        public void StatusCode() {
            Assert.AreEqual(123, new HttpStatusCodeResult(123).StatusCode);

            Assert.AreEqual(234, new HttpStatusCodeResult(234, "foobar").StatusCode);
        }

        [TestMethod]
        public void StatusDescription() {
            Assert.IsNull(new HttpStatusCodeResult(123).StatusDescription);

            Assert.AreEqual("foobar", new HttpStatusCodeResult(234, "foobar").StatusDescription);
        }
    }
}

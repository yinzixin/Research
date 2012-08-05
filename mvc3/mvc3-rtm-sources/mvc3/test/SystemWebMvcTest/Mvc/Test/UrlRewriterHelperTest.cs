namespace System.Web.Mvc.Test {
    using System.Collections.Specialized;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UrlRewriterHelperTest {
        private const string _urlWasRewrittenServerVar = "IIS_WasUrlRewritten";
        private const string _urlRewriterEnabledServerVar = "IIS_UrlRewriteModule";

        [TestMethod]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOff() {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(_urlRewriterEnabledServerVar)).Returns((string)null).Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.IsFalse(result);
            requestMock.Verify();
            requestMock.Verify(c => c.Request.ServerVariables.Get(_urlWasRewrittenServerVar), Times.Never());
        }

        [TestMethod]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOnButRequestWasNotRewritten() {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(_urlRewriterEnabledServerVar)).Returns("yes").Verifiable();
            requestMock.Setup(c => c.Request.ServerVariables.Get(_urlWasRewrittenServerVar)).Returns((string)null).Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.IsFalse(result);
            requestMock.Verify();
        }

        [TestMethod]
        public void WasRequestRewritten_TrueIfUrlRewriterIsTurnedOnAndRequestWasRewritten() {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(_urlRewriterEnabledServerVar)).Returns("yes").Verifiable();
            requestMock.Setup(c => c.Request.ServerVariables.Get(_urlWasRewrittenServerVar)).Returns("yes").Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.IsTrue(result);
            requestMock.Verify();
        }

        [TestMethod]
        public void WasRequestRewritten_ChecksIfUrlRewriterIsTurnedOnOnlyOnce() {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> request1Mock = new Mock<HttpContextBase>();
            request1Mock.Setup(c => c.Request.ServerVariables).Returns(new NameValueCollection());
            Mock<HttpContextBase> request2Mock = new Mock<HttpContextBase>();

            // Act
            bool result1 = helper.WasRequestRewritten(request1Mock.Object);
            bool result2 = helper.WasRequestRewritten(request2Mock.Object);

            // Assert
            request1Mock.Verify(c => c.Request.ServerVariables, Times.Once());
            request2Mock.Verify(c => c.Request.ServerVariables, Times.Never());
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }
    }
}

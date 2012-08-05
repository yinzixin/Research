using System.Collections;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class RequestResourceTrackerTest {
        [TestMethod]
        public void RegisteringForDisposeDisposesObjects() {
            // Arrange
            var context = new Mock<HttpContextBase>();
            IDictionary items = new Hashtable();
            context.Setup(m => m.Items).Returns(items);
            var disposable = new Mock<IDisposable>();
            disposable.Setup(m => m.Dispose()).Verifiable();

            // Act
            RequestResourceTracker.RegisterForDispose(context.Object, disposable.Object);
            RequestResourceTracker.DisposeResources(context.Object);

            // Assert
            disposable.VerifyAll();
        }

        [TestMethod]
        public void RegisteringForDisposeExtensionMethodNullContextThrows() {
            // Arrange
            var disposable = new Mock<IDisposable>();
            
            // Act
            ExceptionAssert.ThrowsArgNull(() => HttpContextExtensions.RegisterForDispose(null, disposable.Object), "context");
        }
    }
}

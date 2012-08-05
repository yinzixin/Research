using System.Web.Razor.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Utils {
    [TestClass]
    public class DisposableActionTest {
        [TestMethod]
        public void ConstructorRequiresNonNullAction() {
            ExceptionAssert.ThrowsArgNull(() => new DisposableAction(null), "action");
        }

        [TestMethod]
        public void ActionIsExecutedOnDispose() {
            // Arrange
            bool called = false;
            DisposableAction action = new DisposableAction(() => {
                called = true;
            });

            // Act
            action.Dispose();

            // Assert
            Assert.IsTrue(called, "The action was not run when the DisposableAction was disposed");
        }
    }
}

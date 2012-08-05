namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterTest {
        [TestMethod]
        public void GuardClause() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new Filter(null, FilterScope.Action, null),
                "instance"
            );
        }

        [TestMethod]
        public void FilterDoesNotImplementIOrderedFilter() {
            // Arrange
            var filterInstance = new object();

            // Act
            var filter = new Filter(filterInstance, FilterScope.Action, null);

            // Assert
            Assert.AreSame(filterInstance, filter.Instance);
            Assert.AreEqual(FilterScope.Action, filter.Scope);
            Assert.AreEqual(Filter.DefaultOrder, filter.Order);
        }

        [TestMethod]
        public void FilterImplementsIOrderedFilter() {
            // Arrange
            var filterInstance = new Mock<IMvcFilter>();
            filterInstance.SetupGet(f => f.Order).Returns(42);

            // Act
            var filter = new Filter(filterInstance.Object, FilterScope.Controller, null);

            // Assert
            Assert.AreSame(filterInstance.Object, filter.Instance);
            Assert.AreEqual(FilterScope.Controller, filter.Scope);
            Assert.AreEqual(42, filter.Order);
        }

        [TestMethod]
        public void ExplicitOrderOverridesIOrderedFilter() {
            // Arrange
            var filterInstance = new Mock<IMvcFilter>();
            filterInstance.SetupGet(f => f.Order).Returns(42);

            // Act
            var filter = new Filter(filterInstance.Object, FilterScope.Controller, 2112);

            // Assert
            Assert.AreSame(filterInstance.Object, filter.Instance);
            Assert.AreEqual(FilterScope.Controller, filter.Scope);
            Assert.AreEqual(2112, filter.Order);
        }
    }
}

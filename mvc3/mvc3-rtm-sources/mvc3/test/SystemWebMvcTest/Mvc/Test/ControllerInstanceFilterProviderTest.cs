namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ControllerInstanceFilterProviderTest {
        [TestMethod]
        public void GetFiltersWithNullControllerReturnsEmptyCollection() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var provider = new ControllerInstanceFilterProvider();

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GetFiltersWithControllerReturnsWrappedController() {
            // Arrange
            var controller = new Mock<ControllerBase>().Object;
            var context = new ControllerContext { Controller = controller };
            var descriptor = new Mock<ActionDescriptor>().Object;
            var provider = new ControllerInstanceFilterProvider();

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Filter filter = result.Single();
            Assert.AreSame(controller, filter.Instance);
            Assert.AreEqual(Int32.MinValue, filter.Order);
            Assert.AreEqual(FilterScope.First, filter.Scope);
        }
    }
}

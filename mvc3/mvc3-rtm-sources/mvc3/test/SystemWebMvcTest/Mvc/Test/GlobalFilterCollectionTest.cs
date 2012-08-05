namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GlobalFilterCollectionTest {
        [TestMethod]
        public void AddPlacesFilterInGlobalScope() {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();

            // Act
            collection.Add(filterInstance);

            // Assert
            Filter filter = collection.Single();
            Assert.AreSame(filterInstance, filter.Instance);
            Assert.AreEqual(FilterScope.Global, filter.Scope);
            Assert.AreEqual(-1, filter.Order);
        }

        [TestMethod]
        public void AddWithOrderPlacesFilterInGlobalScope() {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();

            // Act
            collection.Add(filterInstance, 42);

            // Assert
            Filter filter = collection.Single();
            Assert.AreSame(filterInstance, filter.Instance);
            Assert.AreEqual(FilterScope.Global, filter.Scope);
            Assert.AreEqual(42, filter.Order);
        }

        [TestMethod]
        public void ContainsFindsFilterByInstance() {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);

            // Act
            bool result = collection.Contains(filterInstance);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RemoveDeletesFilterByInstance() {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);

            // Act
            collection.Remove(filterInstance);

            // Assert
            Assert.IsFalse(collection.Any());
        }

        [TestMethod]
        public void CollectionIsIFilterProviderWhichReturnsAllFilters() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);
            var provider = (IFilterProvider)collection;

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Filter filter = result.Single();
            Assert.AreSame(filterInstance, filter.Instance);
        }
    }
}

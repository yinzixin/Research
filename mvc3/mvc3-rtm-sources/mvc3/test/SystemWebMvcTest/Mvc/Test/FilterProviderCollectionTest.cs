namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterProviderCollectionTest {
        [TestMethod]
        public void GuardClauses() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var collection = new FilterProviderCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => collection.GetFilters(null, descriptor),
                "controllerContext"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => collection.GetFilters(context, null),
                "actionDescriptor"
            );
        }

        [TestMethod]
        public void GetFiltersUsesRegisteredProviders() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filter = new Filter(new Object(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor)).Returns(new[] { filter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.AreSame(filter, result.Single());
        }

        [TestMethod]
        public void GetFiltersDelegatesToResolver() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filter = new Filter(new Object(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var resolver = new Resolver<IEnumerable<IFilterProvider>> { Current = new[] { provider.Object } };
            var collection = new FilterProviderCollection(resolver);
           
            provider.Setup(p => p.GetFilters(context, descriptor)).Returns(new[] { filter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.AreSame(filter, result.Single());
        }

        [TestMethod]
        public void GetFiltersSortsFiltersByOrderFirstThenScope() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var actionFilter = new Filter(new Object(), FilterScope.Action, null);
            var controllerFilter = new Filter(new Object(), FilterScope.Controller, null);
            var globalFilter = new Filter(new Object(), FilterScope.Global, null);
            var earlyActionFilter = new Filter(new Object(), FilterScope.Action, -100);
            var lateGlobalFilter = new Filter(new Object(), FilterScope.Global, 100);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                    .Returns(new[] { actionFilter, controllerFilter, globalFilter, earlyActionFilter, lateGlobalFilter });

            // Act
            Filter[] result = collection.GetFilters(context, descriptor).ToArray();

            // Assert
            Assert.AreEqual(5, result.Length);
            Assert.AreSame(earlyActionFilter, result[0]);
            Assert.AreSame(globalFilter, result[1]);
            Assert.AreSame(controllerFilter, result[2]);
            Assert.AreSame(actionFilter, result[3]);
            Assert.AreSame(lateGlobalFilter, result[4]);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private class AllowMultipleFalseAttribute : FilterAttribute {
        }

        [TestMethod]
        public void GetFiltersIncludesLastFilterOnlyWithAttributeUsageAllowMultipleFalse() {    // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                    .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.AreSame(actionFilter, result.Single());
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class AllowMultipleTrueAttribute : FilterAttribute {
        }

        [TestMethod]
        public void GetFiltersIncludesAllFiltersWithAttributeUsageAllowMultipleTrue() {    // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                    .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            List<Filter> result = collection.GetFilters(context, descriptor).ToList();

            // Assert
            Assert.AreSame(globalFilter, result[0]);
            Assert.AreSame(controllerFilter, result[1]);
            Assert.AreSame(actionFilter, result[2]);
        }

        private class AllowMultipleCustomFilter : MvcFilter {
            public AllowMultipleCustomFilter(bool allowMultiple) : base(allowMultiple, -1) {
            }
        }

        [TestMethod]
        public void GetFiltersIncludesLastFilterOnlyWithCustomFilterAllowMultipleFalse() {    // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                    .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.AreSame(actionFilter, result.Single());
        }

        [TestMethod]
        public void GetFiltersIncludesAllFiltersWithCustomFilterAllowMultipleTrue() {    // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                    .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            List<Filter> result = collection.GetFilters(context, descriptor).ToList();

            // Assert
            Assert.AreSame(globalFilter, result[0]);
            Assert.AreSame(controllerFilter, result[1]);
            Assert.AreSame(actionFilter, result[2]);
        }

    }
}

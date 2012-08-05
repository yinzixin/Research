namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MultiServiceResolverTest {

        [TestMethod]
        public void ConstructorWithNullThunkArgumentThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new MultiServiceResolver<TestProvider>(null);
                },
                "itemsThunk");
        }

        [TestMethod]
        public void CurrentPrependsFromResolver() {
            // Arrange
            IEnumerable<TestProvider> providersFromServiceLocation = GetProvidersFromService();
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            IEnumerable<TestProvider> expectedProviders = providersFromServiceLocation.Concat(providersFromItemsThunk);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                    .Returns(providersFromServiceLocation);

            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;

            // Assert
            CollectionAssert.AreEqual(expectedProviders.ToList(), returnedProviders.ToList());
        }

        [TestMethod]
        public void CurrentCachesResolverResult() {
            // Arrange
            IEnumerable<TestProvider> providersFromServiceLocation = GetProvidersFromService();
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            IEnumerable<TestProvider> expectedProviders = providersFromServiceLocation.Concat(providersFromItemsThunk);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                    .Returns(providersFromServiceLocation);

            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;
            IEnumerable<TestProvider> cachedProviders = multiResolver.Current;

            // Assert
            CollectionAssert.AreEqual(expectedProviders.ToList(), returnedProviders.ToList());
            CollectionAssert.AreEqual(expectedProviders.ToList(), cachedProviders.ToList());
            resolver.Verify(r => r.GetServices(typeof(TestProvider)), Times.Exactly(1));
        }

        [TestMethod]
        public void CurrentReturnsCurrentItemsWhenResolverReturnsNoInstances() {
            // Arrange
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            MultiServiceResolver<TestProvider> resolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk);

            // Act
            IEnumerable<TestProvider> returnedProviders = resolver.Current;

            // Assert
            CollectionAssert.AreEqual(providersFromItemsThunk.ToList(), returnedProviders.ToList());
        }

        [TestMethod]
        public void CurrentDoesNotQueryResolverAfterNoInstancesAreReturned() {
            // Arrange
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                    .Returns(new TestProvider[0]);
            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;
            IEnumerable<TestProvider> cachedProviders = multiResolver.Current;

            // Assert
            CollectionAssert.AreEqual(providersFromItemsThunk.ToList(), returnedProviders.ToList());
            CollectionAssert.AreEqual(providersFromItemsThunk.ToList(), cachedProviders.ToList());
            resolver.Verify(r => r.GetServices(typeof(TestProvider)), Times.Exactly(1));
        }

        [TestMethod]
        public void CurrentPropagatesExceptionWhenResolverThrowsNonActivationException() {
            // Arrange
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => null, resolver.Object);

            // Act & Assert
            ExceptionHelper.ExpectException<MockException>(
                () => multiResolver.Current,
                @"IDependencyResolver.GetServices(System.Web.Mvc.Test.MultiServiceResolverTest+TestProvider) invocation failed with mock behavior Strict.
All invocations on the mock must have a corresponding setup."
            );
        }

        private class TestProvider {
        }

        private IEnumerable<TestProvider> GetProvidersFromService() {
            return new TestProvider[]{
                new TestProvider(),
                new TestProvider()
            };
        }

        private IEnumerable<TestProvider> GetProvidersFromItemsThunk() {
            return new TestProvider[]{
                new TestProvider(),
                new TestProvider()
            };
        }
    }
}
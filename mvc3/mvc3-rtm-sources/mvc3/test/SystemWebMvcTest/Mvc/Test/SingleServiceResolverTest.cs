namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SingleServiceResolverTest {

        [TestMethod]
        public void ConstructorWithNullThunkArgumentThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new SingleServiceResolver<TestProvider>(null, null, "TestProvider.Current");
                },
                "currentValueThunk");

            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new SingleServiceResolver<TestProvider>(null, null, "TestProvider.Current");
                },
                "currentValueThunk");

            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new SingleServiceResolver<TestProvider>(() => null, null, "TestProvider.Current");
                },
                "defaultValue");
        }

        [TestMethod]
        public void CurrentConsultsResolver() {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                    .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.AreEqual(providerFromServiceLocation, returnedProvider);
        }

        [TestMethod]
        public void CurrentReturnsCurrentProviderNotDefaultIfSet() {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromCurrentValueThunk = null;
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            providerFromCurrentValueThunk = new TestProvider();
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.AreEqual(providerFromCurrentValueThunk, returnedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)));
        }

        [TestMethod]
        public void CurrentCachesResolverResult() {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                    .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;
            TestProvider cachedProvider = singleResolver.Current;

            // Assert
            Assert.AreEqual(providerFromServiceLocation, returnedProvider);
            Assert.AreEqual(providerFromServiceLocation, cachedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)), Times.Exactly(1));
        }

        [TestMethod]
        public void CurrentDoesNotQueryResolverAfterReceivingNull() {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromCurrentValueThunk = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;
            TestProvider cachedProvider = singleResolver.Current;

            // Assert
            Assert.AreEqual(providerFromCurrentValueThunk, returnedProvider);
            Assert.AreEqual(providerFromCurrentValueThunk, cachedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)), Times.Exactly(1));
        }

        [TestMethod]
        public void CurrentReturnsDefaultIfCurrentNotSet() {
            //Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            //Act
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.AreEqual(returnedProvider, providerFromDefaultValue);
            resolver.Verify(l => l.GetService(typeof(TestProvider)));
        }

        [TestMethod]
        public void CurrentThrowsIfCurrentSetThroughServiceAndSetter() {
            // Arrange
            TestProvider providerFromCurrentValueThunk = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();

            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                    .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            //Act & assert
            ExceptionHelper.ExpectException<InvalidOperationException>(
                () => singleResolver.Current,
                "An instance of TestProvider was found in the resolver as well as a custom registered provider in TestProvider.Current. Please set only one or the other."
            );
        }

        [TestMethod]
        public void CurrentPropagatesExceptionWhenResolverThrowsNonActivationException() {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act & Assert
            ExceptionHelper.ExpectException<MockException>(
                () => singleResolver.Current,
                @"IDependencyResolver.GetService(System.Web.Mvc.Test.SingleServiceResolverTest+TestProvider) invocation failed with mock behavior Strict.
All invocations on the mock must have a corresponding setup."
            );
        }

        private class TestProvider {
        }
    }
}

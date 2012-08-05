namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DependencyResolverTest {

        [TestMethod]
        public void GuardClauses() {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => resolver.InnerSetResolver((IDependencyResolver)null),
                "resolver"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => resolver.InnerSetResolver((object)null),
                "commonServiceLocator"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => resolver.InnerSetResolver(null, type => null),
                "getService"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => resolver.InnerSetResolver(type => null, null),
                "getServices"
            );
        }

        [TestMethod]
        public void DefaultServiceLocatorBehaviorTests() {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            Assert.IsNotNull(resolver.InnerCurrent.GetService<object>()); // Concrete type
            Assert.IsNull(resolver.InnerCurrent.GetService<ModelMetadataProvider>()); // Abstract type
            Assert.IsNull(resolver.InnerCurrent.GetService<IDisposable>()); // Interface
            Assert.IsNull(resolver.InnerCurrent.GetService(typeof(List<>))); // Open generic
        }

        [TestMethod]
        public void DefaultServiceLocatorResolvesNewInstances() {
            // Arrange
            var resolver = new DependencyResolver();

            // Act
            object obj1 = resolver.InnerCurrent.GetService<object>();
            object obj2 = resolver.InnerCurrent.GetService<object>();

            // Assert
            Assert.AreNotSame(obj1, obj2);
        }

        public class MockableResolver {
            public virtual object Get(Type type) {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<object> GetAll(Type type) {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ResolverPassesCallsToDelegateBasedResolver() {
            // Arrange
            var resolver = new DependencyResolver();
            var mockResolver = new Mock<MockableResolver>();
            resolver.InnerSetResolver(mockResolver.Object.Get, mockResolver.Object.GetAll);

            // Act & Assert
            resolver.InnerCurrent.GetService(typeof(object));
            mockResolver.Verify(r => r.Get(typeof(object)));

            resolver.InnerCurrent.GetServices(typeof(string));
            mockResolver.Verify(r => r.GetAll(typeof(string)));
        }

        public class MockableCommonServiceLocator {
            public virtual object GetInstance(Type type) {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<object> GetAllInstances(Type type) {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ResolverPassesCallsToICommonServiceLocator() {
            // Arrange
            var resolver = new DependencyResolver();
            var mockResolver = new Mock<MockableCommonServiceLocator>();
            resolver.InnerSetResolver(mockResolver.Object);

            // Act & Assert
            resolver.InnerCurrent.GetService(typeof(object));
            mockResolver.Verify(r => r.GetInstance(typeof(object)));

            resolver.InnerCurrent.GetServices(typeof(string));
            mockResolver.Verify(r => r.GetAllInstances(typeof(string)));
        }

        class MissingGetInstance {
            public IEnumerable<object> GetAllInstances(Type type) { return null; }
        }

        class MissingGetAllInstances {
            public object GetInstance(Type type) { return null; }
        }

        class GetInstanceHasWrongSignature {
            public string GetInstance(Type type) { return null; }
            public IEnumerable<object> GetAllInstances(Type type) { return null; }
        }

        class GetAllInstancesHasWrongSignature {
            public object GetInstance(Type type) { return null; }
            public IEnumerable<string> GetAllInstances(Type type) { return null; }
        }

        [TestMethod]
        public void ValidationOfCommonServiceLocatorTests() {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            ExceptionHelper.ExpectArgumentException(
                () => resolver.InnerSetResolver(new MissingGetInstance()),
                @"The type System.Web.Mvc.Test.DependencyResolverTest+MissingGetInstance does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator.
Parameter name: commonServiceLocator"
            );
            ExceptionHelper.ExpectArgumentException(
                () => resolver.InnerSetResolver(new MissingGetAllInstances()),
                @"The type System.Web.Mvc.Test.DependencyResolverTest+MissingGetAllInstances does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator.
Parameter name: commonServiceLocator"
            );
            ExceptionHelper.ExpectArgumentException(
                () => resolver.InnerSetResolver(new GetInstanceHasWrongSignature()),
                @"The type System.Web.Mvc.Test.DependencyResolverTest+GetInstanceHasWrongSignature does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator.
Parameter name: commonServiceLocator"
            );
            ExceptionHelper.ExpectArgumentException(
                () => resolver.InnerSetResolver(new GetAllInstancesHasWrongSignature()),
                @"The type System.Web.Mvc.Test.DependencyResolverTest+GetAllInstancesHasWrongSignature does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator.
Parameter name: commonServiceLocator"
            );
        }
    }
}

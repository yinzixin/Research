namespace System.Web.Mvc.Test {
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BuildManagerViewEngineTest {

        [TestMethod]
        public void BuildManagerProperty() {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager(expectedVirtualPath: null, compiledType: null);

            // Act
            engine.BuildManager = buildManagerMock;

            // Assert
            Assert.AreSame(engine.BuildManager, buildManagerMock);
        }

        [TestMethod]
        public void FileExistsReturnsTrueForExistingPath() {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager("some path", typeof(object));
            engine.BuildManager = buildManagerMock;

            // Act
            bool result = engine.FileExists("some path");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileExistsReturnsFalseWhenBuildManagerFileExistsReturnsFalse() {
            // Arrange
            var engine = new TestableBuildManagerViewEngine();
            var buildManagerMock = new MockBuildManager("some path", false);
            engine.BuildManager = buildManagerMock;

            // Act
            bool result = engine.FileExists("some path");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ViewPageActivatorConsultsSetActivatorResolver() {
            // Arrange
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>();

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(activator.Object);

            //Assert
            Assert.AreEqual(activator.Object, engine.ViewPageActivator);
        }

        [TestMethod]
        public void ViewPageActivatorDelegatesToActivatorResolver() {
            // Arrange
            var activator = new Mock<IViewPageActivator>();
            var activatorResolver = new Resolver<IViewPageActivator> { Current = activator.Object };

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(activatorResolver: activatorResolver);

            // Assert
            Assert.AreEqual(activator.Object, engine.ViewPageActivator);
        }

        [TestMethod]
        public void ViewPageActivatorDelegatesToDependencyResolverWhenActivatorResolverIsNull() {
            // Arrange
            var viewInstance = new object();
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(object));
            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            dependencyResolver.Setup(dr => dr.GetService(typeof(object))).Returns(viewInstance).Verifiable();

            // Act
            TestableBuildManagerViewEngine engine = new TestableBuildManagerViewEngine(dependencyResolver: dependencyResolver.Object);
            engine.ViewPageActivator.Create(controllerContext, typeof(object));

            // Assert
            dependencyResolver.Verify();
        }

        [TestMethod]
        public void ViewPageActivatorDelegatesToActivatorCreateInstanceWhenDependencyResolverReturnsNull() {
            // Arrange
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(NoParameterlessCtor));
            var dependencyResolver = new Mock<IDependencyResolver>();

            var engine = new TestableBuildManagerViewEngine(dependencyResolver: dependencyResolver.Object);

            // Act
            Exception ex = ExceptionHelper.Record( // Depend on the fact that Activator.CreateInstance cannot create an object without a parameterless ctor
                () => engine.ViewPageActivator.Create(controllerContext, typeof(NoParameterlessCtor))
            );

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual(typeof(MissingMethodException), ex.GetType());
            StringAssert.Contains(ex.StackTrace, "System.Activator.CreateInstance(");
        }

        [TestMethod]
        public void ActivatorResolverAndDependencyResolverAreNeverCalledWhenViewPageActivatorIsPassedInContstructor() {
            // Arrange
            var controllerContext = new ControllerContext();
            var expectedController = new Goodcontroller();

            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>();

            var resolverActivator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            var activatorResolver = new Resolver<IViewPageActivator> { Current = resolverActivator.Object };

            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);

            //Act
            var engine = new TestableBuildManagerViewEngine(activator.Object, activatorResolver, dependencyResolver.Object);

            //Assert
            Assert.AreSame(activator.Object, engine.ViewPageActivator);
        }

        private class NoParameterlessCtor {
            public NoParameterlessCtor(int x) { }
        }

        private class TestableBuildManagerViewEngine : BuildManagerViewEngine {
            public TestableBuildManagerViewEngine()
                : base() {
            }

            public TestableBuildManagerViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator) {
            }

            public TestableBuildManagerViewEngine(IViewPageActivator viewPageActivator = null, IResolver<IViewPageActivator> activatorResolver = null, IDependencyResolver dependencyResolver = null)
                : base(viewPageActivator, activatorResolver, dependencyResolver) {
            }

            public new IViewPageActivator ViewPageActivator {
                get {
                    return base.ViewPageActivator;
                }
            }

            protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath) {
                throw new NotImplementedException();
            }

            protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {
                throw new NotImplementedException();
            }

            public bool FileExists(string virtualPath) {
                return base.FileExists(null, virtualPath);
            }
        }
    }
}

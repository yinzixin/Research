namespace System.Web.Mvc.Test {
    using System.IO;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CompiledTypeViewTest {
        [TestMethod]
        public void GuardClauses() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new TestableBuildManagerCompiledView(new ControllerContext(), String.Empty),
                "viewPath"
            );

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new TestableBuildManagerCompiledView(new ControllerContext(), null),
                "viewPath"
            );

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new TestableBuildManagerCompiledView(null, "view path"),
                "controllerContext"
            );
        }

        [TestMethod]
        public void RenderWithNullContextThrows() {
            // Arrange
            TestableBuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "~/view");

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => view.Render(null, new Mock<TextWriter>().Object),
                "viewContext"
            );
        }

        [TestMethod]
        public void RenderWithNullViewInstanceThrows() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", compiledType: null);
            TestableBuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "view path");
            view.BuildManager = buildManager;

            // Act & Assert
            ExceptionHelper.ExpectException<InvalidOperationException>(
                () => view.Render(context, new Mock<TextWriter>().Object),
                "The view found at 'view path' was not created."
            );
        }

        [TestMethod]
        public void ViewPathProperty() {
            // Act
            BuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "view path");

            // Assert
            Assert.AreEqual("view path", view.ViewPath);
        }

        [TestMethod]
        public void ViewCreationConsultsSetActivator() {
            // Arrange
            object viewInstance = new object();
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(a => a.Create(controllerContext, typeof(object))).Returns(viewInstance).Verifiable();
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            BuildManagerCompiledView view = new TestableBuildManagerCompiledView(controllerContext, "view path", activator.Object) { BuildManager = buildManager };

            // Act
            view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object);

            // Assert
            activator.Verify();
        }

        [TestMethod]
        public void ViewCreationDelegatesToDependencyResolverWhenActivatorIsNull() {
            // Arrange
            var viewInstance = new object();
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(object));
            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            dependencyResolver.Setup(dr => dr.GetService(typeof(object))).Returns(viewInstance).Verifiable();
            var view = new TestableBuildManagerCompiledView(controllerContext, "view path", dependencyResolver: dependencyResolver.Object) { BuildManager = buildManager };

            // Act
            view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object);

            // Assert
            dependencyResolver.Verify();
        }

        [TestMethod]
        public void ViewCreationDelegatesToActivatorCreateInstanceWhenDependencyResolverReturnsNull() {
            // Arrange
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(NoParameterlessCtor));
            var dependencyResolver = new Mock<IDependencyResolver>();
            var view = new TestableBuildManagerCompiledView(controllerContext, "view path", dependencyResolver: dependencyResolver.Object) { BuildManager = buildManager };

            // Act
            Exception ex = ExceptionHelper.Record( // Depend on the fact that Activator.CreateInstance cannot create an object without a parameterless ctor
                () => view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object)
            );

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual(typeof(MissingMethodException), ex.GetType());
            StringAssert.Contains(ex.StackTrace, "System.Activator.CreateInstance(");
        }

        private class NoParameterlessCtor {
            public NoParameterlessCtor(int x) { }
        }

        private sealed class TestableBuildManagerCompiledView : BuildManagerCompiledView {
            public TestableBuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator = null, IDependencyResolver dependencyResolver = null)
                : base(controllerContext, viewPath, viewPageActivator, dependencyResolver) {
            }

            protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance) {
                return;
            }
        }
    }
}
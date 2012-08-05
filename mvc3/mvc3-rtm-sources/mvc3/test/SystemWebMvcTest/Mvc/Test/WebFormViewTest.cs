namespace System.Web.Mvc.Test {
    using System;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WebFormViewTest {

        [TestMethod]
        public void GuardClauses() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new WebFormView(new ControllerContext(), String.Empty, "~/master"),
                "viewPath"
            );
      
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new WebFormView(new ControllerContext(), null, "~/master"),
                "viewPath"
            );
    
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new WebFormView(null, "view path", "~/master"),
                "controllerContext"
            );
        }

        [TestMethod]
        public void MasterPathProperty() {
            // Act
            WebFormView view = new WebFormView(new ControllerContext(), "view path", "master path");

            // Assert
            Assert.AreEqual("master path", view.MasterPath);
        }

        [TestMethod]
        public void MasterPathPropertyReturnsEmptyStringIfMasterNotSpecified() {
            // Act
            WebFormView view = new WebFormView(new ControllerContext(), "view path", null);

            // Assert
            Assert.AreEqual(String.Empty, view.MasterPath);
        }

        [TestMethod]
        public void RenderWithUnsupportedTypeThrows() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(int));
            WebFormView view = new WebFormView(new ControllerContext(), "view path", null);
            view.BuildManager = buildManagerMock;

            // Act & Assert
            ExceptionHelper.ExpectException<InvalidOperationException>(
                () => view.Render(context, null),
                "The view at 'view path' must derive from ViewPage, ViewPage<TModel>, ViewUserControl, or ViewUserControl<TModel>."
            );
        }

        [TestMethod]
        public void RenderWithViewPageAndMasterRendersView() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewPage viewPage = new StubViewPage();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            WebFormView view = new WebFormView(controllerContext, "view path", "master path", activator.Object);
            view.BuildManager = buildManager;

            // Act
            view.Render(context, null);

            // Assert
            Assert.AreEqual(context, viewPage.ResultViewContext);
            Assert.AreEqual("master path", viewPage.MasterLocation);
        }

        [TestMethod]
        public void RenderWithViewPageRendersView() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewPage viewPage = new StubViewPage();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            WebFormView view = new WebFormView(controllerContext, "view path", null, activator.Object);
            view.BuildManager = buildManager;

            // Act
            view.Render(context, null);

            // Assert
            Assert.AreEqual(context, viewPage.ResultViewContext);
            Assert.AreEqual(String.Empty, viewPage.MasterLocation);
        }

        [TestMethod]
        public void RenderWithViewUserControlAndMasterThrows() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(StubViewUserControl));
            WebFormView view = new WebFormView(new ControllerContext(), "view path", "master path");
            view.BuildManager = buildManagerMock;

            // Act & Assert
            ExceptionHelper.ExpectException<InvalidOperationException>(
                () => view.Render(context, null),
                "A master name cannot be specified when the view is a ViewUserControl."
            );
        }

        [TestMethod]
        public void RenderWithViewUserControlRendersView() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewUserControl viewUserControl = new StubViewUserControl();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewUserControl);
            WebFormView view = new WebFormView(controllerContext, "view path", null, activator.Object) { BuildManager = buildManager };

            // Act
            view.Render(context, null);

            // Assert
            Assert.AreEqual(context, viewUserControl.ResultViewContext);
        }

        public sealed class StubViewPage : ViewPage {
            public ViewContext ResultViewContext;

            public override void RenderView(ViewContext viewContext) {
                ResultViewContext = viewContext;
            }
        }

        public sealed class StubViewUserControl : ViewUserControl {
            public ViewContext ResultViewContext;

            public override void RenderView(ViewContext viewContext) {
                ResultViewContext = viewContext;
            }
        }
    }
}

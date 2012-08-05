namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.TestUtil;
    using System.Web.WebPages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RazorViewTest {
        [TestMethod]
        public void Constructor_RunViewStartPagesParam() {
            var context = new ControllerContext();
            Assert.IsTrue(new RazorView(context, "~/view", "~/master", runViewStartPages: true, viewStartFileExtensions: null).RunViewStartPages);
            Assert.IsFalse(new RazorView(context, "~/view", "~/master", runViewStartPages: false, viewStartFileExtensions: null).RunViewStartPages);
            Assert.IsTrue(new RazorView(context, "~/view", "~/master", runViewStartPages: true, viewStartFileExtensions: null, viewPageActivator: new Mock<IViewPageActivator>().Object).RunViewStartPages);
            Assert.IsFalse(new RazorView(context, "~/view", "~/master", runViewStartPages: false, viewStartFileExtensions: null, viewPageActivator: new Mock<IViewPageActivator>().Object).RunViewStartPages);
        }

        [TestMethod]
        public void ConstructorWithEmptyViewPathThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RazorView(new ControllerContext(), String.Empty, "~/master", false, Enumerable.Empty<string>()),
                "viewPath"
            );
        }

        [TestMethod]
        public void ConstructorWithNullViewPathThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => new RazorView(new ControllerContext(), null, "~/master", false, Enumerable.Empty<string>()),
                "viewPath"
            );
        }

        [TestMethod]
        public void ConstructorWithNullControllerContextThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new RazorView(null, "view path", "~/master", false, Enumerable.Empty<string>()),
                "controllerContext"
            );
        }

        [TestMethod]
        public void LayoutPathProperty() {
            //Arrange
            ControllerContext controllerContext = new ControllerContext();

            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", "master path", false, Enumerable.Empty<string>());

            // Assert
            Assert.AreEqual("master path", view.LayoutPath);
        }

        [TestMethod]
        public void LayoutPathPropertyReturnsEmptyStringIfNullLayoutSpecified() {
            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());

            // Assert
            Assert.AreEqual(String.Empty, view.LayoutPath);
        }

        [TestMethod]
        public void LayoutPathPropertyReturnsEmptyStringIfLayoutNotSpecified() {
            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());

            // Assert
            Assert.AreEqual(String.Empty, view.LayoutPath);
        }

        [TestMethod]
        public void RenderWithNullWriterThrows() {
            // Arrange
            RazorView view = new RazorView(new ControllerContext(), "~/viewPath", null, false, Enumerable.Empty<string>());
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();

            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            view.BuildManager = buildManager;

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => view.Render(viewContextMock.Object, null),
                "writer"
            );
        }

        [TestMethod]
        public void RenderWithUnsupportedTypeThrows() {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(object));
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());
            view.BuildManager = buildManagerMock;

            // Act & Assert
            ExceptionHelper.ExpectException<InvalidOperationException>(
                () => view.Render(context, new Mock<TextWriter>().Object),
                "The view at 'view path' must derive from WebViewPage, or WebViewPage<TModel>."
            );
        }

        [TestMethod]
        public void RenderWithViewPageAndNoStartPageLookupRendersView() {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(vc => vc.HttpContext.Items).Returns(new Dictionary<object, object>());
            viewContextMock.Setup(vc => vc.HttpContext.Request.IsLocal).Returns(false);
            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", null, false, Enumerable.Empty<string>(), activator.Object);
            view.StartPageLookup = (WebPageRenderingBase p, string n, IEnumerable<string> e) => {
                Assert.Fail("ViewStart page lookup should not be called");
                return null;
            };
            view.BuildManager = buildManager;

            // Act
            view.Render(viewContextMock.Object, new Mock<TextWriter>().Object);

            // Assert
            Assert.IsNull(viewPage.Layout);
            Assert.AreEqual("", viewPage.OverridenLayoutPath);
            Assert.AreSame(viewContextMock.Object, viewPage.ViewContext);
            Assert.AreEqual("~/viewPath", viewPage.VirtualPath);
        }

        [TestMethod]
        public void RenderWithViewPageAndStartPageLookupExecutesStartPage() {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(vc => vc.HttpContext.Items).Returns(new Dictionary<object, object>());
            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", null, true, new[] { "cshtml" }, activator.Object);
            Mock<ViewStartPage> startPage = new Mock<ViewStartPage>();
            startPage.Setup(sp => sp.ExecutePageHierarchy()).Verifiable();
            view.StartPageLookup = (WebPageRenderingBase page, string fileName, IEnumerable<string> extensions) => {
                Assert.AreEqual(viewPage, page);
                Assert.AreEqual("_ViewStart", fileName);
                CollectionAssert.AreEqual(new[] { "cshtml" }, extensions.ToList());
                return startPage.Object;
            };
            view.BuildManager = buildManager;

            // Act
            view.Render(viewContextMock.Object, new Mock<TextWriter>().Object);

            // Assert
            startPage.Verify(sp => sp.ExecutePageHierarchy(), Times.Once());
        }

        // TODO: This throws in WebPages and needs to be tracked down.
        [TestMethod, Ignore]
        public void RenderWithViewPageAndLayoutPageRendersView() {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", "~/layoutPath", false, Enumerable.Empty<string>(), activator.Object);
            view.BuildManager = buildManager;

            // Act
            view.Render(context, new Mock<TextWriter>().Object);

            // Assert
            Assert.IsNull(viewPage.Layout);
            Assert.AreEqual("~/layoutPath", viewPage.OverridenLayoutPath);
            Assert.AreSame(context, viewPage.ViewContext);
            Assert.AreEqual("~/viewPath", viewPage.VirtualPath);
        }

        public class StubWebViewPage : WebViewPage {
            public bool InitHelpersCalled;
            public string ResultLayoutPage;
            public string ResultOverridenLayoutPath;
            public ViewContext ResultViewContext;
            public string ResultVirtualPath;

            public override void Execute() {
                ResultLayoutPage = Layout;
                ResultOverridenLayoutPath = OverridenLayoutPath;
                ResultViewContext = ViewContext;
                ResultVirtualPath = VirtualPath;
            }

            public override void InitHelpers() {
                base.InitHelpers();
                InitHelpersCalled = true;
            }
        }
    }
}

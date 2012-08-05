namespace System.Web.Mvc.Test {
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WebFormViewEngineTest {

        [TestMethod]
        public void CreatePartialViewCreatesWebFormView() {
            // Arrange
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Act
            WebFormView result = (WebFormView)engine.CreatePartialView("partial path");

            // Assert
            Assert.AreEqual("partial path", result.ViewPath);
            Assert.AreEqual(String.Empty, result.MasterPath);
        }

        [TestMethod]
        public void CreateViewCreatesWebFormView() {
            // Arrange
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Act
            WebFormView result = (WebFormView)engine.CreateView("view path", "master path");

            // Assert
            Assert.AreEqual("view path", result.ViewPath);
            Assert.AreEqual("master path", result.MasterPath);
        }

        [TestMethod]
        public void WebFormViewEngineSetsViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            //Act & Assert
            Assert.AreEqual(viewPageActivator.Object, viewEngine.ViewPageActivator);
        }

        [TestMethod]
        public void CreatePartialView_PassesViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            // Act
            WebFormView result = (WebFormView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.AreEqual(viewEngine.ViewPageActivator, result._viewPageActivator);
        }

        [TestMethod]
        public void CreateView_PassesViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            // Act
            WebFormView result = (WebFormView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.AreEqual(viewEngine.ViewPageActivator, result._viewPageActivator);
        }

        [TestMethod]
        public void MasterLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.MasterLocationFormats);
        }

        [TestMethod]
        public void AreaMasterLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Areas/{2}/Views/{1}/{0}.master",
                "~/Areas/{2}/Views/Shared/{0}.master",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.AreaMasterLocationFormats);
        }

        [TestMethod]
        public void PartialViewLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.PartialViewLocationFormats);
        }

        [TestMethod]
        public void AreaPartialViewLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.AreaPartialViewLocationFormats);
        }

        [TestMethod]
        public void ViewLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.ViewLocationFormats);
        }

        [TestMethod]
        public void AreaViewLocationFormatsProperty() {
            // Arrange
            string[] expected = new string[] {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, engine.AreaViewLocationFormats);
        }

        [TestMethod]
        public void FileExtensionsProperty() {
            // Arrange
            string[] expected = new string[] {
                "aspx",
                "ascx",
                "master",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();
            
            // Assert
            CollectionAssert.AreEqual(expected, engine.FileExtensions);
        }

        private sealed class TestableWebFormViewEngine : WebFormViewEngine {

            public TestableWebFormViewEngine()
                : base() {
            }

            public TestableWebFormViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator) {
            }

            public new IViewPageActivator ViewPageActivator {
                get {
                    return base.ViewPageActivator;
                }
            }

            public new VirtualPathProvider VirtualPathProvider {
                get {
                    return base.VirtualPathProvider;
                }
                set {
                    base.VirtualPathProvider = value;
                }
            }

            public IView CreatePartialView(string partialPath) {
                return base.CreatePartialView(new ControllerContext(), partialPath);
            }

            public IView CreateView(string viewPath, string masterPath) {
                return base.CreateView(new ControllerContext(), viewPath, masterPath);
            }

            // This method should remain overridable in derived view engines
            protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
                return base.FileExists(controllerContext, virtualPath);
            }
        }
    }
}

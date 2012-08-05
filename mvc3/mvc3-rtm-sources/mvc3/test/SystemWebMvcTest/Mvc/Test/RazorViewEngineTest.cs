namespace System.Web.Mvc.Test {
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RazorViewEngineTest {
        [TestMethod]
        public void AreaMasterLocationFormats() {
            // Arrange
            string[] expected = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.AreaMasterLocationFormats);
        }

        [TestMethod]
        public void AreaPartialViewLocationFormats() {
            // Arrange
            string[] expected = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.AreaPartialViewLocationFormats);
        }

        [TestMethod]
        public void AreaViewLocationFormats() {
            // Arrange
            string[] expected = new[] { 
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.AreaViewLocationFormats);
        }

        [TestMethod]
        public void RazorViewEngineSetsViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            //Act & Assert
            Assert.AreEqual(viewPageActivator.Object, viewEngine.ViewPageActivator);
        }

        [TestMethod]
        public void CreatePartialView_PassesViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            // Act
            RazorView result = (RazorView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.AreEqual(viewEngine.ViewPageActivator, result._viewPageActivator);
        }

        [TestMethod]
        public void CreateView_PassesViewPageActivator() {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            // Act
            RazorView result = (RazorView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.AreEqual(viewEngine.ViewPageActivator, result._viewPageActivator);
        }

        [TestMethod]
        public void CreatePartialView_ReturnsRazorView() {
            // Arrange
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine();

            // Act
            RazorView result = (RazorView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.AreEqual("partial path", result.ViewPath);
            Assert.AreEqual(String.Empty, result.LayoutPath);
            Assert.IsFalse(result.RunViewStartPages);
        }

        [TestMethod]
        public void CreateView_ReturnsRazorView() {
            // Arrange
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine() {
                FileExtensions = new[] { "cshtml", "vbhtml", "razor" }
            };

            // Act
            RazorView result = (RazorView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.AreEqual("partial path", result.ViewPath);
            Assert.AreEqual("master path", result.LayoutPath);
            CollectionAssert.AreEqual(new[] { "cshtml", "vbhtml", "razor" }, result.ViewStartFileExtensions.ToArray());
            Assert.IsTrue(result.RunViewStartPages);
        }

        [TestMethod]
        public void FileExtensionsProperty() {
            // Arrange
            string[] expected = new[] {
                "cshtml",
                "vbhtml",
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.FileExtensions);
        }

        [TestMethod]
        public void MasterLocationFormats() {
            // Arrange
            string[] expected = new[] { 
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.MasterLocationFormats);
        }

        [TestMethod]
        public void PartialViewLocationFormats() {
            // Arrange
            string[] expected = new[] { 
                 "~/Views/{1}/{0}.cshtml",
                 "~/Views/{1}/{0}.vbhtml",
                 "~/Views/Shared/{0}.cshtml",
                 "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.PartialViewLocationFormats);
        }

        [TestMethod]
        public void ViewLocationFormats() {
            // Arrange
            string[] expected = new[] { 
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            CollectionAssert.AreEqual(expected, viewEngine.ViewLocationFormats);
        }

        [TestMethod]
        public void ViewStartFileName() {
            Assert.AreEqual("_ViewStart", RazorViewEngine.ViewStartFileName);
        }

        private sealed class TestableRazorViewEngine : RazorViewEngine {

            public TestableRazorViewEngine()
                : base() {
            }

            public TestableRazorViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator) {
            }

            public new IViewPageActivator ViewPageActivator {
                get {
                    return base.ViewPageActivator;
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

namespace System.Web.Mvc.Test {
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ViewContextTest {

        [TestMethod]
        public void GuardClauses() {
            // Arrange
            var controllerContext = new Mock<ControllerContext>().Object;
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new ViewContext(null, view, viewData, tempData, writer),
                "controllerContext"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new ViewContext(controllerContext, null, viewData, tempData, writer),
                "view"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new ViewContext(controllerContext, view, null, tempData, writer),
                "viewData"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new ViewContext(controllerContext, view, viewData, null, writer),
                "tempData"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new ViewContext(controllerContext, view, viewData, tempData, null),
                "writer"
            );
        }

        [TestMethod]
        public void FormIdGeneratorProperty() {
            // Arrange
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Items).Returns(new Hashtable());
            var viewContext = new ViewContext {
                HttpContext = mockHttpContext.Object
            };

            // Act
            string form0Name = viewContext.FormIdGenerator();
            string form1Name = viewContext.FormIdGenerator();
            string form2Name = viewContext.FormIdGenerator();

            // Assert
            Assert.AreEqual("form0", form0Name);
            Assert.AreEqual("form1", form1Name);
            Assert.AreEqual("form2", form2Name);
        }

        [TestMethod]
        public void PropertiesAreSet() {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Items).Returns(new Hashtable());
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act
            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, tempData, writer);

            // Assert
            Assert.AreEqual(view, viewContext.View);
            Assert.AreEqual(viewData, viewContext.ViewData);
            Assert.AreEqual(tempData, viewContext.TempData);
            Assert.AreEqual(writer, viewContext.Writer);
            Assert.IsFalse(viewContext.UnobtrusiveJavaScriptEnabled, "Unobtrusive JavaScript should be off by default");
            Assert.IsNull(viewContext.FormContext, "FormContext shouldn't be set unless Html.BeginForm() has been called.");
        }

        [TestMethod]
        public void ViewContextUsesScopeThunkForInstanceClientValidationFlag() {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.IsFalse(viewContext.ClientValidationEnabled);
            viewContext.ClientValidationEnabled = true;
            Assert.IsTrue(viewContext.ClientValidationEnabled);
            Assert.AreEqual(true, scope[ViewContext.ClientValidationKeyName]);
            viewContext.ClientValidationEnabled = false;
            Assert.IsFalse(viewContext.ClientValidationEnabled);
            Assert.AreEqual(false, scope[ViewContext.ClientValidationKeyName]);
        }

        [TestMethod]
        public void ViewContextUsesScopeThunkForInstanceUnobstrusiveJavaScriptFlag() {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.IsFalse(viewContext.UnobtrusiveJavaScriptEnabled);
            viewContext.UnobtrusiveJavaScriptEnabled = true;
            Assert.IsTrue(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.AreEqual(true, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
            viewContext.UnobtrusiveJavaScriptEnabled = false;
            Assert.IsFalse(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.AreEqual(false, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
        }
    }
}

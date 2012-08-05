namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.Routing;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.UnitTestUtil;
    using Moq;
    using Match = System.Text.RegularExpressions.Match;

    [TestClass]
    public class HtmlHelperTest {
        public static readonly RouteValueDictionary AttributesDictionary = new RouteValueDictionary(new { baz = "BazValue" });
        public static readonly object AttributesObjectDictionary = new { baz = "BazObjValue" };
        public static readonly object AttributesObjectUnderscoresDictionary = new { foo_baz = "BazObjValue" };

        // Constructor

        [TestMethod]
        public void ConstructorGuardClauses() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new HtmlHelper(null, viewDataContainer),
                "viewContext"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new HtmlHelper(viewContext, null),
                "viewDataContainer"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => new HtmlHelper(viewContext, viewDataContainer, null),
                "routeCollection"
            );
        }

        [TestMethod]
        public void PropertiesAreSet() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewData = new ViewDataDictionary<String>("The Model");
            var routes = new RouteCollection();
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(vdc => vdc.ViewData).Returns(viewData);

            // Act
            var htmlHelper = new HtmlHelper(viewContext, mockViewDataContainer.Object, routes);

            // Assert
            Assert.AreEqual(viewContext, htmlHelper.ViewContext);
            Assert.AreEqual(mockViewDataContainer.Object, htmlHelper.ViewDataContainer);
            Assert.AreEqual(routes, htmlHelper.RouteCollection);
            Assert.AreEqual(viewData.Model, htmlHelper.ViewData.Model);
        }

        [TestMethod]
        public void DefaultRouteCollectionIsRouteTableRoutes() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;

            // Act
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Assert
            Assert.AreEqual(RouteTable.Routes, htmlHelper.RouteCollection);
        }

        // AnonymousObjectToHtmlAttributes tests

        [TestMethod]
        public void ConvertsUnderscoresInNamesToDashes() {
            // Arrange
            var attributes = new { foo = "Bar", baz_bif = "pow_wow" };

            // Act
            RouteValueDictionary result = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Bar", result["foo"]);
            Assert.AreEqual("pow_wow", result["baz-bif"]);
        }

        // AttributeEncode

        [TestMethod]
        public void AttributeEncodeObject() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((object)@"<"">");

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;&quot;>", "Text is not being properly HTML attribute-encoded.");
        }

        [TestMethod]
        public void AttributeEncodeObjectNull() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((object)null);

            // Assert
            Assert.AreEqual("", encodedHtml);
        }

        [TestMethod]
        public void AttributeEncodeString() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode(@"<"">");

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;&quot;>", "Text is not being properly HTML attribute-encoded.");
        }

        [TestMethod]
        public void AttributeEncodeStringNull() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((string)null);

            // Assert
            Assert.AreEqual("", encodedHtml);
        }

        // EnableClientValidation

        [TestMethod]
        public void EnableClientValidation() {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.EnableClientValidation();

            // Act & assert
            mockViewContext.VerifySet(vc => vc.ClientValidationEnabled = true);
        }

        // EnableUnobtrusiveJavaScript

        [TestMethod]
        public void EnableUnobtrusiveJavaScript() {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.EnableUnobtrusiveJavaScript();

            // Act & assert
            mockViewContext.VerifySet(vc => vc.UnobtrusiveJavaScriptEnabled = true);
        }

        // Encode

        [TestMethod]
        public void EncodeObject() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((object)"<br />");

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;br /&gt;", "Text is not being properly HTML-encoded.");
        }

        [TestMethod]
        public void EncodeObjectNull() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((object)null);

            // Assert
            Assert.AreEqual("", encodedHtml);
        }

        [TestMethod]
        public void EncodeString() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode("<br />");

            // Assert
            Assert.AreEqual(encodedHtml, "&lt;br /&gt;", "Text is not being properly HTML-encoded.");
        }

        [TestMethod]
        public void EncodeStringNull() {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((string)null);

            // Assert
            Assert.AreEqual("", encodedHtml);
        }

        // GetModelStateValue

        [TestMethod]
        public void GetModelStateValueReturnsNullIfModelStateHasNoValue() {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.ModelState.AddModelError("foo", "some error text"); // didn't call SetModelValue()

            HtmlHelper helper = new HtmlHelper(new ViewContext(), new SimpleViewDataContainer(vdd));

            // Act
            object retVal = helper.GetModelStateValue("foo", typeof(object));

            // Assert
            Assert.IsNull(retVal);
        }

        [TestMethod]
        public void GetModelStateValueReturnsNullIfModelStateKeyNotPresent() {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new SimpleViewDataContainer(vdd));

            // Act
            object retVal = helper.GetModelStateValue("key_not_present", typeof(object));

            // Assert
            Assert.IsNull(retVal);
        }

        // GenerateIdFromName

        [TestMethod]
        public void GenerateIdFromNameTests() {
            // Guard clauses
            ExceptionHelper.ExpectArgumentNullException(
                () => HtmlHelper.GenerateIdFromName(null),
                "name"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => HtmlHelper.GenerateIdFromName(null, "?"),
                "name"
            );
            ExceptionHelper.ExpectArgumentNullException(
                () => HtmlHelper.GenerateIdFromName("?", null),
                "idAttributeDotReplacement"
            );

            // Default replacement tests
            Assert.AreEqual("", HtmlHelper.GenerateIdFromName(""));
            Assert.AreEqual("Foo", HtmlHelper.GenerateIdFromName("Foo"));
            Assert.AreEqual("Foo_Bar", HtmlHelper.GenerateIdFromName("Foo.Bar"));
            Assert.AreEqual("Foo_Bar_Baz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz"));
            Assert.IsNull(HtmlHelper.GenerateIdFromName("1Foo"));
            Assert.AreEqual("Foo_0_", HtmlHelper.GenerateIdFromName("Foo[0]"));

            // Custom replacement tests
            Assert.AreEqual("", HtmlHelper.GenerateIdFromName("", "?"));
            Assert.AreEqual("Foo", HtmlHelper.GenerateIdFromName("Foo", "?"));
            Assert.AreEqual("Foo?Bar", HtmlHelper.GenerateIdFromName("Foo.Bar", "?"));
            Assert.AreEqual("Foo?Bar?Baz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz", "?"));
            Assert.AreEqual("FooBarBaz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz", ""));
            Assert.IsNull(HtmlHelper.GenerateIdFromName("1Foo", "?"));
            Assert.AreEqual("Foo?0?", HtmlHelper.GenerateIdFromName("Foo[0]", "?"));
        }

        // RenderPartialInternal

        [TestMethod]
        public void NullPartialViewNameThrows() {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => helper.RenderPartialInternal(null /* partialViewName */, null /* viewData */, null /* model */, TextWriter.Null),
                "partialViewName");
        }

        [TestMethod]
        public void EmptyPartialViewNameThrows() {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => helper.RenderPartialInternal(String.Empty /* partialViewName */, null /* viewData */, null /* model */, TextWriter.Null),
                "partialViewName");
        }

        [TestMethod]
        public void EngineLookupSuccessCallsRender() {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            TextWriter writer = helper.ViewContext.Writer;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), writer))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, _) => {
                        Assert.AreSame(helper.ViewContext.View, viewContext.View);
                        Assert.AreSame(helper.ViewContext.TempData, viewContext.TempData);
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, writer, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [TestMethod]
        public void EngineLookupFailureThrows() {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(new[] { "location1", "location2" }))
                .Verifiable();

            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, TextWriter.Null, engine.Object),
                @"The partial view 'partial-view' was not found or no view engine supports the searched locations. The following locations were searched:
location1
location2");

            engine.Verify();
        }

        [TestMethod]
        public void RenderPartialInternalWithNullModelAndNullViewData() {
            // Arrange
            object model = new object();
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) => {
                        Assert.AreNotSame(helper.ViewData, viewContext.ViewData);  // New view data instance
                        Assert.AreEqual("Bar", viewContext.ViewData["Foo"]);       // Copy of the existing view data
                        Assert.AreSame(model, viewContext.ViewData.Model);         // Keep existing model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [TestMethod]
        public void RenderPartialInternalWithNonNullModelAndNullViewData() {
            // Arrange
            object model = new object();
            object newModel = new object();
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) => {
                        Assert.AreNotSame(helper.ViewData, viewContext.ViewData);  // New view data instance
                        Assert.AreEqual(0, viewContext.ViewData.Count);            // Empty (not copied)
                        Assert.AreSame(newModel, viewContext.ViewData.Model);      // New model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, newModel, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [TestMethod]
        public void RenderPartialInternalWithNullModelAndNonNullViewData() {
            // Arrange
            object model = new object();
            object vddModel = new object();
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Baz"] = "Biff";
            vdd.Model = vddModel;
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) => {
                        Assert.AreNotSame(helper.ViewData, viewContext.ViewData);  // New view data instance
                        Assert.AreEqual(1, viewContext.ViewData.Count);            // Copy of the passed view data, not original view data
                        Assert.AreEqual("Biff", viewContext.ViewData["Baz"]);
                        Assert.AreSame(vddModel, viewContext.ViewData.Model);      // Keep model from passed view data, not original view data
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", vdd, null /* model */, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [TestMethod]
        public void RenderPartialInternalWithNonNullModelAndNonNullViewData() {
            // Arrange
            object model = new object();
            object vddModel = new object();
            object newModel = new object();
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Baz"] = "Biff";
            vdd.Model = vddModel;
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) => {
                        Assert.AreNotSame(helper.ViewData, viewContext.ViewData);  // New view data instance
                        Assert.AreEqual(1, viewContext.ViewData.Count);            // Copy of the passed view data, not original view data
                        Assert.AreEqual("Biff", viewContext.ViewData["Baz"]);
                        Assert.AreSame(newModel, viewContext.ViewData.Model);      // New model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", vdd, newModel, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        // HttpMethodOverride

        [TestMethod]
        public void HttpMethodOverrideGuardClauses() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => htmlHelper.HttpMethodOverride(null),
                "httpMethod"
            );
            ExceptionHelper.ExpectArgumentException(
                () => htmlHelper.HttpMethodOverride((HttpVerbs)10000),
                @"The specified HttpVerbs value is not supported. The supported values are Delete, Head, and Put.
Parameter name: httpVerb"
            );
            ExceptionHelper.ExpectArgumentException(
                () => htmlHelper.HttpMethodOverride(HttpVerbs.Get),
                @"The specified HttpVerbs value is not supported. The supported values are Delete, Head, and Put.
Parameter name: httpVerb"
            );
            ExceptionHelper.ExpectArgumentException(
                () => htmlHelper.HttpMethodOverride(HttpVerbs.Post),
                @"The specified HttpVerbs value is not supported. The supported values are Delete, Head, and Put.
Parameter name: httpVerb"
            );
            ExceptionHelper.ExpectArgumentException(
                () => htmlHelper.HttpMethodOverride("gEt"),
                @"The GET and POST HTTP methods are not supported.
Parameter name: httpMethod"
            );
            ExceptionHelper.ExpectArgumentException(
                () => htmlHelper.HttpMethodOverride("pOsT"),
                @"The GET and POST HTTP methods are not supported.
Parameter name: httpMethod"
            );
        }

        [TestMethod]
        public void HttpMethodOverrideWithMethodRendersHiddenField() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            MvcHtmlString hiddenField = htmlHelper.HttpMethodOverride("PUT");

            // Assert
            Assert.AreEqual<string>(@"<input name=""X-HTTP-Method-Override"" type=""hidden"" value=""PUT"" />", hiddenField.ToHtmlString());
        }

        [TestMethod]
        public void HttpMethodOverrideWithVerbRendersHiddenField() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            MvcHtmlString hiddenField = htmlHelper.HttpMethodOverride(HttpVerbs.Delete);

            // Assert
            Assert.AreEqual<string>(@"<input name=""X-HTTP-Method-Override"" type=""hidden"" value=""DELETE"" />", hiddenField.ToHtmlString());
        }

        // Unobtrusive validation attributes

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenClientValidationIsNotEnabled() {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenUnobtrusiveJavaScriptIsNotEnabled() {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenFieldHasAlreadyBeenRendered() {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetAndSetsFieldAsRenderedForFieldWithNoClientRules() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate { return Enumerable.Empty<ModelClientValidationRule>(); };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual(0, result.Count);
            Assert.IsTrue(formContext.RenderedField("foobar"));
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesIncludesDataValTrueWithNonEmptyClientRuleList() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate {
                return new[] { new ModelClientValidationRule { ValidationType = "type" } };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("true", result["data-val"]);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesWithEmptyMessage() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate {
                return new[] { new ModelClientValidationRule { ValidationType = "type" } };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("", result["data-val-type"]);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesMessageIsHtmlEncoded() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate {
                return new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "<script>alert('xss')</script>" } };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result["data-val-type"]);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesWithMessageAndParameters() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate {
                ModelClientValidationRule rule = new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" };
                rule.ValidationParameters["foo"] = "bar";
                rule.ValidationParameters["baz"] = "biff";
                return new[] { rule };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("error", result["data-val-type"]);
            Assert.AreEqual("bar", result["data-val-type-foo"]);
            Assert.AreEqual("biff", result["data-val-type-baz"]);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesWithTwoClientRules() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate {
                ModelClientValidationRule rule1 = new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" };
                rule1.ValidationParameters["foo"] = "bar";
                rule1.ValidationParameters["baz"] = "biff";
                ModelClientValidationRule rule2 = new ModelClientValidationRule { ValidationType = "othertype", ErrorMessage = "othererror" };
                rule2.ValidationParameters["true3"] = "false4";
                return new[] { rule1, rule2 };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("error", result["data-val-type"]);
            Assert.AreEqual("bar", result["data-val-type-foo"]);
            Assert.AreEqual("biff", result["data-val-type-baz"]);
            Assert.AreEqual("othererror", result["data-val-othertype"]);
            Assert.AreEqual("false4", result["data-val-othertype-true3"]);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesUsesShortNameForModelMetadataLookup() {
            // Arrange
            string passedName = null;
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = (name, _) => {
                passedName = name;
                return Enumerable.Empty<ModelClientValidationRule>();
            };

            // Act
            htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.AreEqual("foobar", passedName);
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributeUsesViewDataForModelMetadataLookup() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary<MyModel>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("MyProperty");

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("true", result["data-val"]);
            Assert.AreEqual("My required message", result["data-val-required"]);
        }

        class MyModel {
            [Required(ErrorMessage = "My required message")]
            public object MyProperty { get; set; }
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesMarksRenderedFieldsWithFullName() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.IsFalse(formContext.RenderedField("foobar"));
            Assert.IsTrue(formContext.RenderedField("Prefix.foobar"));
        }

        [TestMethod]
        public void GetUnobtrusiveValidationAttributesGuardClauses() {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act & Assert
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule());
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"OnlyLowerCase\", client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule { ValidationType = "OnlyLowerCase" });
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"nonumb3rs\", client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule { ValidationType = "nonumb3rs" });
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must be unique. The following validation type was seen more than once: rule", new ModelClientValidationRule { ValidationType = "rule" }, new ModelClientValidationRule { ValidationType = "rule" });

            var emptyParamName = new ModelClientValidationRule { ValidationType = "type" };
            emptyParamName.ValidationParameters[""] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule", emptyParamName);

            var paramNameMixedCase = new ModelClientValidationRule { ValidationType = "type" };
            paramNameMixedCase.ValidationParameters["MixedCase"] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: MixedCase, client rule type: System.Web.Mvc.ModelClientValidationRule", paramNameMixedCase);

            var paramNameStartsWithNumber = new ModelClientValidationRule { ValidationType = "type" };
            paramNameStartsWithNumber.ValidationParameters["2112"] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: 2112, client rule type: System.Web.Mvc.ModelClientValidationRule", paramNameStartsWithNumber);
        }

        [TestMethod]
        public void RawReturnsWrapperMarkup() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);
            string markup = "<b>bold</b>";

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(markup);

            // Assert
            Assert.AreEqual("<b>bold</b>", markupHtml.ToString());
            Assert.AreEqual("<b>bold</b>", markupHtml.ToHtmlString());
        }

        [TestMethod]
        public void RawAllowsNullValue() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(null);

            // Assert
            Assert.AreEqual(null, markupHtml.ToString());
            Assert.AreEqual(null, markupHtml.ToHtmlString());
        }

        [TestMethod]
        public void RawAllowsEmptyValue() {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            IHtmlString markupHtml = htmlHelper.Raw("");

            // Assert
            Assert.AreEqual("", markupHtml.ToString());
            Assert.AreEqual("", markupHtml.ToHtmlString());
        }


        // Helpers

        private static void AssertBadClientValidationRule(HtmlHelper htmlHelper, string expectedMessage, params ModelClientValidationRule[] rules) {
            htmlHelper.ClientValidationRuleFactory = delegate { return rules; };
            ExceptionHelper.ExpectInvalidOperationException(
                () => htmlHelper.GetUnobtrusiveValidationAttributes(Guid.NewGuid().ToString()),
                expectedMessage
            );
        }

        internal static ValueProviderResult GetValueProviderResult(object rawValue, string attemptedValue) {
            return new ValueProviderResult(rawValue, attemptedValue, CultureInfo.InvariantCulture);
        }

        internal static IDisposable ReplaceCulture(string currentCulture, string currentUICulture) {
            CultureInfo newCulture = CultureInfo.GetCultureInfo(currentCulture);
            CultureInfo newUICulture = CultureInfo.GetCultureInfo(currentUICulture);
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newUICulture;
            return new CultureReplacement { OriginalCulture = originalCulture, OriginalUICulture = originalUICulture };
        }

        private class CultureReplacement : IDisposable {
            public CultureInfo OriginalCulture;
            public CultureInfo OriginalUICulture;
            public void Dispose() {
                Thread.CurrentThread.CurrentCulture = OriginalCulture;
                Thread.CurrentThread.CurrentUICulture = OriginalUICulture;
            }
        }

        private class TestableHtmlHelper : HtmlHelper {
            TestableHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
                : base(viewContext, viewDataContainer) { }

            public static TestableHtmlHelper Create() {
                ViewDataDictionary viewData = new ViewDataDictionary();

                Mock<ViewContext> mockViewContext = new Mock<ViewContext>() { DefaultValue = DefaultValue.Mock };
                mockViewContext.Setup(c => c.HttpContext.Response.Output).Throws(new Exception("Response.Output should never be called."));
                mockViewContext.Setup(c => c.ViewData).Returns(viewData);
                mockViewContext.Setup(c => c.Writer).Returns(new StringWriter());

                Mock<IViewDataContainer> container = new Mock<IViewDataContainer>();
                container.Setup(c => c.ViewData).Returns(viewData);

                return new TestableHtmlHelper(mockViewContext.Object, container.Object);
            }

            public void RenderPartialInternal(string partialViewName,
                                              ViewDataDictionary viewData,
                                              object model,
                                              TextWriter writer,
                                              params IViewEngine[] engines) {
                base.RenderPartialInternal(partialViewName, viewData, model, writer, new ViewEngineCollection(engines));
            }
        }
    }
}

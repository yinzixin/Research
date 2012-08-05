namespace System.Web.Mvc.Html.Test {
    using System.Linq.Expressions;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LabelExtensionsTest {
        ModelMetadataProvider originalProvider;
        Mock<ModelMetadataProvider> metadataProvider;
        Mock<ModelMetadata> metadata;
        ViewDataDictionary viewData;
        Mock<ViewContext> viewContext;
        Mock<IViewDataContainer> viewDataContainer;
        HtmlHelper<object> html;

        [TestInitialize]
        public void Initialialize() {
            metadataProvider = new Mock<ModelMetadataProvider>();
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), null);
            viewData = new ViewDataDictionary();

            viewContext = new Mock<ViewContext>();
            viewContext.Setup(c => c.ViewData).Returns(viewData);

            viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(c => c.ViewData).Returns(viewData);

            html = new HtmlHelper<object>(viewContext.Object, viewDataContainer.Object);

            metadataProvider.Setup(p => p.GetMetadataForProperties(It.IsAny<object>(), It.IsAny<Type>()))
                            .Returns(new ModelMetadata[0]);
            metadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                            .Returns(metadata.Object);
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                            .Returns(metadata.Object);

            originalProvider = ModelMetadataProviders.Current;
            ModelMetadataProviders.Current = metadataProvider.Object;
        }

        [TestCleanup]
        public void Cleanup() {
            ModelMetadataProviders.Current = originalProvider;
        }

        // Label tests

        [TestMethod]
        public void LabelNullExpressionThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => html.Label(null),
                "expression");
        }

        [TestMethod]
        public void LabelViewDataNotFound() {
            // Act
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(@"<label for=""PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelViewDataNull() {
            // Act
            viewData["PropertyName"] = null;
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(@"<label for=""PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        class Model {
            public string PropertyName { get; set; }
        }

        [TestMethod]
        public void LabelViewDataFromPropertyGetsActualPropertyType() {
            // Arrange
            Model model = new Model { PropertyName = "propertyValue" };
            HtmlHelper<Model> html = new HtmlHelper<Model>(viewContext.Object, viewDataContainer.Object);
            viewData.Model = model;
            metadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), typeof(Model), "PropertyName"))
                            .Returns(metadata.Object)
                            .Verifiable();

            // Act
            html.Label("PropertyName");

            // Assert
            metadataProvider.Verify();
        }

        [TestMethod]
        public void LabelUsesTemplateInfoPrefix() {
            // Arrange
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            // Act
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(@"<label for=""Prefix_PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelUsesLabelTextBeforeMetadata() {
            // Arrange
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), "Custom property name from metadata");
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                            .Returns(metadata.Object);

            //Act
            MvcHtmlString result = html.Label("PropertyName", "Label Text");

            // Assert
            Assert.AreEqual(@"<label for=""PropertyName"">Label Text</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelUsesMetadataForDisplayTextWhenLabelTextIsNull() {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(@"<label for=""PropertyName"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelUsesMetadataForPropertyNameWhenDisplayNameIsNull() {
            // Arrange
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), "Custom property name from metadata");
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                            .Returns(metadata.Object);

            // Act
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(@"<label for=""PropertyName"">Custom property name from metadata</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelEmptyDisplayNameReturnsEmptyLabelText() {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns(string.Empty);

            // Act
            MvcHtmlString result = html.Label("PropertyName");

            // Assert
            Assert.AreEqual(string.Empty, result.ToHtmlString());
        }

        // LabelFor tests

        [TestMethod]
        public void LabelForNullExpressionThrows() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => html.LabelFor((Expression<Func<Object, Object>>)null),
                "expression");
        }

        [TestMethod]
        public void LabelForNonMemberExpressionThrows() {
            // Act & Assert
            ExceptionHelper.ExpectInvalidOperationException(
                () => html.LabelFor(model => new { foo = "Bar" }),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [TestMethod]
        public void LabelForViewDataNotFound() {
            // Arrange
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey);

            // Assert
            Assert.AreEqual(@"<label for=""unknownKey"">unknownKey</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForUsesTemplateInfoPrefix() {
            // Arrange
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey);

            // Assert
            Assert.AreEqual(@"<label for=""Prefix_unknownKey"">unknownKey</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForUsesLabelTextBeforeModelMetadata() {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");
            string unknownKey = "this is a dummy parameter value";

            //Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, "Label Text");

            // Assert
            Assert.AreEqual(@"<label for=""unknownKey"">Label Text</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForUsesModelMetadata() {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey);

            // Assert
            Assert.AreEqual(@"<label for=""unknownKey"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForEmptyDisplayNameReturnsEmptyLabelText() {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns(string.Empty);
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey);

            // Assert
            Assert.AreEqual(string.Empty, result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForWithNestedClass() { // Dev10 Bug #936323
            // Arrange
            HtmlHelper<NestedProduct> html = new HtmlHelper<NestedProduct>(viewContext.Object, viewDataContainer.Object);

            // Act
            MvcHtmlString result = html.LabelFor(nested => nested.product.Id);

            //Assert
            Assert.AreEqual(@"<label for=""product_Id"">Id</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForWithArrayExpression() { // Dev10 Bug #905780
            // Arrange
            HtmlHelper<Cart> html = new HtmlHelper<Cart>(viewContext.Object, viewDataContainer.Object);

            // Act
            MvcHtmlString result = html.LabelFor(cart => cart.Products[0].Id);

            // Assert
            Assert.AreEqual(@"<label for=""Products_0__Id"">Id</label>", result.ToHtmlString());
        }

        private class Product {
            public int Id { get; set; }
        }

        private class Cart {
            public Product[] Products { get; set; }
        }

        private class NestedProduct {
            public Product product = new Product();
        }

        // LabelForModel tests

        [TestMethod]
        public void LabelForModelUsesLabelTextBeforeModelMetadata() {
            // Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel("Label Text");

            // Assert
            Assert.AreEqual(@"<label for=""Prefix"">Label Text</label>", result.ToHtmlString());
        }

        [TestMethod]
        public void LabelForModelUsesModelMetadata() {
            // Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel();

            // Assert
            Assert.AreEqual(@"<label for=""Prefix"">Custom display name from metadata</label>", result.ToHtmlString());
        }
    }
}

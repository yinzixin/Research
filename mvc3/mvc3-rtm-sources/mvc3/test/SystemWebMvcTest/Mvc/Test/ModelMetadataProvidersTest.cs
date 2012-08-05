namespace System.Web.Mvc.Test {
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ModelMetadataProvidersTest {
        [TestMethod]
        public void DefaultModelMetadataProviderIsDataAnnotations() {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();

            // Act
            ModelMetadataProvider provider = providers.CurrentInternal;

            // Assert
            Assert.AreEqual(typeof(DataAnnotationsModelMetadataProvider), provider.GetType());
        }

        [TestMethod]
        public void SettingModelMetadataProviderReturnsSetProvider() {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            providers.CurrentInternal = provider.Object;

            // Assert
            Assert.AreSame(provider.Object, providers.CurrentInternal);
        }

        [TestMethod]
        public void SettingNullModelMetadataProviderUsesEmptyModelMetadataProvider() {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();

            // Act
            providers.CurrentInternal = null;

            // Assert
            Assert.AreEqual(typeof(EmptyModelMetadataProvider), providers.CurrentInternal.GetType());
        }

        [TestMethod]
        public void ModelMetadataProvidersCurrentDelegatesToResolver() {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Resolver<ModelMetadataProvider> resolver = new Resolver<ModelMetadataProvider> { Current = provider.Object };
            ModelMetadataProviders providers = new ModelMetadataProviders(resolver);

            // Act
            ModelMetadataProvider result = providers.CurrentInternal;

            // Assert
            Assert.AreSame(provider.Object, result);
        }

        private class Resolver<T> : IResolver<T> {
            public T Current { get; set; }
        }
    }
}



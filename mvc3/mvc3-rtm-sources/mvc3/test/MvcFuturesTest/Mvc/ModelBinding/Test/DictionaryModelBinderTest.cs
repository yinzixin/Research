namespace Microsoft.Web.Mvc.ModelBinding.Test {
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.UnitTestUtil;
    using Moq;
    using ModelBinderProviderCollection = Microsoft.Web.Mvc.ModelBinding.ModelBinderProviderCollection;

    [TestClass]
    public class DictionaryModelBinderTest {

        [TestMethod]
        public void BindModel() {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext() {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider() {
                    { "someName[0]", new KeyValuePair<int, string>(42, "forty-two") },
                    { "someName[1]", new KeyValuePair<int, string>(84, "eighty-four") }
                }
            };

            Mock<IExtensibleModelBinder> mockKvpBinder = new Mock<IExtensibleModelBinder>();
            mockKvpBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc) {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(KeyValuePair<int, string>), mockKvpBinder.Object, false /* suppressPrefixCheck */);

            // Act
            bool retVal = new DictionaryModelBinder<int, string>().BindModel(controllerContext, bindingContext);

            // Assert
            Assert.IsTrue(retVal);

            IDictionary<int, string> dictionary = bindingContext.Model as IDictionary<int, string>;
            Assert.IsNotNull(dictionary);
            Assert.AreEqual(2, dictionary.Count);
            Assert.AreEqual("forty-two", dictionary[42]);
            Assert.AreEqual("eighty-four", dictionary[84]);
        }

    }
}

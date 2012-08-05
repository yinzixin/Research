namespace System.Web.Mvc.Test {
    using System.Globalization;
    using System.Web.Routing;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpiicitRouteDataValueProviderFactoryTest {

        [TestMethod]
        public void GetValueProviderReturnsChildActionValue() {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();

            string conflictingKey = "conflictingKey";

            controllerContext.RouteData.Values["conflictingKey"] = 43;

            DictionaryValueProvider<object> explictValueDictionary = new DictionaryValueProvider<object>(new RouteValueDictionary { { conflictingKey, 42 } }, CultureInfo.InvariantCulture);
            controllerContext.RouteData.Values[ChildActionValueProvider.ChildActionValuesKey] = explictValueDictionary;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.AreEqual(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue(conflictingKey);

            Assert.IsNotNull(vpResult, "Should have contained a value for key '" + conflictingKey + "'.");
            Assert.AreEqual(42, vpResult.RawValue);
            Assert.AreEqual("42", vpResult.AttemptedValue);
            Assert.AreEqual(CultureInfo.InvariantCulture, vpResult.Culture);
        }

        [TestMethod]
        public void GetValueProviderReturnsNullIfNoChildActionDictionary() {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();
            controllerContext.RouteData.Values["forty-two"] = 42;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.AreEqual(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("forty-two");

            Assert.IsNull(vpResult, "Should not have contained a value for key 'forty-two'.");
        }

        [TestMethod]
        public void GetValueProviderReturnsNullIfKeyIsNotInChildActionDictionary() {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();
            controllerContext.RouteData.Values["forty-two"] = 42;

            DictionaryValueProvider<object> explictValueDictionary = new DictionaryValueProvider<object>(new RouteValueDictionary { { "forty-three", 42 } }, CultureInfo.CurrentUICulture);
            controllerContext.RouteData.Values[ChildActionValueProvider.ChildActionValuesKey] = explictValueDictionary;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.AreEqual(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("forty-two");

            Assert.IsNull(vpResult, "ChildActionValueProvider should not have contained a value for key 'forty-two'.");
        }

        [TestMethod]
        public void GetValueProvider_ThrowsIfControllerContextIsNull() {
            // Arrange
            RouteDataValueProviderFactory factory = new RouteDataValueProviderFactory();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    factory.GetValueProvider(null);
                }, "controllerContext");
        }

    }
}
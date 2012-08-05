namespace Microsoft.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Routing;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.Mvc;
    using Moq;

    [TestClass]
    public class CookieTempDataProviderTest {
        [TestMethod]
        public void ConstructProviderThrowsOnNullHttpContext() {
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new CookieTempDataProvider(null);
                },
                "httpContext");
        }

        [TestMethod]
        public void CtorSetsHttpContextProperty() {
            var httpContext = new Mock<HttpContextBase>().Object;
            var provider = new CookieTempDataProvider(httpContext);

            Assert.AreEqual(httpContext, provider.HttpContext);
        }

        [TestMethod]
        public void LoadTempDataWithEmptyCookieReturnsEmptyDictionary() {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            cookie.Value = string.Empty;
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.IsNotNull(tempData);
            Assert.AreEqual(0, tempData.Count);
        }

        [TestMethod]
        public void LoadTempDataWithNullCookieReturnsEmptyTempDataDictionary() {
            var cookies = new HttpCookieCollection();

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.IsNotNull(tempData);
            Assert.AreEqual(0, tempData.Count);
        }

        [TestMethod]
        public void LoadTempDataIgnoresNullResponseCookieDoesNotThrowException() {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            var initialTempData = new Dictionary<string, object>();
            initialTempData.Add("WhatIsInHere?", "Stuff");
            cookie.Value = CookieTempDataProvider.DictionaryToBase64String(initialTempData);
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns((HttpCookieCollection)null);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.AreEqual("Stuff", tempData["WhatIsInHere?"]);
        }

        [TestMethod]
        public void LoadTempDataWithNullResponseDoesNotThrowException() {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            var initialTempData = new Dictionary<string, object>();
            initialTempData.Add("WhatIsInHere?", "Stuff");
            cookie.Value = CookieTempDataProvider.DictionaryToBase64String(initialTempData);
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns((HttpResponseBase)null);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.AreEqual("Stuff", tempData["WhatIsInHere?"]);
        }

        [TestMethod]
        public void SaveTempDataStoresSerializedFormInCookie() {
            var cookies = new HttpCookieCollection();
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);
            var tempData = new Dictionary<string, object>();
            tempData.Add("Testing", "Turn it up to 11");
            tempData.Add("Testing2", 1.23);

            provider.SaveTempData(null, tempData);
            HttpCookie cookie = cookies["__ControllerTempData"];
            string serialized = cookie.Value;
            IDictionary<string, object> deserializedTempData = CookieTempDataProvider.Base64StringToDictionary(serialized);
            Assert.AreEqual("Turn it up to 11", deserializedTempData["Testing"]);
            Assert.AreEqual(1.23, deserializedTempData["Testing2"]);
        }

        [TestMethod]
        public void CanLoadTempDataFromCookie() {
            var tempData = new Dictionary<string, object>();
            tempData.Add("abc", "easy as 123");
            tempData.Add("price", 1.234);
            string serializedTempData = CookieTempDataProvider.DictionaryToBase64String(tempData);
            
            var cookies = new HttpCookieCollection();
            var httpCookie = new HttpCookie("__ControllerTempData");
            httpCookie.Value = serializedTempData;
            cookies.Add(httpCookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);
            IDictionary<string, object> loadedTempData = provider.LoadTempData(null /* controllerContext */);
            Assert.AreEqual(2, loadedTempData.Count);
            Assert.AreEqual("easy as 123", loadedTempData["abc"]);
            Assert.AreEqual(1.234, loadedTempData["price"]);
        }
    }
}

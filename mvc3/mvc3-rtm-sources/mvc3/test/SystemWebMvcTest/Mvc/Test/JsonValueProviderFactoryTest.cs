namespace System.Web.Mvc.Test {
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class JsonValueProviderFactoryTest {

        [TestMethod]
        public void GetValueProvider_NullControllerContext_ThrowsException() {
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            ExceptionHelper.ExpectArgumentNullException(delegate() {
                factory.GetValueProvider(controllerContext: null);    
            }, "controllerContext");
        }

        [TestMethod]
        public void GetValueProvider_SimpleArrayJsonObject() {
            const string jsonString = @"
[ ""abc"", null, ""foobar"" ]
";
            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.IsTrue(valueProvider.ContainsPrefix("[0]"));
            Assert.IsTrue(valueProvider.ContainsPrefix("[2]"));
            Assert.IsFalse(valueProvider.ContainsPrefix("[3]"));

            ValueProviderResult vpResult1 = valueProvider.GetValue("[0]");
            Assert.AreEqual("abc", vpResult1.AttemptedValue);
            Assert.AreEqual(CultureInfo.CurrentCulture, vpResult1.Culture);

            // null values should exist in the backing store as actual entries
            ValueProviderResult vpResult2 = valueProvider.GetValue("[1]");
            Assert.IsNotNull(vpResult2);
            Assert.IsNull(vpResult2.RawValue);
        }

        [TestMethod]
        public void GetValueProvider_SimpleDictionaryJsonObject() {
            const string jsonString = @"
{   ""FirstName"":""John"",
    ""LastName"": ""Doe""
}";

            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.IsTrue(valueProvider.ContainsPrefix("firstname"));
            
            ValueProviderResult vpResult1 = valueProvider.GetValue("firstname");
            Assert.AreEqual("John", vpResult1.AttemptedValue);
            Assert.AreEqual(CultureInfo.CurrentCulture, vpResult1.Culture);
        }

        [TestMethod]
        public void GetValueProvider_ComplexJsonObject() {
            // Arrange
            const string jsonString = @"
[
  { 
    ""BillingAddress"": {
      ""Street"": ""1 Microsoft Way"",
      ""City"": ""Redmond"",
      ""State"": ""WA"",
      ""ZIP"": 98052 },
    ""ShippingAddress"": { 
      ""Street"": ""123 Anywhere Ln"",
      ""City"": ""Anytown"",
      ""State"": ""ZZ"",
      ""ZIP"": 99999 }
  },
  { 
    ""Enchiladas"": [ ""Delicious"", ""Nutritious""]
  }
]
";

            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.IsNotNull(valueProvider);

            Assert.IsTrue(valueProvider.ContainsPrefix("[0].billingaddress"), "[0].billingaddress prefix should have existed.");
            Assert.IsNull(valueProvider.GetValue("[0].billingaddress"), "[0].billingaddress key should not have existed.");

            Assert.IsTrue(valueProvider.ContainsPrefix("[0].billingaddress.street"));
            Assert.IsNotNull(valueProvider.GetValue("[0].billingaddress.street"));

            ValueProviderResult vpResult1 = valueProvider.GetValue("[1].enchiladas[0]");
            Assert.IsNotNull(vpResult1);
            Assert.AreEqual("Delicious", vpResult1.AttemptedValue);
            Assert.AreEqual(CultureInfo.CurrentCulture, vpResult1.Culture);
        }

        [TestMethod]
        public void GetValueProvider_NoJsonBody_ReturnsNull() {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("application/json");
            mockControllerContext.Setup(o => o.HttpContext.Request.InputStream).Returns(new MemoryStream());

            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.IsNull(valueProvider);
        }

        [TestMethod]
        public void GetValueProvider_NotJsonRequest_ReturnsNull() {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("not JSON");

            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.IsNull(valueProvider);
        }

        private static ControllerContext GetJsonEnabledControllerContext(string jsonString) {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            MemoryStream jsonStream = new MemoryStream(jsonBytes);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("application/json");
            mockControllerContext.Setup(o => o.HttpContext.Request.InputStream).Returns(jsonStream);
            return mockControllerContext.Object;
        }

    }
}

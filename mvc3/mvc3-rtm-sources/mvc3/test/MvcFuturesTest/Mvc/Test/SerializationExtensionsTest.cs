namespace Microsoft.Web.Mvc.Test {
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.Mvc;
    using Microsoft.Web.UnitTestUtil;
    using Moq;

    [TestClass]
    public class SerializationExtensionsTest {

        [TestMethod]
        public void SerializeFromProvidedValueOverridesViewData() {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary() {
                { "someKey", 42 }
            };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Serialize("Hello!", SerializationMode.Signed)).Returns("some-value");

            // Act
            MvcHtmlString htmlString = helper.Serialize("someKey", "Hello!", SerializationMode.Signed, mockSerializer.Object);

            // Assert
            Assert.AreEqual(@"<input name=""someKey"" type=""hidden"" value=""some-value"" />", htmlString.ToHtmlString());
        }

        [TestMethod]
        public void SerializeFromViewData() {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary() {
                { "someKey", 42 }
            };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Serialize(42, SerializationMode.EncryptedAndSigned)).Returns("some-other-value");

            // Act
            MvcHtmlString htmlString = helper.Serialize("someKey", SerializationMode.EncryptedAndSigned, mockSerializer.Object);

            // Assert
            Assert.AreEqual(@"<input name=""someKey"" type=""hidden"" value=""some-other-value"" />", htmlString.ToHtmlString());
        }

        [TestMethod]
        public void SerializeThrowsIfHtmlHelperIsNull() {
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    SerializationExtensions.Serialize(null, "someName");
                }, "htmlHelper");
        }

        [TestMethod]
        public void SerializeThrowsIfNameIsEmpty() {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    helper.Serialize("");
                }, "name");
        }

        [TestMethod]
        public void SerializeThrowsIfNameIsNull() {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    helper.Serialize(null);
                }, "name");
        }

    }
}

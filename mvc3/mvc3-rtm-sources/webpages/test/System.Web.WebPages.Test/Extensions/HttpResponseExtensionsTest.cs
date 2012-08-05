namespace Microsoft.WebPages.Test.Helpers {
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages;
    using Moq;

    [TestClass]
    public class HttpResponseExtensionsTest {
        HttpResponseBase _response;
        string _redirectUrl;
        StringBuilder _output;
        Stream _outputStream;

        [TestInitialize]
        public void Initialize() {
            _output = new StringBuilder();
            _outputStream = new MemoryStream();
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.SetupProperty(response => response.StatusCode);
            mockResponse.SetupProperty(response => response.ContentType);
            mockResponse.Setup(response => response.Redirect(It.IsAny<string>())).Callback((string url) => _redirectUrl = url);
            mockResponse.Setup(response => response.Write(It.IsAny<string>())).Callback((string str) => _output.Append(str));
            mockResponse.Setup(response => response.OutputStream).Returns(_outputStream);
            mockResponse.Setup(response => response.OutputStream).Returns(_outputStream);
            mockResponse.Setup(response => response.Output).Returns(new StringWriter(_output));
            _response = mockResponse.Object;
        }

        [TestMethod]
        public void SetStatusWithIntTest() {
            int status = 200;
            _response.SetStatus(status);
            Assert.AreEqual(status, _response.StatusCode);
        }

        [TestMethod]
        public void SetStatusWithHttpStatusCodeTest() {
            HttpStatusCode status = HttpStatusCode.Forbidden;
            _response.SetStatus(status);
            Assert.AreEqual((int)status, _response.StatusCode);
        }

        [TestMethod]
        public void WriteBinaryTest() {
            string foo = "I am a string, please don't mangle me!";
            _response.WriteBinary(ASCIIEncoding.ASCII.GetBytes(foo));
            _outputStream.Flush();
            _outputStream.Position = 0;
            StreamReader reader = new StreamReader(_outputStream);
            Assert.AreEqual(foo, reader.ReadToEnd());
        }

        [TestMethod]
        public void WriteBinaryWithMimeTypeTest() {
            string foo = "I am a string, please don't mangle me!";
            string mimeType = "mime/foo";
            _response.WriteBinary(ASCIIEncoding.ASCII.GetBytes(foo), mimeType);
            _outputStream.Flush();
            _outputStream.Position = 0;
            StreamReader reader = new StreamReader(_outputStream);
            Assert.AreEqual(foo, reader.ReadToEnd());
            Assert.AreEqual(mimeType, _response.ContentType);
        }
    }
}

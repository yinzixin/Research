using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Helpers.Test {
    [TestClass]
    public class WebMailTest {
        const string FromAddress = "abc@123.com";
        const string Server = "myserver.com";
        const int Port = 100;
        const string UserName = "My UserName";
        const string Password = "My Password";

        private TestContext testContextInstance;
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void WebMailSmtpServerTests() {
            // All tests prior to setting smtp server go here
            // Verify Send throws if no SmtpServer is set
            ExceptionAssert.Throws<InvalidOperationException>(
                () => WebMail.Send(to: "test@test.com", subject: "test", body: "test body"),
                "\"SmtpServer\" was not specified."
            );

            // Verify SmtpServer uses scope storage.
            // Arrange
            var value = "value";

            // Act
            WebMail.SmtpServer = value;

            // Assert
            Assert.AreEqual(WebMail.SmtpServer, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._smtpServerKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForSmtpPort() {
            // Arrange
            var value = 4;

            // Act
            WebMail.SmtpPort = value;

            // Assert
            Assert.AreEqual(WebMail.SmtpPort, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._smtpPortKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForEnableSsl() {
            // Arrange
            var value = true;

            // Act
            WebMail.EnableSsl = value;

            // Assert
            Assert.AreEqual(WebMail.EnableSsl, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._enableSslKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForDefaultCredentials() {
            // Arrange
            var value = true;

            // Act
            WebMail.SmtpUseDefaultCredentials = value;

            // Assert
            Assert.AreEqual(WebMail.SmtpUseDefaultCredentials, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._smtpUseDefaultCredentialsKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForUserName() {
            // Arrange
            var value = "value";

            // Act
            WebMail.UserName = value;

            // Assert
            Assert.AreEqual(WebMail.UserName, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._userNameKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForPassword() {
            // Arrange
            var value = "value";

            // Act
            WebMail.Password = value;

            // Assert
            Assert.AreEqual(WebMail.Password, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._passwordKey], value);
        }

        [TestMethod]
        public void WebMailUsesScopeStorageForFrom() {
            // Arrange
            var value = "value";

            // Act
            WebMail.From = value;

            // Assert
            Assert.AreEqual(WebMail.From, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[WebMail._fromKey], value);
        }

        [TestMethod]
        public void WebMailThrowsWhenSmtpServerValueIsNullOrEmpty() {
            // Act and Assert
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebMail.SmtpServer = null, "SmtpServer");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebMail.SmtpServer = String.Empty, "SmtpServer");
        }

        [TestMethod()]
        public void MailSendWithNullInCollection_ThrowsArgumentException() {
            bool exceptionThrown = false;
            try {
                WebMail.Send("foo@bar.com", "sub", "body", filesToAttach: new string[] { "c:\\foo.txt", null });
            }
            catch (System.ArgumentException) {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);

            exceptionThrown = false;
            try {
                WebMail.Send("foo@bar.com", "sub", "body", additionalHeaders: new string[] { "foo:bar", null });
            }
            catch (System.ArgumentException) {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }
    }
}
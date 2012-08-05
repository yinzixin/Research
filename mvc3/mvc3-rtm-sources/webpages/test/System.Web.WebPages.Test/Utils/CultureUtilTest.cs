using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class CultureUtilTest {
        [TestMethod]
        public void SetAutoCultureWithNoUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(null);
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentCulture);
        }

        [TestMethod]
        public void SetAutoUICultureWithNoUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(null);
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentUICulture);
        }

        [TestMethod]
        public void SetAutoCultureWithEmptyUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(Enumerable.Empty<string>());
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentCulture);
        }

        [TestMethod]
        public void SetAutoUICultureWithEmptyUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(Enumerable.Empty<string>());
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentUICulture);
        }

        [TestMethod]
        public void SetAutoCultureWithBlankUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(new[] { " " });
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentCulture);
        }

        [TestMethod]
        public void SetAutoUICultureWithBlankUserLanguagesDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(new[] { " " });
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentUICulture);
        }

        [TestMethod]
        public void SetAutoCultureWithInvalidLanguageDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "aa-AA", "bb-BB", "cc-CC" });
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentCulture);
        }

        [TestMethod]
        public void SetAutoUICultureWithInvalidLanguageDoesNothing() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "aa-AA", "bb-BB", "cc-CC" });
            Thread thread = GetThread();
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(culture, thread.CurrentUICulture);
        }

        [TestMethod]
        public void SetAutoCultureDetectsUserLanguageCulture() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB", "en-US", "ar-eg" });
            Thread thread = GetThread();

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [TestMethod]
        public void SetAutoUICultureDetectsUserLanguageCulture() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB", "en-US", "ar-eg" });
            Thread thread = GetThread();

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        [TestMethod]
        public void SetAutoCultureUserLanguageWithQParameterCulture() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB;q=0.3", "en-US", "ar-eg;q=0.5" });
            Thread thread = GetThread();

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [TestMethod]
        public void SetAutoUICultureDetectsUserLanguageWithQParameterCulture() {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB;q=0.3", "en-US", "ar-eg;q=0.5" });
            Thread thread = GetThread();

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        [TestMethod]
        public void SetCultureWithInvalidCultureThrows() {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = GetThread();

            // Act and Assert
            ExceptionAssert.Throws<CultureNotFoundException>(() => CultureUtil.SetCulture(thread, context, "sans-culture"));
        }

        [TestMethod]
        public void SetUICultureWithInvalidCultureThrows() {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = GetThread();

            // Act and Assert
            ExceptionAssert.Throws<CultureNotFoundException>(() => CultureUtil.SetUICulture(thread, context, "sans-culture"));
        }

        [TestMethod]
        public void SetCultureWithValidCulture() {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = GetThread();

            // Act
            CultureUtil.SetCulture(thread, context, "en-GB");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [TestMethod]
        public void SetUICultureWithValidCulture() {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = GetThread();

            // Act
            CultureUtil.SetUICulture(thread, context, "en-GB");

            // Assert
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.AreEqual("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        private static Thread GetThread() {
            return new Thread(() => { });
        }

        private static HttpContextBase GetContextForSetCulture(IEnumerable<string> userLanguages = null) {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.UserLanguages).Returns(userLanguages == null ? null : userLanguages.ToArray());
            return contextMock.Object;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Razor.Test {
    [TestClass]
    public class RazorCodeLanguageTest {
        [TestMethod]
        public void ServicesPropertyContainsEntriesForCSharpCodeLanguageService() {
            // Assert
            Assert.AreEqual(2, RazorCodeLanguage.Languages.Count);
            Assert.IsInstanceOfType(RazorCodeLanguage.Languages["cshtml"], typeof(CSharpRazorCodeLanguage));
            Assert.IsInstanceOfType(RazorCodeLanguage.Languages["vbhtml"], typeof(VBRazorCodeLanguage));
        }

        [TestMethod]
        public void GetServiceByExtensionReturnsEntryMatchingExtensionWithoutPreceedingDot() {
            Assert.IsInstanceOfType(RazorCodeLanguage.GetLanguageByExtension("cshtml"), typeof(CSharpRazorCodeLanguage));
        }

        [TestMethod]
        public void GetServiceByExtensionReturnsEntryMatchingExtensionWithPreceedingDot() {
            Assert.IsInstanceOfType(RazorCodeLanguage.GetLanguageByExtension(".cshtml"), typeof(CSharpRazorCodeLanguage));
        }

        [TestMethod]
        public void GetServiceByExtensionReturnsNullIfNoServiceForSpecifiedExtension() {
            Assert.IsNull(RazorCodeLanguage.GetLanguageByExtension("foobar"));
        }

        [TestMethod]
        public void MultipleCallsToGetServiceWithSameExtensionReturnSameObject() {
            // Arrange
            RazorCodeLanguage expected = RazorCodeLanguage.GetLanguageByExtension("cshtml");

            // Act
            RazorCodeLanguage actual = RazorCodeLanguage.GetLanguageByExtension("cshtml");

            // Assert
            Assert.AreSame(expected, actual);
        }
    }
}

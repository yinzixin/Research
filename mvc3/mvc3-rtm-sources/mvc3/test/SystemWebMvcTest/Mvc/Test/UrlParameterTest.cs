namespace System.Web.Mvc.Test {
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UrlParameterTest {

        [TestMethod]
        public void UrlParameterOptionalToStringReturnsEmptyString() {
            // Act & Assert
            Assert.AreEqual(String.Empty, UrlParameter.Optional.ToString());
        }
    }
}

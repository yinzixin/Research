namespace System.Web.Mvc.Test {
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ViewEnginesTest {

        [TestMethod]
        public void EnginesProperty() {
            // Act
            ViewEngineCollection collection = ViewEngines.Engines;

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.IsInstanceOfType(collection[0], typeof(WebFormViewEngine), "Collection should have contained a WebFormViewEngine.");
            Assert.IsInstanceOfType(collection[1], typeof(RazorViewEngine), "Collection should have contained a RazorViewEngine.");
        }

    }
}

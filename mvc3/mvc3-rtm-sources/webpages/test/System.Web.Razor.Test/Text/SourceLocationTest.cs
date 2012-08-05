using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Text {
    [TestClass]
    public class SourceLocationTest {
        [TestMethod]
        public void ConstructorWithLineAndCharacterIndexSetsAssociatedProperties() {
            // Act
            SourceLocation loc = new SourceLocation(0, 42, 24);

            // Assert
            Assert.AreEqual(0, loc.AbsoluteIndex);
            Assert.AreEqual(42, loc.LineIndex);
            Assert.AreEqual(24, loc.CharacterIndex);
        }
    }
}

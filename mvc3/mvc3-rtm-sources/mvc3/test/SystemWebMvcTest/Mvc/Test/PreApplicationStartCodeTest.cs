namespace System.Web.Mvc.Test {
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Web.UnitTestUtil;

    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void PreApplicationStartCodeIsNotBrowsableTest() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }

        [TestMethod]
        public void PreApplicationStartMethodAttributeTest() {
            Assembly assembly = typeof(Controller).Assembly;
            object[] attributes = assembly.GetCustomAttributes(typeof(PreApplicationStartMethodAttribute), true);
            Assert.AreEqual(1, attributes.Length, "{0} does not have a PreApplicationStartMethodAttribute. ", assembly.FullName);
            PreApplicationStartMethodAttribute preAppStartMethodAttribute = (PreApplicationStartMethodAttribute)attributes[0];
            Type preAppStartMethodType = preAppStartMethodAttribute.Type;
            Assert.AreEqual(typeof(PreApplicationStartCode), preAppStartMethodType, "The PreApplicationStartMethod type should be PreApplicationStartCode");
        }
    }
}

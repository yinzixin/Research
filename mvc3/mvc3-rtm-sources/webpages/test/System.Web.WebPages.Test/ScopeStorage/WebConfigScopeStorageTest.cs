using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class WebConfigScopeStorageTest {
        [TestMethod]
        public void WebConfigScopeStorageReturnsConfigValue() {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.AreEqual(stateStorage["foo1"], "bar1");
            Assert.AreEqual(stateStorage["foo2"], "bar2");
        }

        [TestMethod]
        public void WebConfigScopeStoragePerformsCaseInsensitiveKeyCompares() {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.AreEqual(stateStorage["FOO1"], "bar1");
            Assert.AreEqual(stateStorage["FoO2"], "bar2");
        }

        [TestMethod]
        public void WebConfigScopeStorageThrowsWhenWriting() {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Act and Assert
            ExceptionAssert.Throws<NotSupportedException>(() => stateStorage["foo"] = "some value", "Storage scope is read only.");
            ExceptionAssert.Throws<NotSupportedException>(() => stateStorage.Add("foo", "value"), "Storage scope is read only.");
            ExceptionAssert.Throws<NotSupportedException>(() => stateStorage.Remove("foo"), "Storage scope is read only.");
            ExceptionAssert.Throws<NotSupportedException>(() => stateStorage.Clear(), "Storage scope is read only.");
            ExceptionAssert.Throws<NotSupportedException>(() => stateStorage.Remove(new KeyValuePair<object, object>("foo", "bar")), "Storage scope is read only.");
        }

        [TestMethod]
        public void WebConfigStateAllowsEnumeratingOverConfigItems() {
            // Arrange
            var dictionary = new Dictionary<string, string> { { "a", "b" }, { "c", "d" }, { "x12", "y34" } };
            var stateStorage = GetWebConfigScopeStorage(dictionary);

            // Act and Assert
            Assert.IsTrue(dictionary.All(item => item.Value == stateStorage[item.Key] as string));
        }

        private WebConfigScopeDictionary GetWebConfigScopeStorage(IDictionary<string, string> values = null) {
            NameValueCollection collection = new NameValueCollection();
            if (values == null) {
                collection.Add("foo1", "bar1");
                collection.Add("foo2", "bar2");
            }
            else {
                foreach (var item in values) {
                    collection.Add(item.Key, item.Value);
                }
            }

            return new WebConfigScopeDictionary(collection);

        }
    }
}

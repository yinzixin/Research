using System.Collections.Generic;
using System.Web.WebPages.Scope;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class ScopeStorageKeyComparerTest {
        [TestMethod]
        public void ScopeStorageComparerPerformsCaseInsensitiveOrdinalComparisonForStrings() {
            // Arrange
            var dictionary = new Dictionary<object, object>(ScopeStorageComparer.Instance) { { "foo", "bar" } };

            // Act and Assert
            Assert.AreEqual(dictionary["foo"], "bar");
            Assert.AreEqual(dictionary["foo"], dictionary["FOo"]);
        }

        [TestMethod]
        public void ScopeStorageComparerPerformsRegularComparisonForOtherTypes() {
            // Arrange
            var stateStorage = new Dictionary<object, object> { { 4, "4-value" }, { new Person { ID = 10 }, "person-value" } };

            // Act and Assert
            Assert.AreEqual(stateStorage[4], "4-value");
            Assert.AreEqual(stateStorage[(int)8 / 2], stateStorage[4]);
            Assert.AreEqual(stateStorage[new Person { ID = 10 }], "person-value");
        }

        private class Person {
            public int ID { get; set; }

            public override bool Equals(object o) {
                var other = o as Person;
                return (other != null) && (other.ID == ID);
            }

            public override int GetHashCode() {
                return ID;
            }
        }
    }
}

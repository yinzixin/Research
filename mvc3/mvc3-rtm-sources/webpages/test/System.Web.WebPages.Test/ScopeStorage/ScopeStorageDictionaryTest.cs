using System.Web.WebPages.Scope;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class ScopeStorageDictionaryTest {
        [TestMethod]
        public void ScopeStorageDictionaryLooksUpLocalValuesFirst() {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.AreEqual(stateStorage["f"], "f2");
        }

        [TestMethod]
        public void ScopeStorageDictionaryOverridesParentValuesWithLocalValues() {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.AreEqual(stateStorage["a"], "a2");
            Assert.AreEqual(stateStorage["d"], "d2");
        }

        [TestMethod]
        public void ScopeStorageDictionaryLooksUpParentValuesWhenNotFoundLocally() {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.AreEqual(stateStorage["c"], "c0");
            Assert.AreEqual(stateStorage["b"], "b1");
        }

        [TestMethod]
        public void ScopeStorageDictionaryTreatsNullAsOrdinaryValues() {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();
            stateStorage["b"] = null;

            // Act and Assert
            Assert.IsNull(stateStorage["b"]);
        }

        private ScopeStorageDictionary GetChainedStorageStateDictionary() {
            var root = new ScopeStorageDictionary();
            root["a"] = "a0";
            root["b"] = "b0";
            root["c"] = "c0";
            
            var firstGen = new ScopeStorageDictionary(baseScope: root);
            firstGen["a"] = "a1";
            firstGen["b"] = "b1";
            firstGen["d"] = "d1";
            firstGen["e"] = "e1";
                

            var secondGen = new ScopeStorageDictionary(baseScope: firstGen);
            secondGen["a"] = "a2";
            secondGen["d"] = "d2";
            secondGen["f"] = "f2";

            return secondGen;
        }
    }
}
 
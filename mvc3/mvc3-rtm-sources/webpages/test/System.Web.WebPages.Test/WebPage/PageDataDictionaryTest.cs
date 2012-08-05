using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class PageDataDictionaryTest {
        [TestMethod]
        public void PageDataDictionaryConstructorTest() {
            var d = new PageDataDictionary<dynamic>();
            Assert.IsNotNull(d.Data);
        }

        [TestMethod]
        public void AddTest() {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.IsTrue(d.Data.Contains(item));
        }

        [TestMethod]
        public void AddTest1() {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            object value = 1;
            d.Add(key, value);
            Assert.IsTrue(d.Data.ContainsKey(key));
            Assert.AreEqual(1, d.Data[key]);
            // Use uppercase string key
            Assert.AreEqual(1, d.Data["X"]);
        }

        [TestMethod]
        public void ClearTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 2);
            d.Clear();
            Assert.AreEqual(0, d.Data.Count);
        }

        [TestMethod]
        public void ContainsTest() {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 1);
            d.Add(item);
            Assert.IsTrue(d.Contains(item));
            var item2 = new KeyValuePair<object, object>("y", 2);
            Assert.IsFalse(d.Contains(item2));
        }

        [TestMethod]
        public void ContainsKeyTest() {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            Assert.IsFalse(d.ContainsKey(key));
            d.Add(key, 1);
            Assert.IsTrue(d.ContainsKey(key));
            Assert.IsTrue(d.ContainsKey("X"));
        }

        [TestMethod]
        public void CopyToTest() {
            var d = new PageDataDictionary<dynamic>();
            KeyValuePair<object, object>[] array = new KeyValuePair<object, object>[1];
            d.Add("x", 1);
            d.CopyTo(array, 0);
            Assert.AreEqual(new KeyValuePair<object, object>("x", 1), array[0]);
        }

        [TestMethod]
        public void GetEnumeratorTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            var e = d.GetEnumerator();
            e.MoveNext();
            Assert.AreEqual(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [TestMethod]
        public void RemoveTest() {
            var d = new PageDataDictionary<dynamic>();
            var key = "x";
            d.Add(key, 1);
            d.Remove(key);
            Assert.IsFalse(d.Data.ContainsKey(key));
        }

        [TestMethod]
        public void RemoveTest1() {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.IsTrue(d.Contains(item));
            d.Remove(item);
            Assert.IsFalse(d.Contains(item));
        }

        [TestMethod]
        [DeploymentItem("Microsoft.WebPages.dll")]
        public void GetEnumeratorTest1() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            var e = ((IEnumerable)d).GetEnumerator();
            e.MoveNext();
            Assert.AreEqual(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [TestMethod]
        public void TryGetValueTest() {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            d.Add(key, 1);
            object value = null;
            Assert.IsTrue(d.TryGetValue(key, out value));
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void CountTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            Assert.AreEqual(1, d.Count);
            d.Add("y", 2);
            Assert.AreEqual(2, d.Count);
        }

        [TestMethod]
        public void IsReadOnlyTest() {
            var d = new PageDataDictionary<dynamic>();
            Assert.AreEqual(d.Data.IsReadOnly, d.IsReadOnly);
        }

        [TestMethod]
        public void ItemTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.AreEqual(1, d["x"]);
            Assert.AreEqual(2, d["y"]);
        }

        [TestMethod]
        public void KeysTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.IsTrue(d.Keys.Contains("x"));
            Assert.IsTrue(d.Keys.Contains("y"));
            Assert.AreEqual(2, d.Keys.Count);
        }

        [TestMethod]
        public void ValuesTest() {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.IsTrue(d.Values.Contains(1));
            Assert.IsTrue(d.Values.Contains(2));
            Assert.AreEqual(2, d.Values.Count);
        }
    }
}
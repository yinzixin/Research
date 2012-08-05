using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class UrlDataTest {
        [TestMethod]
        public void UrlDataListConstructorTests() {
            Assert.IsNotNull(new UrlDataList(null));
            Assert.IsNotNull(new UrlDataList(string.Empty));
            Assert.IsNotNull(new UrlDataList("abc/foo"));
        }

        [TestMethod]
        public void AddTest() {
            var d = new UrlDataList(null);
            var item = "!!@#$#$";
            ExceptionAssert.Throws<NotSupportedException>(() => { d.Add(item); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void ClearTest() {
            var d = new UrlDataList(null);
            ExceptionAssert.Throws<NotSupportedException>(() => { d.Clear(); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void IndexOfTest() {
            var item = "!!@#$#$";
            var item2 = "13l53125";
            var d = new UrlDataList(item+"/"+item2);
            Assert.IsTrue(d.IndexOf(item) == 0);
            Assert.IsTrue(d.IndexOf(item2) == 1);
        }

        [TestMethod]
        public void InsertAtTest() {
            var d = new UrlDataList("x/y/z");
            ExceptionAssert.Throws<NotSupportedException>(() => { d.Insert(1, "a"); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void ContainsTest() {
            var item = "!!@#$#$";
            var d = new UrlDataList(item);
            Assert.IsTrue(d.Contains(item));
        }

        [TestMethod]
        public void CopyToTest() {
            var d = new UrlDataList("x/y");
            string[] array = new string[2];
            d.CopyTo(array, 0);
            Assert.AreEqual(array[0], d[0]);
            Assert.AreEqual(array[1], d[1]);
        }

        [TestMethod]
        public void GetEnumeratorTest() {
            var d = new UrlDataList("x");
            var e = d.GetEnumerator();
            e.MoveNext();
            Assert.AreEqual("x", e.Current);
        }

        [TestMethod]
        public void RemoveTest() {
            var d = new UrlDataList("x");
            ExceptionAssert.Throws<NotSupportedException>(() => { d.Remove("x"); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void RemoveAtTest() {
            var d = new UrlDataList("x/y");
            ExceptionAssert.Throws<NotSupportedException>(() => { d.RemoveAt(0); }, "The UrlData collection is read-only.");
        }

        [TestMethod]
        public void CountTest() {
            var d = new UrlDataList("x");
            Assert.AreEqual(1, d.Count);
        }

        [TestMethod]
        public void IsReadOnlyTest() {
            var d = new UrlDataList(null);
            Assert.AreEqual(true, d.IsReadOnly);
        }

        [TestMethod]
        public void ItemTest() {
            var d = new UrlDataList("x/y");
            Assert.AreEqual("x", d[0]);
            Assert.AreEqual("y", d[1]);
        }

    }
}
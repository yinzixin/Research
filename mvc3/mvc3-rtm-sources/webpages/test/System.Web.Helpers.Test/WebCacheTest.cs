using System.Collections.Generic;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Helpers.Test
{
    [TestClass]
    public class WebCacheTest
    {
        [TestMethod]
        public void GetReturnsExpectedValueTest()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_GetTest";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);

            var actual = WebCache.Get(key);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void RemoveRemovesRightValueTest() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_RemoveTest";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);
            
            var actual = WebCache.Remove(key);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void RemoveRemovesValueFromCacheTest() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_RemoveTest2";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);

            var removed = WebCache.Remove(key);
            
            Assert.AreEqual(null, WebCache.Get(key));
        }

        [TestMethod]
        public void SetWithAbsoluteExpirationDoesNotThrow()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "SetWithAbsoluteExpirationDoesNotThrow_SetTest"; 
            object expected = new object(); 
            int minutesToCache = 10; 
            bool slidingExpiration = false; 
            WebCache.Set(key, expected, minutesToCache, slidingExpiration);
            object actual = WebCache.Get(key);
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void CanSetWithSlidingExpiration() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_CanSetWithSlidingExpiration_SetTest";
            object expected = new object();
            
            WebCache.Set(key, expected, slidingExpiration: true);
            object actual = WebCache.Get(key);
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void SetWithSlidingExpirationForNegativeTime() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForNegativeTime_SetTest";
            object expected = new object();
            ExceptionAssert.ThrowsArgGreaterThan(() => WebCache.Set(key, expected, -1), "minutesToCache", "0");
        }

        [TestMethod]
        public void SetWithSlidingExpirationForZeroTime() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForZeroTime_SetTest";
            object expected = new object();
            ExceptionAssert.ThrowsArgGreaterThan(() => WebCache.Set(key, expected, 0), "minutesToCache", "0");
        }

        [TestMethod]
        public void SetWithSlidingExpirationForYear() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForYear_SetTest";
            object expected = new object();

            WebCache.Set(key, expected, 365 * 24 * 60, true);
            object actual = WebCache.Get(key);
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void SetWithSlidingExpirationForMoreThanYear() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForMoreThanYear_SetTest";
            object expected = new object();
            ExceptionAssert.ThrowsArgLessThanOrEqualTo(() =>  WebCache.Set(key, expected, 365 * 24 * 60 + 1, true), "minutesToCache", (365 * 24 * 60).ToString());
        }

        [TestMethod]
        public void SetWithAbsoluteExpirationForMoreThanYear() {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithAbsoluteExpirationForMoreThanYear_SetTest";
            object expected = new object();

            WebCache.Set(key, expected, 365 * 24 * 60, true);
            object actual = WebCache.Get(key);
            Assert.IsTrue(expected == actual);
        }
    }
}

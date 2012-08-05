using System.Threading;
using System.Web.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.WebPages.TestUtils;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class FileExistenceCacheTest {

        [TestMethod]
        public void ConstructorTest() {
            var ms = 1000;
            var cache = new FileExistenceCache(null);
            Assert.IsNull(cache.VirtualPathProvider);
            Assert.AreEqual(ms, cache.MilliSecondsBeforeReset);

            var vpp = new Mock<VirtualPathProvider>().Object;
            cache = new FileExistenceCache(vpp);
            Assert.AreEqual(vpp, cache.VirtualPathProvider);
            Assert.AreEqual(ms, cache.MilliSecondsBeforeReset);

            ms = 9999;
            cache = new FileExistenceCache(vpp, ms);
            Assert.AreEqual(vpp, cache.VirtualPathProvider);
            Assert.AreEqual(ms, cache.MilliSecondsBeforeReset);
        }

        [TestMethod]
        public void TimeExceededFalseTest() {
            var ms = 100000;
            var cache = new FileExistenceCache(null, ms);
            Assert.IsFalse(cache.TimeExceeded);
        }

        [TestMethod]
        public void TimeExceededTrueTest() {
            var ms = 5;
            var cache = new FileExistenceCache(null, ms);
            Thread.Sleep(300);
            Assert.IsTrue(cache.TimeExceeded);
        }

        [TestMethod]
        public void ResetTest() {
            var cache = new FileExistenceCache(null);
            var cacheInternal = cache.CacheInternal;
            cache.Reset();
            Assert.AreNotEqual(cacheInternal, cache.CacheInternal);
        }

        [TestMethod]
        public void FileExistsTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var path = "~/index.cshtml";
                Utils.SetupVirtualPathInAppDomain(path, "");
                var cache = FileExistenceCache.GetInstance();
                Assert.IsTrue(cache.FileExists(path));
                Assert.IsFalse(cache.FileExists("~/test.cshtml"));
            });
        }

        [TestMethod]
        public void FileExistsVppLaterTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var path = "~/index.cshtml";
                // Set up the VPP only after GetInstance is called
                var cache = FileExistenceCache.GetInstance();
                Utils.SetupVirtualPathInAppDomain(path, "");
                Assert.IsTrue(cache.FileExists(path));
                Assert.IsFalse(cache.FileExists("~/test.cshtml"));
            });
        }

        [TestMethod]
        public void FileExistsTimeExceededTest() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                var path = "~/index.cshtml";
                Utils.SetupVirtualPathInAppDomain(path, "");
                var cache = FileExistenceCache.GetInstance();
                var cacheInternal = cache.CacheInternal;
                cache.MilliSecondsBeforeReset = 5;
                Thread.Sleep(300);
                Assert.IsTrue(cache.FileExists(path));
                Assert.IsFalse(cache.FileExists("~/test.cshtml"));
                Assert.AreNotEqual(cacheInternal, cache.CacheInternal);
            });
        }

    }
}

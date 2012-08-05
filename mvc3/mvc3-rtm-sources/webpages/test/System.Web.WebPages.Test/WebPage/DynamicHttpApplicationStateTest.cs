using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.Resources;
using System.Web.WebPages.TestUtils;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class DynamicHttpApplicationStateTest {
        [TestMethod]
        public void DynamicTest() {
            HttpApplicationStateBase appState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
            dynamic d = new DynamicHttpApplicationState(appState);
            d["x"] = "y";
            Assert.AreEqual("y", d.x);
            Assert.AreEqual("y", d[0]);
            d.a = "b";
            Assert.AreEqual("b", d["a"]);
            d.Foo = "bar";
            Assert.AreEqual("bar", d.Foo);
            Assert.AreEqual(null, d.XYZ);
            Assert.AreEqual(null, d["xyz"]);
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => {
                var x = d[5];
            });
        }

        [TestMethod]
        public void InvalidNumberOfIndexes() {
            ExceptionAssert.Throws<ArgumentException>(() => {
                HttpApplicationStateBase appState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
                dynamic d = new DynamicHttpApplicationState(appState);
                d[1, 2] = 3;
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);

            ExceptionAssert.Throws<ArgumentException>(() => {
                HttpApplicationStateBase appState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
                dynamic d = new DynamicHttpApplicationState(appState);
                var x = d[1, 2];
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
        }

        [TestMethod]
        public void InvalidTypeWhenSetting() {
            ExceptionAssert.Throws<ArgumentException>(() => {
                HttpApplicationStateBase appState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
                dynamic d = new DynamicHttpApplicationState(appState);
                d[new object()] = 3;
            }, WebPageResources.DynamicHttpApplicationState_UseOnlyStringToSet);
        }

        [TestMethod]
        public void InvalidTypeWhenGetting() {
            ExceptionAssert.Throws<ArgumentException>(() => {
                HttpApplicationStateBase appState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);
                dynamic d = new DynamicHttpApplicationState(appState);
                var x = d[new object()];
            }, WebPageResources.DynamicHttpApplicationState_UseOnlyStringOrIntToGet);
        }
    }
}
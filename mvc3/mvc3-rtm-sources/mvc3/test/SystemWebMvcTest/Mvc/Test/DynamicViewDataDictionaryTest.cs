using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.Mvc.Test {
    [TestClass]
    public class DynamicViewDataDictionaryTest {
        [TestMethod]
        public void Get_OnExistingProperty_ReturnsValue() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary() {
                { "Prop", "Value" }
            };
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            object value = dynamicVD.Prop;

            // Assert
            Assert.IsInstanceOfType(value, typeof(string));
            Assert.AreEqual("Value", value);
        }

        [TestMethod]
        public void Get_OnNonExistentProperty_ReturnsNull() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            object value = dynamicVD.Prop;

            // Assert
            Assert.IsNull(value);
        }

        [TestMethod]
        public void Set_OnExistingProperty_OverridesValue() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary() {
                { "Prop", "Value" }
            };
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            dynamicVD.Prop = "NewValue";

            // Assert
            Assert.AreEqual("NewValue", dynamicVD.Prop);
            Assert.AreEqual("NewValue", vd["Prop"]);
        }

        [TestMethod]
        public void Set_OnNonExistentProperty_SetsValue() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            dynamicVD.Prop = "NewValue";

            // Assert
            Assert.AreEqual("NewValue", dynamicVD.Prop);
            Assert.AreEqual("NewValue", vd["Prop"]);
        }

        [TestMethod]
        public void TryGetMember_OnExistingProperty_ReturnsValueAndSucceeds() {
            // Arrange
            object result = null;
            ViewDataDictionary vd = new ViewDataDictionary() {
                { "Prop", "Value" }
            };
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<GetMemberBinder> binderMock = new Mock<GetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TryGetMember(binderMock.Object, out result);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("Value", result);
        }

        [TestMethod]
        public void TryGetMember_OnNonExistentProperty_ReturnsNullAndSucceeds() {
            // Arrange
            object result = null;
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<GetMemberBinder> binderMock = new Mock<GetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TryGetMember(binderMock.Object, out result);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TrySetMember_OnExistingProperty_OverridesValueAndSucceeds() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary() {
                { "Prop", "Value" }
            };
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<SetMemberBinder> binderMock = new Mock<SetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TrySetMember(binderMock.Object, "NewValue");

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("NewValue", ((dynamic)dynamicVD).Prop);
            Assert.AreEqual("NewValue", vd["Prop"]);
        }

        [TestMethod]
        public void TrySetMember_OnNonExistentProperty_SetsValueAndSucceeds() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<SetMemberBinder> binderMock = new Mock<SetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TrySetMember(binderMock.Object, "NewValue");

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("NewValue", ((dynamic)dynamicVD).Prop);
            Assert.AreEqual("NewValue", vd["Prop"]);
        }

        [TestMethod]
        public void GetDynamicMemberNames_ReturnsEmptyListForEmptyViewDataDictionary() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetDynamicMemberNames_ReturnsKeyNamesForFilledViewDataDictionary() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary() {
                { "Prop1", 1 },
                { "Prop2", 2 }
            };
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            CollectionAssert.AreEqual(new[] { "Prop1", "Prop2" }, result.OrderBy(s => s).ToList());
        }

        [TestMethod]
        public void GetDynamicMemberNames_ReflectsChangesToUnderlyingViewDataDictionary() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            vd["OldProp"] = 123;
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);
            vd["NewProp"] = "somevalue";

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            CollectionAssert.AreEqual(new[] { "NewProp", "OldProp" }, result.OrderBy(s => s).ToList());
        }

        [TestMethod]
        public void GetDynamicMemberNames_ReflectsChangesToDynamicObject() {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            vd["OldProp"] = 123;
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);
            ((dynamic)dvd).NewProp = "foo";

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            CollectionAssert.AreEqual(new[] { "NewProp", "OldProp" }, result.OrderBy(s => s).ToList());
        }
    }
}

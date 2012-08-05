namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ActionDescriptorTest {

        [TestMethod]
        public void ExtractParameterOrDefaultFromDictionary_ReturnsDefaultParameterValueIfMismatch() {
            // Arrange
            Dictionary<string, object> dictionary = new Dictionary<string, object>() {
                { "stringParameterWithDefaultValue", 42 }
            };

            // Act
            object value = ActionDescriptor.ExtractParameterOrDefaultFromDictionary(ParameterExtractionController.StringParameterWithDefaultValue, dictionary);

            // Assert
            Assert.AreEqual("hello", value);
        }

        [TestMethod]
        public void ExtractParameterOrDefaultFromDictionary_ReturnsDefaultTypeValueIfNoMatchAndNoDefaultParameterValue() {
            // Arrange
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            // Act
            object value = ActionDescriptor.ExtractParameterOrDefaultFromDictionary(ParameterExtractionController.IntParameter, dictionary);

            // Assert
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void ExtractParameterOrDefaultFromDictionary_ReturnsDictionaryValueIfTypeMatch() {
            // Arrange
            Dictionary<string, object> dictionary = new Dictionary<string, object>() {
                { "stringParameterNoDefaultValue", "someValue" }
            };

            // Act
            object value = ActionDescriptor.ExtractParameterOrDefaultFromDictionary(ParameterExtractionController.StringParameterNoDefaultValue, dictionary);

            // Assert
            Assert.AreEqual("someValue", value);
        }

        [TestMethod]
        public void GetCustomAttributesReturnsEmptyArrayOfAttributeType() {
            // Arrange
            ActionDescriptor ad = GetActionDescriptor();

            // Act
            ObsoleteAttribute[] attrs = (ObsoleteAttribute[])ad.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.AreEqual(0, attrs.Length);
        }

        [TestMethod]
        public void GetCustomAttributesThrowsIfAttributeTypeIsNull() {
            // Arrange
            ActionDescriptor ad = GetActionDescriptor();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    ad.GetCustomAttributes(null /* attributeType */, true);
                }, "attributeType");
        }

        [TestMethod]
        public void GetCustomAttributesWithoutAttributeTypeCallsGetCustomAttributesWithAttributeType() {
            // Arrange
            object[] expected = new object[0];
            Mock<ActionDescriptor> mockDescriptor = new Mock<ActionDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(object), true)).Returns(expected);
            ActionDescriptor ad = mockDescriptor.Object;

            // Act
            object[] returned = ad.GetCustomAttributes(true /* inherit */);

            // Assert
            Assert.AreSame(expected, returned);
        }

        [TestMethod]
        public void GetFilterAttributes_CallsGetCustomAttributes() {
            // Arrange
            var mockDescriptor = new Mock<ActionDescriptor>();
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(FilterAttribute), true)).Returns(new object[] { new Mock<FilterAttribute>().Object }).Verifiable();

            // Act
            var result = mockDescriptor.Object.GetFilterAttributes(true).ToList();
            
            // Assert
            mockDescriptor.Verify();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSelectorsReturnsEmptyCollection() {
            // Arrange
            ActionDescriptor ad = GetActionDescriptor();

            // Act
            ICollection<ActionSelector> selectors = ad.GetSelectors();

            // Assert
            Assert.IsInstanceOfType(selectors, typeof(ActionSelector[]), "Should return an immutable collection.");
            Assert.AreEqual(0, selectors.Count);
        }

        [TestMethod]
        public void IsDefinedReturnsFalse() {
            // Arrange
            ActionDescriptor ad = GetActionDescriptor();

            // Act
            bool isDefined = ad.IsDefined(typeof(object), true);

            // Assert
            Assert.IsFalse(isDefined);
        }

        [TestMethod]
        public void IsDefinedThrowsIfAttributeTypeIsNull() {
            // Arrange
            ActionDescriptor ad = GetActionDescriptor();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    ad.IsDefined(null /* attributeType */, true);
                }, "attributeType");
        }

        [TestMethod]
        public void UniqueId_SameTypeControllerDescriptorAndActionName_SameID() {
            // Arrange
            var controllerDescriptor = new Mock<ControllerDescriptor>().Object;

            var descriptor1 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor1.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            descriptor1.SetupGet(d => d.ActionName).Returns("Action1");

            var descriptor2 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor2.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            descriptor2.SetupGet(d => d.ActionName).Returns("Action1");

            // Act
            var id1 = descriptor1.Object.UniqueId;
            var id2 = descriptor2.Object.UniqueId;

            // Assert
            Assert.AreEqual(id1, id2);
        }

        [TestMethod]
        public void UniqueId_VariesWithActionName() {
            // Arrange
            var controllerDescriptor = new Mock<ControllerDescriptor>().Object;

            var descriptor1 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor1.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            descriptor1.SetupGet(d => d.ActionName).Returns("Action1");

            var descriptor2 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor2.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            descriptor2.SetupGet(d => d.ActionName).Returns("Action2");

            // Act
            var id1 = descriptor1.Object.UniqueId;
            var id2 = descriptor2.Object.UniqueId;

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        [TestMethod]
        public void UniqueId_VariesWithControllerDescriptorsUniqueId() {
            // Arrange
            var controllerDescriptor1 = new Mock<ControllerDescriptor>();
            controllerDescriptor1.SetupGet(cd => cd.UniqueId).Returns("1");
            var descriptor1 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor1.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor1.Object);
            descriptor1.SetupGet(d => d.ActionName).Returns("Action1");

            var controllerDescriptor2 = new Mock<ControllerDescriptor>();
            controllerDescriptor2.SetupGet(cd => cd.UniqueId).Returns("2");
            var descriptor2 = new Mock<ActionDescriptor> { CallBase = true };
            descriptor2.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor2.Object);
            descriptor2.SetupGet(d => d.ActionName).Returns("Action1");

            // Act
            var id1 = descriptor1.Object.UniqueId;
            var id2 = descriptor2.Object.UniqueId;

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        [TestMethod]
        public void UniqueId_VariesWithActionDescriptorType() {
            // Arrange
            var descriptor1 = new BaseDescriptor();
            var descriptor2 = new DerivedDescriptor();

            // Act
            var id1 = descriptor1.UniqueId;
            var id2 = descriptor2.UniqueId;

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        class BaseDescriptor : ActionDescriptor {
            static ControllerDescriptor controllerDescriptor = new Mock<ControllerDescriptor>().Object;

            public override string ActionName {
                get { return "ActionName"; }
            }

            public override ControllerDescriptor ControllerDescriptor {
                get { return controllerDescriptor; }
            }

            public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters) {
                throw new NotImplementedException();
            }

            public override ParameterDescriptor[] GetParameters() {
                throw new NotImplementedException();
            }
        }

        class DerivedDescriptor : BaseDescriptor { }

        private static ActionDescriptor GetActionDescriptor() {
            Mock<ActionDescriptor> mockDescriptor = new Mock<ActionDescriptor>() { CallBase = true };
            return mockDescriptor.Object;
        }

        private class ParameterExtractionController : Controller {
            public static readonly ParameterInfo IntParameter = typeof(ParameterExtractionController).GetMethod("SomeMethod").GetParameters()[0];
            public static readonly ParameterInfo StringParameterNoDefaultValue = typeof(ParameterExtractionController).GetMethod("SomeMethod").GetParameters()[1];
            public static readonly ParameterInfo StringParameterWithDefaultValue = typeof(ParameterExtractionController).GetMethod("SomeMethod").GetParameters()[2];

            public void SomeMethod(int intParameter, string stringParameterNoDefaultValue, [DefaultValue("hello")] string stringParameterWithDefaultValue) {
            }
        }

    }
}

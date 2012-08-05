namespace System.Web.Mvc.Test {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ControllerDescriptorTest {

        [TestMethod]
        public void ControllerNamePropertyReturnsControllerTypeName() {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor(typeof(object));

            // Act
            string name = cd.ControllerName;

            // Assert
            Assert.AreEqual("Object", name);
        }

        [TestMethod]
        public void ControllerNamePropertyReturnsControllerTypeNameWithoutControllerSuffix() {
            // Arrange
            Mock<Type> mockType = new Mock<Type>();
            mockType.Setup(t => t.Name).Returns("somecontroller");
            ControllerDescriptor cd = GetControllerDescriptor(mockType.Object);

            // Act
            string name = cd.ControllerName;

            // Assert
            Assert.AreEqual("some", name);
        }

        [TestMethod]
        public void GetCustomAttributesReturnsEmptyArrayOfAttributeType() {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act
            ObsoleteAttribute[] attrs = (ObsoleteAttribute[])cd.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.AreEqual(0, attrs.Length);
        }

        [TestMethod]
        public void GetCustomAttributesThrowsIfAttributeTypeIsNull() {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    cd.GetCustomAttributes(null /* attributeType */, true);
                }, "attributeType");
        }

        [TestMethod]
        public void GetCustomAttributesWithoutAttributeTypeCallsGetCustomAttributesWithAttributeType() {
            // Arrange
            object[] expected = new object[0];
            Mock<ControllerDescriptor> mockDescriptor = new Mock<ControllerDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(object), true)).Returns(expected);
            ControllerDescriptor cd = mockDescriptor.Object;

            // Act
            object[] returned = cd.GetCustomAttributes(true /* inherit */);

            // Assert
            Assert.AreSame(expected, returned);
        }

        [TestMethod]
        public void GetFilterAttributes_CallsGetCustomAttributes() {
            // Arrange
            var mockDescriptor = new Mock<ControllerDescriptor>();
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(FilterAttribute), true)).Returns(new object[] { new Mock<FilterAttribute>().Object }).Verifiable();

            // Act
            var result = mockDescriptor.Object.GetFilterAttributes(true).ToList();

            // Assert
            mockDescriptor.Verify();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDefinedReturnsFalse() {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act
            bool isDefined = cd.IsDefined(typeof(object), true);

            // Assert
            Assert.IsFalse(isDefined);
        }

        [TestMethod]
        public void IsDefinedThrowsIfAttributeTypeIsNull() {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    cd.IsDefined(null /* attributeType */, true);
                }, "attributeType");
        }

        private static ControllerDescriptor GetControllerDescriptor() {
            return GetControllerDescriptor(null);
        }

        private static ControllerDescriptor GetControllerDescriptor(Type controllerType) {
            Mock<ControllerDescriptor> mockDescriptor = new Mock<ControllerDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.ControllerType).Returns(controllerType);
            return mockDescriptor.Object;
        }

    }
}

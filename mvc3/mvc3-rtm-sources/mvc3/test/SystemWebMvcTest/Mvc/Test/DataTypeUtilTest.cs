namespace System.Web.Mvc.Test {
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DataTypeUtilTest {
        private class DerivedDataTypeAttribute : DataTypeAttribute {
            public DerivedDataTypeAttribute(DataType dataType)
                : base(dataType) {
            }

            public override string GetDataTypeName() {
                return "DerivedTypeName";
            }
        }

        [TestMethod]
        public void VirtualDataTypeNameCallsAttributeGetDataTypeName() {
            // Arrange
            DataTypeAttribute derivedAttr = new DerivedDataTypeAttribute(DataType.Html);
            string expectedTypeName = derivedAttr.GetDataTypeName();

            // Act
            string actualTypeName = DataTypeUtil.ToDataTypeName(derivedAttr);

            // Assert
            Assert.AreEqual(expectedTypeName, actualTypeName);
        }

        [TestMethod]
        public void DataTypeAttributeDoesNotCallAttributeGetDataTypeName() {
            // Arrange
            Func<DataTypeAttribute, Boolean> isDataTypeAttribute = t => (t as DataTypeAttribute) != null;

            foreach (DataType dataTypeValue in Enum.GetValues(typeof(DataType))) {
                if (dataTypeValue != DataType.Custom) {
                    Mock<DataTypeAttribute> dataType = new Mock<DataTypeAttribute>(dataTypeValue);

                    // Act
                    string actualTypeName = DataTypeUtil.ToDataTypeName(dataType.Object, dta => dta as DataTypeAttribute != null);

                    // Assert
                    Assert.AreEqual(dataTypeValue.ToString(), actualTypeName);
                    dataType.Verify(dt => dt.GetDataTypeName(), Times.Never());
                }
            }
        }

        [TestMethod]
        public void CustomDataTypeNameCallsAttributeGetDataTypeName() {
            // Arrange
            Func<DataTypeAttribute, Boolean> isDataTypeAttribute = t => (t as DataTypeAttribute) != null;

            Mock<DataTypeAttribute> customDataType = new Mock<DataTypeAttribute>(DataType.Custom);
            customDataType.Setup(c => c.GetDataTypeName()).Returns("CustomTypeName").Verifiable();

            // Act
            string actualTypeName = DataTypeUtil.ToDataTypeName(customDataType.Object);

            // Assert
            customDataType.Verify(c => c.GetDataTypeName(), Times.Once());
            Assert.AreEqual("CustomTypeName", actualTypeName);
        }
    }
}

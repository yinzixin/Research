namespace WebMatrix.Data.Test {
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages.TestUtils;
    using Moq;

    [TestClass]
    public class DynamicRecordTest {
        [TestMethod]
        public void GetFieldValueByNameAccessesUnderlyingRecordForValue() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.SetupGet(m => m["B"]).Returns(2);

            dynamic record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.AreEqual(1, record.A);
            Assert.AreEqual(2, record.B);
        }

        [TestMethod]
        public void GetFieldValueByIndexAccessesUnderlyingRecordForValue() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m[0]).Returns(1);
            mockRecord.SetupGet(m => m[1]).Returns(2);

            dynamic record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.AreEqual(1, record[0]);
            Assert.AreEqual(2, record[1]);
        }

        [TestMethod]
        public void GetFieldValueByNameReturnsNullIfValueIsDbNull() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(DBNull.Value);

            dynamic record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            Assert.IsNull(record.A);
        }

        [TestMethod]
        public void GetFieldValueByIndexReturnsNullIfValueIsDbNull() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m[0]).Returns(DBNull.Value);

            dynamic record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            Assert.IsNull(record[0]);
        }

        [TestMethod]
        public void GetInvalidFieldValueThrows() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            dynamic record = new DynamicRecord(Enumerable.Empty<string>(), mockRecord.Object);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => { var value = record.C; }, "Invalid column name \"C\".");
        }

        [TestMethod]
        public void VerfiyCustomTypeDescriptorMethods() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.SetupGet(m => m["B"]).Returns(2);

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.AreEqual(AttributeCollection.Empty, record.GetAttributes());
            Assert.IsNull(record.GetClassName());
            Assert.IsNull(record.GetConverter());
            Assert.IsNull(record.GetDefaultEvent());
            Assert.IsNull(record.GetComponentName());
            Assert.IsNull(record.GetDefaultProperty());
            Assert.IsNull(record.GetEditor(null));
            Assert.AreEqual(EventDescriptorCollection.Empty, record.GetEvents());
            Assert.AreEqual(EventDescriptorCollection.Empty, record.GetEvents(null));
            Assert.AreSame(record, record.GetPropertyOwner(null));
            Assert.AreEqual(2, record.GetProperties().Count);
            Assert.AreEqual(2, record.GetProperties(null).Count);
            Assert.IsNotNull(record.GetProperties()["A"]);
            Assert.IsNotNull(record.GetProperties()["B"]);
        }

        [TestMethod]
        public void VerifyPropertyDescriptorProperties() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.Setup(m => m.GetOrdinal("A")).Returns(0);
            mockRecord.Setup(m => m.GetFieldType(0)).Returns(typeof(string));

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            var aDescriptor = record.GetProperties().Find("A", ignoreCase: false);

            Assert.IsNotNull(aDescriptor);
            Assert.IsNull(aDescriptor.GetValue(null));
            Assert.AreEqual(1, aDescriptor.GetValue(record));
            Assert.IsTrue(aDescriptor.IsReadOnly);
            Assert.AreEqual(typeof(string), aDescriptor.PropertyType);
            Assert.AreEqual(typeof(DynamicRecord), aDescriptor.ComponentType);
            Assert.IsFalse(aDescriptor.ShouldSerializeValue(record));
            Assert.IsFalse(aDescriptor.CanResetValue(record));
        }

        [TestMethod]
        public void SetAndResetValueOnPropertyDescriptorThrows() {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            var aDescriptor = record.GetProperties().Find("A", ignoreCase: false);
            Assert.IsNotNull(aDescriptor);
            ExceptionAssert.Throws<InvalidOperationException>(() => aDescriptor.SetValue(record, 1), "Unable to modify the value of column \"A\" because the record is read only.");
            ExceptionAssert.Throws<InvalidOperationException>(() => aDescriptor.ResetValue(record), "Unable to modify the value of column \"A\" because the record is read only.");
        }
    }
}

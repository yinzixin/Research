namespace System.Web.Mvc.Test {
    using System;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DescriptorUtilTest {
        [TestMethod]
        public void CreateUniqueId_FromIUniquelyIdentifiable() {
            // Arrange
            CustomUniquelyIdentifiable custom = new CustomUniquelyIdentifiable("hello-world");

            // Act
            string retVal = DescriptorUtil.CreateUniqueId(custom);

            // Assert
            Assert.AreEqual("[11]hello-world", retVal);
        }

        [TestMethod]
        public void CreateUniqueId_FromMemberInfo() {
            // Arrange
            string moduleVersionId = typeof(DescriptorUtilTest).Module.ModuleVersionId.ToString();
            string metadataToken = typeof(DescriptorUtilTest).MetadataToken.ToString();
            string expected = String.Format("[{0}]{1}[{2}]{3}", moduleVersionId.Length, moduleVersionId, metadataToken.Length, metadataToken);

            // Act
            string retVal = DescriptorUtil.CreateUniqueId(typeof(DescriptorUtilTest));

            // Assert
            Assert.AreEqual(expected, retVal);
        }

        [TestMethod]
        public void CreateUniqueId_FromSimpleTypes() {
            // Act
            string retVal = DescriptorUtil.CreateUniqueId("foo", null, 12345);

            // Assert
            Assert.AreEqual("[3]foo[-1][5]12345", retVal);
        }

        private sealed class CustomUniquelyIdentifiable : IUniquelyIdentifiable {
            public CustomUniquelyIdentifiable(string uniqueId) {
                UniqueId = uniqueId;
            }

            public string UniqueId { get; private set; }
        }
    }
}
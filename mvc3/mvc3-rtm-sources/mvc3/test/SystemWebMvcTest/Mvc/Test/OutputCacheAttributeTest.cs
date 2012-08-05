namespace System.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using System.Web.UI;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class OutputCacheAttributeTest {

        [TestMethod]
        public void CacheProfileProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "CacheProfile", String.Empty);
        }

        [TestMethod]
        public void CacheSettingsProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute() {
                CacheProfile = "SomeProfile",
                Duration = 50,
                Location = OutputCacheLocation.Downstream,
                NoStore = true,
                SqlDependency = "SomeSqlDependency",
                VaryByContentEncoding = "SomeContentEncoding",
                VaryByCustom = "SomeCustom",
                VaryByHeader = "SomeHeader",
                VaryByParam = "SomeParam",
            };

            // Act
            OutputCacheParameters cacheSettings = attr.CacheSettings;

            // Assert
            Assert.AreEqual("SomeProfile", cacheSettings.CacheProfile);
            Assert.AreEqual(50, cacheSettings.Duration);
            Assert.AreEqual(OutputCacheLocation.Downstream, cacheSettings.Location);
            Assert.AreEqual(true, cacheSettings.NoStore);
            Assert.AreEqual("SomeSqlDependency", cacheSettings.SqlDependency);
            Assert.AreEqual("SomeContentEncoding", cacheSettings.VaryByContentEncoding);
            Assert.AreEqual("SomeCustom", cacheSettings.VaryByCustom);
            Assert.AreEqual("SomeHeader", cacheSettings.VaryByHeader);
            Assert.AreEqual("SomeParam", cacheSettings.VaryByParam);
        }

        [TestMethod]
        public void DurationProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestInt32Property(attr, "Duration", 10, 20);
        }

        [TestMethod]
        public void LocationProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestPropertyValue(attr, "Location", OutputCacheLocation.ServerAndClient);
        }

        [TestMethod]
        public void NoStoreProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestBooleanProperty(attr, "NoStore", false /* initialValue */, false /* testDefaultValue */);
        }

        [TestMethod]
        public void OnResultExecutingThrowsIfFilterContextIsNull() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    attr.OnResultExecuting(null);
                }, "filterContext");
        }

        [TestMethod]
        public void SqlDependencyProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "SqlDependency", String.Empty);
        }

        [TestMethod]
        public void VaryByContentEncodingProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByContentEncoding", String.Empty);
        }

        [TestMethod]
        public void VaryByCustomProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByCustom", String.Empty);
        }

        [TestMethod]
        public void VaryByHeaderProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByHeader", String.Empty);
        }

        [TestMethod]
        public void VaryByParamProperty() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByParam", "*");
        }

        [TestMethod]
        public void OutputCacheDoesNotExecuteIfInChildAction() {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            Mock<ResultExecutingContext> context = new Mock<ResultExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act
            attr.OnResultExecuting(context.Object);

            // Assert
            context.Verify();
            context.Verify(c => c.Result, Times.Never());
        }

        // GetChildActionUniqueId

        [TestMethod]
        public void GetChildActionUniqueId_ReturnsRepeatableValueForIdenticalContext() {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context = new MockActionExecutingContext();

            // Act
            string result1 = attr.GetChildActionUniqueId(context.Object);
            string result2 = attr.GetChildActionUniqueId(context.Object);

            // Assert
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void GetChildActionUniqueId_VariesByActionDescriptorsUniqueId() {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.Setup(c => c.ActionDescriptor.UniqueId).Returns("1");
            var context2 = new MockActionExecutingContext();
            context2.Setup(c => c.ActionDescriptor.UniqueId).Returns("2");

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void GetChildActionUniqueId_VariesByCustom() {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByCustom = "foo" };
            var context1 = new MockActionExecutingContext();
            context1.Setup(c => c.HttpContext.ApplicationInstance.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("1");
            var context2 = new MockActionExecutingContext();
            context2.Setup(c => c.HttpContext.ApplicationInstance.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("2");

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void GetChildActionUniqueId_VariesByActionParameters_AllParametersByDefault() {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void GetChildActionUniqueId_DoesNotVaryByActionParametersWhenVaryByParamIsNone() {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByParam = "none" };
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters() {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByParam = "bar" };
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.AreEqual(result1, result2);
        }

        class MockActionExecutingContext : Mock<ActionExecutingContext> {
            public Dictionary<string, object> ActionParameters = new Dictionary<string, object>();

            public MockActionExecutingContext() {
                Setup(c => c.ActionDescriptor.UniqueId).Returns("abc123");
                Setup(c => c.ActionParameters).Returns(() => ActionParameters);
            }
        }
    }
}

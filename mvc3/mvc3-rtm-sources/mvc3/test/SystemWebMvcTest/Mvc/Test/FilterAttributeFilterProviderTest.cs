namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc.Async;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterAttributeFilterProviderTest {
        [TestMethod]
        public void GetFilters_WithNullController_ReturnsEmptyList() {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var provider = new FilterAttributeFilterProvider();

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [MyFilterAttribute(Order = 2112)]
        private class ControllerWithTypeAttribute : Controller {
        }

        [TestMethod]
        public void GetFilters_IncludesAttributesOnControllerType() {
            // Arrange
            var context = new ControllerContext { Controller = new ControllerWithTypeAttribute() };
            var controllerDescriptorMock = new Mock<ControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetCustomAttributes(typeof(FilterAttribute), true))
                                    .Returns(new FilterAttribute[] { new MyFilterAttribute { Order = 2112 } });
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);
            var provider = new FilterAttributeFilterProvider();

            // Act
            Filter filter = provider.GetFilters(context, actionDescriptorMock.Object).Single();

            // Assert
            MyFilterAttribute attrib = filter.Instance as MyFilterAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual(FilterScope.Controller, filter.Scope);
            Assert.AreEqual(2112, filter.Order);
        }

        private class ControllerWithActionAttribute : Controller {
            [MyFilterAttribute(Order = 1234)]
            public ActionResult MyActionMethod() {
                return null;
            }
        }

        [TestMethod]
        public void GetFilters_IncludesAttributesOnActionMethod() {
            // Arrange
            var context = new ControllerContext { Controller = new ControllerWithActionAttribute() };
            var controllerDescriptor = new ReflectedControllerDescriptor(context.Controller.GetType());
            var action = context.Controller.GetType().GetMethod("MyActionMethod");
            var actionDescriptor = new ReflectedActionDescriptor(action, "MyActionMethod", controllerDescriptor);
            var provider = new FilterAttributeFilterProvider();

            // Act
            Filter filter = provider.GetFilters(context, actionDescriptor).Single();

            // Assert
            MyFilterAttribute attrib = filter.Instance as MyFilterAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual(FilterScope.Action, filter.Scope);
            Assert.AreEqual(1234, filter.Order);
        }

        private abstract class BaseController : Controller {
            public ActionResult MyActionMethod() {
                return null;
            }
        }

        [MyFilterAttribute]
        private class DerivedController : BaseController {
        }

        [TestMethod]
        public void GetFilters_IncludesTypeAttributesFromDerivedTypeWhenMethodIsOnBaseClass() {    // DDB #208062
            // Arrange
            var context = new ControllerContext { Controller = new DerivedController() };
            var controllerDescriptor = new ReflectedControllerDescriptor(context.Controller.GetType());
            var action = context.Controller.GetType().GetMethod("MyActionMethod");
            var actionDescriptor = new ReflectedActionDescriptor(action, "MyActionMethod", controllerDescriptor);
            var provider = new FilterAttributeFilterProvider();

            // Act
            IEnumerable<Filter> filters = provider.GetFilters(context, actionDescriptor);

            // Assert
            Assert.IsNotNull(filters.Select(f => f.Instance).Cast<MyFilterAttribute>().Single());
        }

        private class MyFilterAttribute : FilterAttribute {
        }

        [TestMethod]
        public void GetFilters_RetrievesCachedAttributesByDefault() {
            // Arrange
            var provider = new FilterAttributeFilterProvider();
            var context = new ControllerContext { Controller = new DerivedController()};
            var controllerDescriptorMock = new Mock<TestableControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetFilterAttributesPublic(true)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            var actionDescriptorMock = new Mock<TestableActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.GetFilterAttributesPublic(true)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);

            // Act
            var result = provider.GetFilters(context, actionDescriptorMock.Object);

            // Assert
            controllerDescriptorMock.Verify();
            actionDescriptorMock.Verify();
        }

        [TestMethod]
        public void GetFilters_RetrievesNonCachedAttributesWhenConfiguredNotTo() {
            // Arrange
            var provider = new FilterAttributeFilterProvider(false);
            var context = new ControllerContext { Controller = new DerivedController() };
            var controllerDescriptorMock = new Mock<TestableControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetFilterAttributesPublic(false)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            var actionDescriptorMock = new Mock<TestableActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.GetFilterAttributesPublic(false)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);

            // Act
            var result = provider.GetFilters(context, actionDescriptorMock.Object);

            // Assert
            controllerDescriptorMock.Verify();
            actionDescriptorMock.Verify();
        }

        public abstract class TestableControllerDescriptor : ControllerDescriptor {

            public abstract IEnumerable<FilterAttribute> GetFilterAttributesPublic(bool useCache);

            internal override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache) {
                return GetFilterAttributesPublic(useCache);
            }
        }

        public abstract class TestableActionDescriptor : ActionDescriptor {

            public abstract IEnumerable<FilterAttribute> GetFilterAttributesPublic(bool useCache);

            internal override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache) {
                return GetFilterAttributesPublic(useCache);
            }
        }
    }
}
namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterInfoTest {
        [TestMethod]
        public void Constructor_Default() {
            // Arrange + Act
            FilterInfo filterInfo = new FilterInfo();

            // Assert
            Assert.AreEqual(0, filterInfo.ActionFilters.Count);
            Assert.AreEqual(0, filterInfo.AuthorizationFilters.Count);
            Assert.AreEqual(0, filterInfo.ExceptionFilters.Count);
            Assert.AreEqual(0, filterInfo.ResultFilters.Count);
        }

        [TestMethod]
        public void Constructor_PopulatesFilterCollections() {
            // Arrange
            Mock<IActionFilter> actionFilterMock = new Mock<IActionFilter>();
            Mock<IAuthorizationFilter> authorizationFilterMock = new Mock<IAuthorizationFilter>();
            Mock<IExceptionFilter> exceptionFilterMock = new Mock<IExceptionFilter>();
            Mock<IResultFilter> resultFilterMock = new Mock<IResultFilter>();

            List<Filter> filters = new List<Filter>() {
                CreateFilter(actionFilterMock),
                CreateFilter(authorizationFilterMock),
                CreateFilter(exceptionFilterMock),
                CreateFilter(resultFilterMock),
            };

            // Act
            FilterInfo filterInfo = new FilterInfo(filters);

            // Assert
            Assert.AreEqual(actionFilterMock.Object, filterInfo.ActionFilters.SingleOrDefault());
            Assert.AreEqual(authorizationFilterMock.Object, filterInfo.AuthorizationFilters.SingleOrDefault());
            Assert.AreEqual(exceptionFilterMock.Object, filterInfo.ExceptionFilters.SingleOrDefault());
            Assert.AreEqual(resultFilterMock.Object, filterInfo.ResultFilters.SingleOrDefault());
        }

        [TestMethod]
        public void Constructor_IteratesOverFiltersOnlyOnce() {
            // Arrange
            var filtersMock = new Mock<IEnumerable<Filter>>();
            filtersMock.Setup(f => f.GetEnumerator()).Returns(new List<Filter>().GetEnumerator());

            // Act
            FilterInfo filterInfo = new FilterInfo(filtersMock.Object);

            // Assert
            filtersMock.Verify(f => f.GetEnumerator(), Times.Once());
        }

        private static Filter CreateFilter(Mock instanceMock) {
            return new Filter(instanceMock.Object, FilterScope.Global, null);
        }
    }
}

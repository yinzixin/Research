using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.Helpers.Test {
    [TestClass]
    public class PreComputedGridDataSourceTest {
        [TestMethod]
        public void PreSortedDataSourceReturnsRowCountItWasSpecified() {
            // Arrange
            int rows = 20;
            var dataSource = new PreComputedGridDataSource(new WebGrid(GetContext()), values: Enumerable.Range(0, 10).Cast<dynamic>(), totalRows: rows);

            // Act and Assert
            Assert.AreEqual(rows, dataSource.TotalRowCount);
        }

        [TestMethod]
        public void PreSortedDataSourceReturnsAllRows() {
            // Arrange
            var grid = new WebGrid(GetContext());
            var dataSource = new PreComputedGridDataSource(grid: grid, values: Enumerable.Range(0, 10).Cast<dynamic>(), totalRows: 10);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = String.Empty }, 0);

            // Assert
            Assert.AreEqual(rows.Count, 10);
            Assert.AreEqual(rows.First().Value, 0);
            Assert.AreEqual(rows.Last().Value, 9);
        }

        private HttpContextBase GetContext() {
            return new Mock<HttpContextBase>().Object;
        }
    }
}

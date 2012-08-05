using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Web.WebPages;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.Helpers.Test {
    [TestClass]
    public class WebGridTest {

        [TestMethod]
        public void AjaxCheckedOnlyOnce() {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid")
                .Bind(new[] { new { First = "First", Second = "Second" } });
            string html = grid.Table().ToString();
            Assert.IsTrue(html.Contains("typeof(jQuery)"));
            html = grid.Table().ToString();
            Assert.IsFalse(html.Contains("typeof(jQuery)"));
            html = grid.Pager().ToString();
            Assert.IsFalse(html.Contains("typeof(jQuery)"));
        }

        [TestMethod]
        public void AjaxCallbackIgnoredIfAjaxUpdateContainerIdIsNotSet() {
            var grid = new WebGrid(GetContext(), ajaxUpdateCallback: "myCallback")
                       .Bind(new[] { new { First = "First", Second = "Second" } });
            string html = grid.Table().ToString();
            Assert.IsFalse(html.Contains("typeof(jQuery)"));
            Assert.IsFalse(html.Contains("myCallback"));
        }

        [TestMethod]
        public void ColumnNameDefaultsExcludesIndexedProperties() {
            var grid = new WebGrid(GetContext()).Bind(new[] { "First", "Second" });
            Assert.AreEqual(1, grid.ColumnNames.Count());
            Assert.IsTrue(grid.ColumnNames.Contains("Length"));
        }

        [TestMethod]
        public void ColumnNameDefaultsForDynamics() {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(new { First = "First", Second = "Second" }));
            Assert.AreEqual(2, grid.ColumnNames.Count());
            Assert.IsTrue(grid.ColumnNames.Contains("First"));
            Assert.IsTrue(grid.ColumnNames.Contains("Second"));
        }

        [TestMethod]
        public void ColumnNameDefaultsForNonDynamics() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { First = "First", Second = "Second" } });
            Assert.AreEqual(2, grid.ColumnNames.Count());
            Assert.IsTrue(grid.ColumnNames.Contains("First"));
            Assert.IsTrue(grid.ColumnNames.Contains("Second"));
        }

        [TestMethod]
        public void ColumnNameDefaultsSupportsBindableTypes() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new {
                    DateTime = DateTime.MinValue,
                    DateTimeOffset = DateTimeOffset.MinValue,
                    Decimal = Decimal.MinValue,
                    Guid = Guid.Empty,
                    Int32 = 1,
                    NullableInt32 = (int?)1,
                    Object = new object(),
                    Projection = new { Foo = "Bar" },
                    TimeSpan = TimeSpan.MinValue
                }
            });
            Assert.AreEqual(7, grid.ColumnNames.Count());
            Assert.IsTrue(grid.ColumnNames.Contains("DateTime"));
            Assert.IsTrue(grid.ColumnNames.Contains("DateTimeOffset"));
            Assert.IsTrue(grid.ColumnNames.Contains("Decimal"));
            Assert.IsTrue(grid.ColumnNames.Contains("Guid"));
            Assert.IsTrue(grid.ColumnNames.Contains("Int32"));
            Assert.IsTrue(grid.ColumnNames.Contains("NullableInt32"));
            Assert.IsTrue(grid.ColumnNames.Contains("TimeSpan"));
            Assert.IsFalse(grid.ColumnNames.Contains("Object"));
            Assert.IsFalse(grid.ColumnNames.Contains("Projection"));
        }

        [TestMethod]
        public void ColumnsIsNoOp() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { First = "First", Second = "Second" }
            });
            var columns = new[] {
                grid.Column("First"), grid.Column("Second")
            };
            Assert.AreEqual(columns, grid.Columns(columns));
        }

        [TestMethod]
        public void ColumnThrowsIfColumnNameIsEmptyAndNoFormat() {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Column(columnName: String.Empty, format: null);
            }, "columnName", "The column name cannot be null or an empty string unless a custom format is specified.");
        }

        [TestMethod]
        public void ColumnThrowsIfColumnNameIsNullAndNoFormat() {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Column(columnName: null, format: null);
            }, "columnName", "The column name cannot be null or an empty string unless a custom format is specified.");
        }

        [TestMethod]
        public void BindThrowsIfSourceIsNull() {
            ExceptionAssert.ThrowsArgNull(() => {
                new WebGrid(GetContext()).Bind(null);
            }, "source");
        }

        [TestMethod]
        public void ConstructorThrowsIfRowsPerPageIsLessThanOne() {
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                new WebGrid(GetContext(), rowsPerPage: 0);
            }, "rowsPerPage", 1, null, true);
        }

        [TestMethod]
        public void GetHtmlDefaults() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            var html = grid.GetHtml();
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html);
        }

        [TestMethod]
        public void WebGridProducesValidHtmlWhenSummaryIsSpecified() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                           .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            var caption = "WebGrid With Caption";
            var html = grid.GetHtml(caption: caption);
            AssertEqualsIgnoreLineBreaks(
                "<table>" +
                "<caption>" + caption + "</caption>" +
                "<thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html);
        }

        [TestMethod]
        public void WebGridEncodesCaptionText() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            var caption = "WebGrid <> With Caption";
            var html = grid.GetHtml(caption: caption);
            AssertEqualsIgnoreLineBreaks(
                "<table>" +
                "<caption>WebGrid &lt;&gt; With Caption</caption>" +
                "<thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html);
        }

        [TestMethod]
        public void GetHtmlWhenPageCountIsOne() {
            var grid = new WebGrid(GetContext())
                        .Bind(new[] {
                            new { P1 = 1, P2 = '2', P3 = "3" }
                        });
            var html = grid.GetHtml();
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html);
        }

        [TestMethod]
        public void GetHtmlWhenPagingAndSortingAreDisabled() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false, canSort: false)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            var html = grid.GetHtml();
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\">P1</th>" +
                "<th scope=\"col\">P2</th>" +
                "<th scope=\"col\">P3</th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>1</td><td>2</td><td>3</td></tr>" +
                "<tr><td>4</td><td>5</td><td>6</td></tr>" +
                "</tbody>" +
                "</table>", html);
        }

        [TestMethod]
        public void PageIndexCanBeReset() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(1, grid.PageIndex);
            grid.PageIndex = 0;
            Assert.AreEqual(0, grid.PageIndex);
            // verify that selection link has updated page
            Assert.AreEqual("?page=1&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
        }

        [TestMethod]
        public void PageIndexCanBeResetToSameValue() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                        .Bind(new[] {
                            new { P1 = 1, P2 = '2', P3 = "3" },
                            new { P1 = 4, P2 = '5', P3 = "6" }
                        });
            grid.PageIndex = 0;
            Assert.AreEqual(0, grid.PageIndex);
        }

        [TestMethod]
        public void PageIndexDefaultsToZero() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(0, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(1, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void SetPageIndexThrowsExceptionWhenValueIsNegative() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            ExceptionAssert.ThrowsArgOutOfRange(() => { grid.PageIndex = -1; }, "value", 0, grid.PageCount - 1, false);
        }

        [TestMethod]
        public void SetPageIndexThrowsExceptionWhenValueIsEqualToPageCount() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            ExceptionAssert.ThrowsArgOutOfRange(() => { grid.PageIndex = grid.PageCount; }, "value", 0, grid.PageCount - 1, false);
        }

        [TestMethod]
        public void SetPageIndexThrowsExceptionWhenValueIsGreaterToPageCount() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            ExceptionAssert.ThrowsArgOutOfRange(() => { grid.PageIndex = grid.PageCount + 1; }, "value", 0, grid.PageCount - 1, false);
        }

        [TestMethod]
        public void SetPageIndexThrowsExceptionWhenPagingIsDisabled() {
            var grid = new WebGrid(GetContext(), canPage: false)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            ExceptionAssert.Throws<NotSupportedException>(() => { grid.PageIndex = 1; }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [TestMethod]
        public void PageIndexResetsToLastPageWhenQueryStringValueGreaterThanPageCount() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(1, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(4, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void PageIndexResetWhenQueryStringValueIsInvalid() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "NotAnInt";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(0, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(1, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void PageIndexResetWhenQueryStringValueLessThanOne() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                        .Bind(new[] {
                            new { P1 = 1, P2 = '2', P3 = "3" },
                            new { P1 = 4, P2 = '5', P3 = "6" }
                        });
            Assert.AreEqual(0, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(1, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void PageIndexUsesCustomQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_pg"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, fieldNamePrefix: "g_", pageFieldName: "pg")
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(1, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(4, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void PageIndexUsesQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(1, grid.PageIndex);
            Assert.AreEqual(1, grid.Rows.Count);
            Assert.AreEqual(4, grid.Rows.First()["P1"]);
        }

        [TestMethod]
        public void GetPageCountWhenPagingIsTurnedOn() {
            var grid = new WebGrid(GetContext(), canPage: true, rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(2, grid.PageCount);
        }

        [TestMethod]
        public void GetPageIndexWhenPagingIsTurnedOn() {
            var grid = new WebGrid(GetContext(), canPage: true, rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" },
                                new { P1 = 4, P2 = '5', P3 = "6" },
                            });
            grid.PageIndex = 1;
            Assert.AreEqual(1, grid.PageIndex);
            Assert.AreEqual(3, grid.PageCount);
            grid.PageIndex = 2;
            Assert.AreEqual(2, grid.PageIndex);
        }

        [TestMethod]
        public void GetPageCountWhenPagingIsTurnedOff() {
            var grid = new WebGrid(GetContext(), canPage: false, rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            Assert.AreEqual(1, grid.PageCount);
        }

        [TestMethod]
        public void GetPageIndexWhenPagingIsTurnedOff() {
            var grid = new WebGrid(GetContext(), canPage: false, rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" },
                                new { P1 = 4, P2 = '5', P3 = "6" },
                            });
            Assert.AreEqual(0, grid.PageIndex);
            Assert.AreEqual(1, grid.PageCount);
        }

        [TestMethod]
        public void PageUrlResetsSelection() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            string url = grid.GetPageUrl(1);
            Assert.AreEqual("?page=2&sort=P1&sortdir=DESC", url);
        }

        [TestMethod]
        public void PageUrlResetsSelectionForAjax() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, ajaxUpdateContainerId: "grid")
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            string html = grid.GetContainerUpdateScript(grid.GetPageUrl(1)).ToString();
            Assert.IsTrue(html.StartsWith("$(&#39;#grid&#39;).load(&#39;?page=2&amp;sort=P1&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;);", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PageUrlResetsSelectionForAjaxWithCallback() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback")
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            string html = grid.GetContainerUpdateScript(grid.GetPageUrl(1)).ToString();
            Assert.IsTrue(html.StartsWith("$(&#39;#grid&#39;).load(&#39;?page=2&amp;sort=P1&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;, myCallback);", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PageUrlThrowsIfIndexGreaterThanOrEqualToPageCount() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                grid.GetPageUrl(2);
            }, "pageIndex", 0, 1, true);
        }

        [TestMethod]
        public void PageUrlThrowsIfIndexLessThanZero() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                grid.GetPageUrl(-1);
            }, "pageIndex", 0, 1, true);
        }

        [TestMethod]
        public void PageUrlThrowsIfPagingIsDisabled() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false).Bind(new[] { new { }, new { } });
            ExceptionAssert.Throws<NotSupportedException>(() => {
                grid.GetPageUrl(2);
            }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [TestMethod]
        public void PagerRenderingDefaults() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { }, new { }, new { } });
            var html = grid.Pager();
            Assert.AreEqual(
                "1 " +
                "<a href=\"?page=2\">2</a> " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=2\">&gt;</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnFirstShowingAll() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { }, new { }, new { } });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 5);
            Assert.AreEqual(
                "1 " +
                "<a href=\"?page=2\">2</a> " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=2\">&gt;</a> " +
                "<a href=\"?page=4\">&gt;&gt;</a>",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnNextToLastShowingAll() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] { 
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 4);
            Assert.AreEqual(
                "<a href=\"?page=1\">&lt;&lt;</a> " +
                "<a href=\"?page=2\">&lt;</a> " +
                "<a href=\"?page=1\">1</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=4\">&gt;</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnMiddleShowingAll() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 3);
            Assert.AreEqual(
                "<a href=\"?page=1\">&lt;&lt;</a> " +
                "<a href=\"?page=2\">&lt;</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=4\">&gt;</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnSecondHidingFirstLast() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric, numericLinksCount: 2);
            Assert.AreEqual(
                "<a href=\"?page=1\">&lt;</a> " +
                "2 " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=3\">&gt;</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnLastHidingFirstLast() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "4";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric, numericLinksCount: 1);
            Assert.AreEqual(
                "<a href=\"?page=3\">&lt;</a> " +
                "4 ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnMiddleHidingNextPrevious() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.FirstLast | WebGridPagerModes.Numeric, numericLinksCount: 0);
            Assert.AreEqual(
                "<a href=\"?page=1\">&lt;&lt;</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingOnMiddleWithLinksCountGreaterThanPageCount() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.Numeric, numericLinksCount: 6);
            Assert.AreEqual(
                "<a href=\"?page=1\">1</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> ",
                html.ToString());
        }

        [TestMethod]
        public void PagerRenderingHidingAll() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[] {
                new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.Numeric, numericLinksCount: 0);
            Assert.AreEqual("", html.ToString());
        }

        [TestMethod]
        public void PagerRenderingTextOverrides() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[] {
                new {}, new {}, new {}, new {}, new {}
            });
            var html = grid.Pager(WebGridPagerModes.FirstLast | WebGridPagerModes.NextPrevious,
            firstText: "first", previousText: "previous", nextText: "next", lastText: "last");
            Assert.AreEqual(
                "<a href=\"?page=1\">first</a> " +
                "<a href=\"?page=2\">previous</a> " +
                "<a href=\"?page=4\">next</a> " +
                "<a href=\"?page=5\">last</a>",
                html.ToString());
        }

        [TestMethod]
        public void PagerThrowsIfTextSetAndModeNotEnabled() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Pager(firstText: "first");

            }, "firstText", "To use this argument, pager mode \"FirstLast\" must be enabled.");
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Pager(mode: WebGridPagerModes.Numeric, previousText: "previous");
            }, "previousText", "To use this argument, pager mode \"NextPrevious\" must be enabled.");
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Pager(mode: WebGridPagerModes.Numeric, nextText: "next");
            }, "nextText", "To use this argument, pager mode \"NextPrevious\" must be enabled.");
            ExceptionAssert.ThrowsArgumentException(() => {
                grid.Pager(lastText: "last");
            }, "lastText", "To use this argument, pager mode \"FirstLast\" must be enabled.");
        }

        [TestMethod]
        public void PagerThrowsIfNumericLinkCountIsLessThanZero() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                grid.Pager(numericLinksCount: -1);
            }, "numericLinksCount", 0, null, true);
        }

        [TestMethod]
        public void PagerThrowsIfPagingIsDisabled() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false).Bind(new[] { new { }, new { } });
            ExceptionAssert.Throws<NotSupportedException>(() => {
                grid.Pager();
            }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [TestMethod]
        public void PagerWithAjax() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, ajaxUpdateContainerId: "grid")
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            string html = grid.Pager().ToString();
            Assert.IsTrue(html.Contains("typeof(jQuery)"));
            Assert.IsTrue(html.Contains("<a href=\"#\" onclick="));
        }

        [TestMethod]
        public void PagerWithAjaxAndCallback() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback")
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            string html = grid.Pager().ToString();
            Assert.IsTrue(html.Contains("typeof(jQuery)"));
            Assert.IsTrue(html.Contains("<a href=\"#\" onclick="));
            Assert.IsTrue(html.Contains("myCallback"));
        }

        [TestMethod]
        public void PropertySettersDoNotThrowBeforePagingAndSorting() {
            // test with selection because SelectedIndex getter used to do range checking that caused paging and sorting
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "1";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2).Bind(new[] { 
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });

            // invoke other WebGrid properties to ensure they don't cause sorting and paging
            foreach (var prop in typeof(WebGrid).GetProperties()) {
                // exceptions: these do cause sorting and paging
                if (prop.Name.Equals("Rows") || prop.Name.Equals("SelectedRow") || prop.Name.Equals("ElementType")) {
                    continue;
                }
                prop.GetValue(grid, null);
            }

            grid.PageIndex = 1;
            grid.SelectedIndex = 0;
            grid.SortColumn = "P1";
            grid.SortDirection = SortDirection.Descending;
        }

        [TestMethod]
        public void PropertySettersDoNotThrowAfterPagingAndSortingIfValuesHaveNotChanged() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[] {
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });
            // calling Rows will sort and page the data
            Assert.AreEqual(2, grid.Rows.Count());

            grid.PageIndex = 0;
            grid.SelectedIndex = -1;
            grid.SortColumn = String.Empty;
            grid.SortDirection = SortDirection.Ascending;
        }

        [TestMethod]
        public void PropertySettersThrowAfterPagingAndSorting() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[] {
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });
            // calling Rows will sort and page the data
            Assert.AreEqual(2, grid.Rows.Count());

            var message = "This property cannot be set after the \"WebGrid\" object has been sorted or paged. Make sure that this property is set prior to invoking the \"Rows\" property directly or indirectly through other methods such as \"GetHtml\", \"Pager\", \"Table\", etc.";
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                grid.PageIndex = 1;
            }, message);
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                grid.SelectedIndex = 0;
            }, message);
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                grid.SortColumn = "P1";
            }, message);
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                grid.SortDirection = SortDirection.Descending;
            }, message);
        }

        [TestMethod]
        public void RowColumnsAreDynamicMembersForDynamics() {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
            ));
            dynamic row = grid.Rows.First();
            Assert.AreEqual(1, row.P1);
            Assert.AreEqual('2', row.P2);
            Assert.AreEqual("3", row.P3);
        }

        [TestMethod]
        public void RowColumnsAreDynamicMembersForNonDynamics() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            dynamic row = grid.Rows.First();
            Assert.AreEqual(1, row.P1);
            Assert.AreEqual('2', row.P2);
            Assert.AreEqual("3", row.P3);
        }

        [TestMethod]
        public void RowExposesRowIndex() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { }, new { }, new { }
            });
            dynamic row = grid.Rows.First();
            Assert.AreEqual(0, row["ROW"]);
            row = grid.Rows.Skip(1).First();
            Assert.AreEqual(1, row.ROW);
            row = grid.Rows.Skip(2).First();
            Assert.AreEqual(2, row.ROW);
        }

        [TestMethod]
        public void RowExposesUnderlyingValue() {
            var sb = new System.Text.StringBuilder("Foo");
            sb.Append("Bar");
            var grid = new WebGrid(GetContext()).Bind(new[] { sb });
            var row = grid.Rows.First();
            Assert.AreEqual(sb, row.Value);
            Assert.AreEqual("FooBar", row.ToString());
            Assert.AreEqual(grid, row.WebGrid);
        }

        [TestMethod]
        public void RowIndexerThrowsWhenColumnNameIsEmpty() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                var value = row[String.Empty];
            }, "name");
        }

        [TestMethod]
        public void RowIndexerThrowsWhenColumnNameIsNull() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                var value = row[null];
            }, "name");
        }

        [TestMethod] // todo - should throw ArgumentException?
        public void RowIndexerThrowsWhenColumnNotFound() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                var value = row["NotAColumn"];
            });
        }

        [TestMethod]
        public void RowIndexerThrowsWhenGreaterThanColumnCount() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => {
                var value = row[4];
            });
        }

        [TestMethod]
        public void RowIndexerThrowsWhenLessThanZero() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => {
                var value = row[-1];
            });
        }

        [TestMethod]
        public void RowIsEnumerableForDynamics() {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
            ));
            int i = 0;
            foreach (var col in (IEnumerable)grid.Rows.First()) i++;
            Assert.AreEqual(3, i);
        }

        [TestMethod]
        public void RowIsEnumerableForNonDynamics() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            int i = 0;
            foreach (var col in grid.Rows.First()) i++;
            Assert.AreEqual(3, i);
        }

        [TestMethod]
        public void RowIsIndexableByColumnForDynamics() {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
            ));
            var row = grid.Rows.First();
            Assert.AreEqual(1, row["P1"]);
            Assert.AreEqual('2', row["P2"]);
            Assert.AreEqual("3", row["P3"]);
        }

        [TestMethod]
        public void RowIsIndexableByColumnForNonDynamics() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            Assert.AreEqual(1, row["P1"]);
            Assert.AreEqual('2', row["P2"]);
            Assert.AreEqual("3", row["P3"]);
        }

        [TestMethod]
        public void RowIsIndexableByIndexForDynamics() {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
            ));
            var row = grid.Rows.First();
            Assert.AreEqual(1, row[0]);
            Assert.AreEqual('2', row[1]);
            Assert.AreEqual("3", row[2]);
        }

        [TestMethod]
        public void RowIsIndexableByIndexForNonDynamics() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            Assert.AreEqual(1, row[0]);
            Assert.AreEqual('2', row[1]);
            Assert.AreEqual("3", row[2]);
        }

        [TestMethod]
        public void RowsNotPagedWhenPagingIsDisabled() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, canPage: false)
                            .Bind(new[] {
                                new { P1 = 1, P2 = '2', P3 = "3" },
                                new { P1 = 4, P2 = '5', P3 = "6" }
                            });
            // review: should we reset PageIndex or Sort when operation disabled?
            Assert.AreEqual(0, grid.PageIndex);
            Assert.AreEqual(2, grid.Rows.Count);
            Assert.AreEqual(1, grid.Rows.First()["P1"]);
            Assert.AreEqual(4, grid.Rows.Skip(1).First()["P1"]);
        }

        [TestMethod] // todo - should throw ArgumentException?
        public void RowTryGetMemberReturnsFalseWhenColumnNotFound() {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            object value = null;
            Assert.IsFalse(row.TryGetMember("NotAColumn", out value));
        }

        [TestMethod]
        public void SelectedIndexCanBeReset() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.AreEqual(1, grid.SelectedIndex);
            grid.SelectedIndex = 0;
            Assert.AreEqual(0, grid.SelectedIndex);
        }

        [TestMethod]
        public void SelectedIndexCanBeResetToSameValue() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            grid.SelectedIndex = -1;
            Assert.AreEqual(-1, grid.SelectedIndex);
        }

        [TestMethod]
        public void SelectedIndexDefaultsToNegative() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsFalse(grid.HasSelection);
            Assert.AreEqual(-1, grid.SelectedIndex);
            Assert.AreEqual(null, grid.SelectedRow);
        }

        [TestMethod]
        public void SelectedIndexResetWhenQueryStringValueGreaterThanRowsPerPage() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsFalse(grid.HasSelection);
            Assert.AreEqual(-1, grid.SelectedIndex);
            Assert.AreEqual(null, grid.SelectedRow);
        }

        [TestMethod]
        public void SelectedIndexPersistsWhenPagingTurnedOff() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2, canPage: false).Bind(new[] {
                new {} , new {} , new {}, new {}
            });
            grid.SelectedIndex = 3;
            Assert.AreEqual(3, grid.SelectedIndex);
        }

        [TestMethod]
        public void SelectedIndexResetWhenQueryStringValueIsInvalid() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "NotAnInt";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsFalse(grid.HasSelection);
            Assert.AreEqual(-1, grid.SelectedIndex);
            Assert.AreEqual(null, grid.SelectedRow);
        }

        [TestMethod]
        public void SelectedIndexResetWhenQueryStringValueLessThanOne() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "0";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsFalse(grid.HasSelection);
            Assert.AreEqual(-1, grid.SelectedIndex);
            Assert.AreEqual(null, grid.SelectedRow);
        }

        [TestMethod]
        public void SelectedIndexUsesCustomQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_sel"] = "2";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", selectionFieldName: "sel").Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsTrue(grid.HasSelection);
            Assert.AreEqual(1, grid.SelectedIndex);
            Assert.IsNotNull(grid.SelectedRow);
            Assert.AreEqual(4, grid.SelectedRow["P1"]);
        }

        [TestMethod]
        public void SelectedIndexUsesQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.IsTrue(grid.HasSelection);
            Assert.AreEqual(1, grid.SelectedIndex);
            Assert.IsNotNull(grid.SelectedRow);
            Assert.AreEqual(4, grid.SelectedRow["P1"]);
        }

        [TestMethod]
        public void SelectLink() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "1";
            queryString["row"] = "1";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            string html = grid.Rows[1].GetSelectLink().ToString();
            Assert.AreEqual("<a href=\"?page=1&amp;row=2&amp;sort=P1&amp;sortdir=DESC\">Select</a>", html);
        }

        [TestMethod]
        public void SelectLinkForAjax() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "1";
            queryString["row"] = "1";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), ajaxUpdateContainerId: "grid").Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            string html = grid.Rows[1].GetSelectLink(text: "Select Second").ToString();

            Assert.IsTrue(html.StartsWith("<a href=\"#\" onclick=\"$(&#39;#grid&#39;).load(&#39;?page=1&amp;row=2&amp;sort=P1&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;);\">Select Second</a>", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SelectLinkForAjaxWithCallback() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "1";
            queryString["row"] = "1";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback").Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            string html = grid.Rows[1].GetSelectLink(text: "Select Second").ToString();

            Assert.IsTrue(html.StartsWith("<a href=\"#\" onclick=\"$(&#39;#grid&#39;).load(&#39;?page=1&amp;row=2&amp;sort=P1&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;, myCallback);\">Select Second</a>", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SortCanBeReset() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.AreEqual("P1", grid.SortColumn);
            grid.SortColumn = "P2";
            Assert.AreEqual("P2", grid.SortColumn);
            // verify that selection and page links have updated sort
            Assert.AreEqual("?sort=P2&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.AreEqual("?sort=P2&page=1", grid.GetPageUrl(0));
        }

        [TestMethod]
        public void SortCanBeResetToNull() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.AreEqual("P1", grid.SortColumn);
            grid.SortColumn = null;
            Assert.AreEqual(String.Empty, grid.SortColumn);
            // verify that selection and page links have updated sort
            Assert.AreEqual("?row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.AreEqual("?page=1", grid.GetPageUrl(0));
        }

        [TestMethod]
        public void SortCanBeResetToSameValue() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            grid.SortColumn = String.Empty;
            Assert.AreEqual(String.Empty, grid.SortColumn);
        }

        [TestMethod]
        public void SortColumnDefaultsToEmpty() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.AreEqual(String.Empty, grid.SortColumn);
        }

        [TestMethod]
        public void SortColumnResetWhenQueryStringValueIsInvalid() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P4";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.AreEqual("", grid.SortColumn);
        }

        [TestMethod]
        public void SortColumnUsesCustomQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_st"] = "P2";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", sortFieldName: "st").Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.AreEqual("P2", grid.SortColumn);
        }

        [TestMethod]
        public void SortColumnUsesQueryString() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.AreEqual("P2", grid.SortColumn);
        }

        [TestMethod]
        public void SortDirectionCanBeReset() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.AreEqual(SortDirection.Descending, grid.SortDirection);
            grid.SortDirection = SortDirection.Ascending;
            Assert.AreEqual(SortDirection.Ascending, grid.SortDirection);
            // verify that selection and page links have updated sort
            Assert.AreEqual("?sortdir=ASC&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.AreEqual("?sortdir=ASC&page=1", grid.GetPageUrl(0));
        }

        [TestMethod]
        public void SortDirectionDefaultsToAscending() {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            Assert.AreEqual(SortDirection.Ascending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDirectionResetWhenQueryStringValueIsInvalid() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "NotASortDir";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.AreEqual(SortDirection.Ascending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDirectionUsesQueryStringOfAsc() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "aSc";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.AreEqual(SortDirection.Ascending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDirectionUsesQueryStringOfAscending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "AScendING";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.AreEqual(SortDirection.Ascending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDirectionUsesQueryStringOfDesc() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "DeSc";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.AreEqual(SortDirection.Descending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDirectionUsesQueryStringOfDescending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_sd"] = "DeScendING";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", sortDirectionFieldName: "sd").Bind(new object[0]);
            Assert.AreEqual(SortDirection.Descending, grid.SortDirection);
        }

        [TestMethod]
        public void SortDisabledIfSortIsEmpty() {
            var grid = new WebGrid(GetContext(), defaultSort: String.Empty).Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            ));
            Assert.AreEqual("Joe", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortDisabledIfSortIsNull() {
            var grid = new WebGrid(GetContext(), defaultSort: null).Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            ));
            Assert.AreEqual("Joe", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortForDynamics() {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            ));
            Assert.AreEqual("Bob", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortForDynamicsDescending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            ));
            Assert.AreEqual("Smith", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Johnson", grid.Rows[2]["LastName"]);
            Assert.AreEqual("Anderson", grid.Rows[3]["LastName"]);
        }

        [TestMethod]
        public void SortForNonDynamicNavigationColumn() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "Not.A.Column";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "Person.FirstName").Bind(new[] {
                new { Person = new { FirstName = "Joe", LastName = "Smith" } },
                new { Person = new { FirstName = "Bob", LastName = "Johnson" } },
                new { Person = new { FirstName = "Sam", LastName = "Jones" } },
                new { Person = new { FirstName = "Tom", LastName = "Anderson" } }
            });
            Assert.AreEqual("Not.A.Column", grid.SortColumn); // navigation columns are validated during sort
            Assert.AreEqual("Bob", grid.Rows[0]["Person.FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[1]["Person.FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["Person.FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["Person.FirstName"]);
        }

        [TestMethod]
        public void SortForNonDynamics() {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.AreEqual("Bob", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortForNonDynamicsDescending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.AreEqual("Smith", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Johnson", grid.Rows[2]["LastName"]);
            Assert.AreEqual("Anderson", grid.Rows[3]["LastName"]);
        }

        [TestMethod]
        public void SortForNonDynamicsEnumerable() {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            }.ToList());
            Assert.AreEqual("Bob", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortForNonDynamicsEnumerableDescending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            }.ToList());
            Assert.AreEqual("Smith", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Johnson", grid.Rows[2]["LastName"]);
            Assert.AreEqual("Anderson", grid.Rows[3]["LastName"]);
        }

        [TestMethod]
        public void SortForNonGenericEnumerable() {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new NonGenericEnumerable(new[] {
                new Person { FirstName = "Joe", LastName = "Smith" },
                new Person { FirstName = "Bob", LastName = "Johnson" },
                new Person { FirstName = "Sam", LastName = "Jones" },
                new Person { FirstName = "Tom", LastName = "Anderson" }
            }));
            Assert.AreEqual("Bob", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortForNonGenericEnumerableDescending() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new NonGenericEnumerable(new[] {
                new Person { FirstName = "Joe", LastName = "Smith" },
                new Person { FirstName = "Bob", LastName = "Johnson" },
                new Person { FirstName = "Sam", LastName = "Jones" },
                new Person { FirstName = "Tom", LastName = "Anderson" }
            }));
            Assert.AreEqual("Smith", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Johnson", grid.Rows[2]["LastName"]);
            Assert.AreEqual("Anderson", grid.Rows[3]["LastName"]);
        }

        [TestMethod]
        public void SortUrlAjax() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "FirstName";
            queryString["page"] = "2";
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2, ajaxUpdateContainerId: "grid").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            string html = grid.GetContainerUpdateScript(grid.GetSortUrl("FirstName")).ToString();
            Assert.IsTrue(html.StartsWith("$(&#39;#grid&#39;).load(&#39;?sort=FirstName&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;);", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SortUrlAjaxWithCallback() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "FirstName";
            queryString["page"] = "2";
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback").Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            string html = grid.GetContainerUpdateScript(grid.GetSortUrl("FirstName")).ToString();
            Assert.IsTrue(html.StartsWith("$(&#39;#grid&#39;).load(&#39;?sort=FirstName&amp;sortdir=DESC", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.EndsWith("#grid&#39;, myCallback);", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SortUrlDefaults() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { FirstName = "Bob" }
            });
            string html = grid.GetSortUrl("FirstName");
            Assert.AreEqual("?sort=FirstName&sortdir=ASC", html);
        }

        [TestMethod]
        public void SortUrlThrowsIfColumnNameIsEmpty() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { }, new { }
            });
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                grid.GetSortUrl(String.Empty);
            }, "column");
        }

        [TestMethod]
        public void SortUrlThrowsIfColumnNameIsNull() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { }, new { }
            });
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                grid.GetSortUrl(null);
            }, "column");
        }

        [TestMethod]
        public void SortUrlThrowsIfSortingIsDisabled() {
            var grid = new WebGrid(GetContext(), canSort: false).Bind(new[] {
                new { P1 = 1 }, new { P1 = 2 }
            });
            ExceptionAssert.Throws<NotSupportedException>(() => {
                grid.GetSortUrl("P1");
            }, "This operation is not supported when sorting is disabled for the \"WebGrid\" object.");
        }

        [TestMethod]
        public void SortWhenSortIsDisabled() {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName", canSort: false).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.AreEqual("Joe", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Sam", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Tom", grid.Rows[3]["FirstName"]);
        }

        [TestMethod]
        public void SortWithNullValues() {
            var data = new[] {
                new { FirstName = (object)"Joe", LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)null, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.AreEqual("Jones", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[2]["FirstName"]);

            grid = new WebGrid(GetContext(), defaultSort: "FirstName desc").Bind(data);

            Assert.AreEqual("Joe", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Jones", grid.Rows[2]["LastName"]);
        }

        [TestMethod]
        public void SortWithMultipleNullValues() {
            var data = new[] {
                new { FirstName = (object)"Joe", LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)null, LastName = "Hughes" },
                new { FirstName = (object)null, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.AreEqual("Hughes", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Bob", grid.Rows[2]["FirstName"]);
            Assert.AreEqual("Joe", grid.Rows[3]["FirstName"]);

            grid = new WebGrid(GetContext(), defaultSort: "FirstName desc").Bind(data);

            Assert.AreEqual("Joe", grid.Rows[0]["FirstName"]);
            Assert.AreEqual("Bob", grid.Rows[1]["FirstName"]);
            Assert.AreEqual("Hughes", grid.Rows[2]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[3]["LastName"]);
        }

        [TestMethod]
        public void SortWithMixedValuesDoesNotThrow() {
            var data = new[] {
                new { FirstName = (object)1, LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)DBNull.Value, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.IsNotNull(grid.Rows, "Rows should be the original unsorted set");

            Assert.AreEqual("Smith", grid.Rows[0]["LastName"]);
            Assert.AreEqual("Johnson", grid.Rows[1]["LastName"]);
            Assert.AreEqual("Jones", grid.Rows[2]["LastName"]);
        }

        [TestMethod]
        public void SortWithUnsortableDoesNotThrow() {
            var object1 = new object();
            var object2 = new object();
            var data = new[] {
                new { Value = object1 },
                new { Value = object2 }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "Value").Bind(data);

            Assert.IsNotNull(grid.Rows, "Rows should be the original unsorted set");

            Assert.AreEqual(object1, grid.Rows[0]["Value"]);
            Assert.AreEqual(object2, grid.Rows[1]["Value"]);
        }

        [TestMethod]
        public void TableRenderingWithColumnTemplates() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var html = grid.Table(displayHeader: false,
                columns: new[] {
                    grid.Column("P1", format: item => { return "<span>P1: " + item.P1 + "</span>"; }),
                    grid.Column("P2", format: item => { return new HtmlString("<span>P2: " + item.P2 + "</span>"); }),
                    grid.Column("P3", format: item => {
                        return new HelperResult(tw => {
                            tw.Write("<span>P3: " + item.P3 + "</span>");
                        });
                    })
                });
            AssertEqualsIgnoreLineBreaks(
                "<table><tbody><tr>" +
                "<td>&lt;span&gt;P1: 1&lt;/span&gt;</td>" +
                "<td><span>P2: 2</span></td>" +
                "<td><span>P3: 3</span></td>" +
                "</tr></tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithDefaultCellValueOfCustom() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[] {
                new { P1 = String.Empty, P2 = (string)null },
            });
            var html = grid.Table(fillEmptyRows: true, emptyRowCellValue: "N/A", displayHeader: false);
            AssertEqualsIgnoreLineBreaks(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td>N/A</td><td>N/A</td></tr>" +
                "<tr><td>N/A</td><td>N/A</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithDefaultCellValueOfEmpty() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[] {
                new { P1 = String.Empty, P2 = (string)null }
            });
            var html = grid.Table(fillEmptyRows: true, emptyRowCellValue: "", displayHeader: false);
            AssertEqualsIgnoreLineBreaks(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td></td><td></td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithDefaultCellValueOfNbsp() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[] {
                new { P1 = String.Empty, P2 = (string)null }
            });
            var html = grid.Table(fillEmptyRows: true, displayHeader: false);
            AssertEqualsIgnoreLineBreaks(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithExclusions() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var html = grid.Table(exclusions: new string[] { "P2" });
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>1</td><td>3</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithNoStylesAndFillEmptyRows() {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(fillEmptyRows: true);
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithSortingDisabled() {
            var grid = new WebGrid(GetContext(), canSort: false).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table();
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\">FirstName</th>" +
                "<th scope=\"col\">LastName</th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingWithAttributes() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { id = "my-table-id", summary = "Table summary" });
            AssertEqualsIgnoreLineBreaks(
                "<table id=\"my-table-id\" summary=\"Table summary\"><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingEncodesAttributes() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { summary = "\"<Table summary" });
            AssertEqualsIgnoreLineBreaks(
                "<table summary=\"&quot;&lt;Table summary\"><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingIsNotAffectedWhenAttributesIsNull() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: null);
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableRenderingIsNotAffectedWhenAttributesIsEmpty() {
            var grid = new WebGrid(GetContext()).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { });
            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html);
        }


        [TestMethod]
        public void TableRenderingWithStyles() {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "1";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 4).Bind(new[] {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" }
            });
            var html = grid.Table(tableStyle: "tbl", headerStyle: "hdr", footerStyle: "ftr",
                rowStyle: "row", alternatingRowStyle: "arow", selectedRowStyle: "sel", fillEmptyRows: true,
                footer: item => "footer text",
                columns: new[] {
                    grid.Column("firstName", style: "c1", canSort: false),
                    grid.Column("lastName", style: "c2", canSort: false)
                });
            AssertEqualsIgnoreLineBreaks(
                "<table class=\"tbl\"><thead><tr class=\"hdr\">" +
                "<th scope=\"col\">firstName</th><th scope=\"col\">lastName</th>" +
                "</tr></thead>" +
                "<tfoot>" +
                "<tr class=\"ftr\"><td colspan=\"2\">footer text</td></tr>" +
                "</tfoot>" +
                "<tbody>" +
                "<tr class=\"row sel\"><td class=\"c1\">Joe</td><td class=\"c2\">Smith</td></tr>" +
                "<tr class=\"arow\"><td class=\"c1\">Bob</td><td class=\"c2\">Johnson</td></tr>" +
                "<tr class=\"row\"><td class=\"c1\">&nbsp;</td><td class=\"c2\">&nbsp;</td></tr>" +
                "<tr class=\"arow\"><td class=\"c1\">&nbsp;</td><td class=\"c2\">&nbsp;</td></tr>" +
                "</tbody></table>", html);
        }

        [TestMethod]
        public void TableWithAjax() {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid").Bind(new[] {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.IsTrue(html.Contains("typeof(jQuery)"));
            Assert.IsTrue(html.Contains("<a href=\"#\" onclick="));
        }

        [TestMethod]
        public void TableWithAjaxAndCallback() {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback").Bind(new[] {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.IsTrue(html.Contains("typeof(jQuery)"));
            Assert.IsTrue(html.Contains("<a href=\"#\" onclick="));
            Assert.IsTrue(html.Contains("myCallback"));
        }

        [TestMethod]
        public void WebGridEncodesJavascriptStrings() {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "'grid'", ajaxUpdateCallback: "'myCallback'").Bind(new[] {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.IsTrue(html.Contains(@"if (typeof(jQuery)=='undefined') alert(""A jQuery script reference is required in order to enable Ajax support in the \""WebGrid\"" helper."");"));
            Assert.IsTrue(html.Contains(@"$(&#39;#\u0027grid\u0027&#39;).load"));
            Assert.IsTrue(html.Contains(@"\u0027myCallback\u0027"));
        }

        [TestMethod]
        public void RandomizedUrlAddsParameterToUrlWhenNoParamsArePresent() {
            // Arrange
            string path1 = "http://www.some-website-for.net/homepage";
            string path2 = "http://www.some-website-for.net/homepage.cshtml";
            var randomValue = 4;

            // Act
            Assert.AreEqual(WebGrid.GetRandomizedUrl(path1, randomValue), path1 + "?__=" + randomValue);
            Assert.AreEqual(WebGrid.GetRandomizedUrl(path2, randomValue), path2 + "?__=" + randomValue);
        }

        [TestMethod]
        public void RandomizedUrlAppendsParameterToUrlWhenNoParamsArePresent() {
            // Arrange
            string url1 = "http://www.some-website.net/homepage";
            string url2 = "http://www.some-website.net/homepage.cshtml";
            string url3 = "http://www.some-website.net/homepage/";
            var randomValue = 4;

            // Act And Assert
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url1, randomValue), url1 + "?__=" + randomValue);
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url2, randomValue), url2 + "?__=" + randomValue);
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url3, randomValue), url3 + "?__=" + randomValue);
        }

        [TestMethod]
        public void RandomizedUrlAppendsParameterToUrlWhenOtherParamsArePresent() {
            // Arrange
            string url1 = "http://www.some-website-for.net/homepage?foo=bar";
            string url2 = "http://www.some-website-for.net/homepage?foo=bar&x=y";
            var randomValue = 4;

            // Act and Assert
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url1, randomValue), url1 + "&__=" + randomValue);
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url2, randomValue), url2 + "&__=" + randomValue);
        }

        [TestMethod]
        public void RandomizedUrlReplacesParameterValueWhenRandomKeyIsPresent() {
            // Arrange
            string url1 = "http://www.some-website-for.net/homepage?__=1234";
            string url2 = "http://www.some-website-for.net/homepage?foo=bar&x=y&__=1234";
            string url3 = "http://www.some-website-for.net/homepage?foo=bar&__=1234&x=y";
            string url4 = "http://www.some-website-for.net/homepage?__=1234&x=y";

            var randomValue = 4;

            // Act and Assert
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url1, randomValue), url1.Replace("1234", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url2, randomValue), url2.Replace("1234", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url3, randomValue), url3.Replace("1234", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url4, randomValue), url4.Replace("1234", randomValue.ToString()));
        }

        [TestMethod]
        public void RandomizedUrlDoesNotAffectUrlPathPortion() {
            // Arrange
            string url1 = "http://www.some-website-for.net/homepage__=1234?__=4567";
            string url2 = "http://www.some-website-for.net/__=1234homepage?__=4567&x=y";
            string url3 = "http://www.some-website-for.net/__=1234homepage?x=y&__=4567&a=b";
            string url4 = "http://www.some-website-for.net/__=1234homepage?x=y&a=b";

            var randomValue = 4;

            // Act and Assert
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url1, randomValue), url1.Replace("4567", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url2, randomValue), url2.Replace("4567", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url3, randomValue), url3.Replace("4567", randomValue.ToString()));
            Assert.AreEqual(WebGrid.GetRandomizedUrl(url4, randomValue), url4 + "&__=" + randomValue.ToString());
        }

        [TestMethod]
        public void WebGridThrowsIfOperationsArePerformedBeforeBinding() {
            // Arrange
            string errorMessage = "A data source must be bound before this operation can be performed.";
            var grid = new WebGrid(GetContext());

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => { var rows = grid.Rows; }, errorMessage);
            ExceptionAssert.Throws<InvalidOperationException>(() => { int count = grid.TotalRowCount; }, errorMessage);
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.GetHtml(), errorMessage);
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.Pager(), errorMessage);
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.Table(), errorMessage);
            ExceptionAssert.Throws<InvalidOperationException>(() => { grid.SelectedIndex = 1; var row = grid.SelectedRow; }, errorMessage);
        }

        [TestMethod]
        public void WebGridThrowsIfBindingIsPerformedWhenAlreadyBound() {
            // Arrange
            var grid = new WebGrid(GetContext());
            var values = Enumerable.Range(0, 10).Cast<dynamic>();

            // Act
            grid.Bind(values);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.Bind(values), "The WebGrid instance is already bound to a data source.");
        }

        [TestMethod]
        public void GetElementTypeReturnsDynamicTypeIfElementIsDynamic() {
            // Arrange
            IEnumerable<dynamic> elements = Dynamics(new[] { new Person { FirstName = "Foo", LastName = "Bar" } });

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.AreEqual(typeof(IDynamicMetaObjectProvider), type);
        }

        [TestMethod]
        public void GetElementTypeReturnsEnumerableTypeIfFirstInstanceIsNotDynamic() {
            // Arrange
            IEnumerable<dynamic> elements = Iterator();

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.AreEqual(typeof(Person), type);
        }

        [TestMethod]
        public void TableThrowsIfQueryStringDerivedSortColumnIsExcluded() {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });
            
            var grid = new WebGrid(context, defaultSort: "Name").Bind(employees);


            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.GetHtml(exclusions: new[] { "Salary" }), "Column \"Salary\" does not exist.");
        }

        [TestMethod]
        public void TableThrowsIfQueryStringDerivedSortColumnDoesNotExistInColumnsArgument() {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });

            var grid = new WebGrid(context, canSort: true, defaultSort: "Name").Bind(employees);

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => grid.Table(columns: new[] { new WebGridColumn { ColumnName = "Name" }, new WebGridColumn { ColumnName = "Manager.Name" } }),
                "Column \"Salary\" does not exist.");
        }

        [TestMethod]
        public void TableDoesNotThrowIfQueryStringDerivedSortColumnIsVisibleButNotSortable() {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            collection["sortDir"] = "Desc";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });

            var grid = new WebGrid(context, canSort: true).Bind(employees);

            // Act
            var html = grid.Table(columns: new[] { new WebGridColumn { ColumnName = "Salary", CanSort = false } });

            // Assert
            Assert.IsNotNull(html);
            Assert.AreEqual(grid.Rows[0]["Salary"], 20);
            Assert.AreEqual(grid.Rows[1]["Salary"], 15);
            Assert.AreEqual(grid.Rows[2]["Salary"], 10);
            Assert.AreEqual(grid.Rows[3]["Salary"], 5);
        }

        [TestMethod]
        public void TableThrowsIfComplexPropertyIsUnsortable() {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Manager.Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });
            var grid = new WebGrid(context).Bind(employees, columnNames: new[] { "Name", "Manager.Name" });

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => grid.GetHtml(),
                "Column \"Manager.Salary\" does not exist.");
        }

        [TestMethod]
        public void TableDoesNotThrowIfUnsortableColumnIsExplicitlySpecifiedByUser() {
            // Arrange
            var context = GetContext();
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });
            
            // Act
            var grid = new WebGrid(context).Bind(employees, columnNames: new[] { "Name", "Manager.Name" });
            grid.SortColumn = "Salary";
            var html = grid.Table();

            // Assert
            Assert.AreEqual(grid.Rows[0]["Salary"], 5);
            Assert.AreEqual(grid.Rows[1]["Salary"], 10);
            Assert.AreEqual(grid.Rows[2]["Salary"], 15);

            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>"
                + "<th scope=\"col\"><a href=\"?sort=Name&amp;sortdir=ASC\">Name</a></th>"
                + "<th scope=\"col\"><a href=\"?sort=Manager.Name&amp;sortdir=ASC\">Manager.Name</a></th>"
                + "</tr></thead><tbody>"
                + "<tr><td>A</td><td>-</td></tr>"
                + "<tr><td>D</td><td>C</td></tr>"
                + "<tr><td>C</td><td>A</td></tr>"
                + "</tbody></table>", html);
        }

        [TestMethod]
        public void TableDoesNotThrowIfUnsortableColumnIsDefaultSortColumn() {
            // Arrange
            var context = GetContext();
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });

            // Act
            var grid = new WebGrid(context, defaultSort: "Salary").Bind(employees, columnNames: new[] { "Name", "Manager.Name" });
            var html = grid.Table();

            // Assert
            Assert.AreEqual(grid.Rows[0]["Salary"], 5);
            Assert.AreEqual(grid.Rows[1]["Salary"], 10);
            Assert.AreEqual(grid.Rows[2]["Salary"], 15);

            AssertEqualsIgnoreLineBreaks(
                "<table><thead><tr>"
                + "<th scope=\"col\"><a href=\"?sort=Name&amp;sortdir=ASC\">Name</a></th>"
                + "<th scope=\"col\"><a href=\"?sort=Manager.Name&amp;sortdir=ASC\">Manager.Name</a></th>"
                + "</tr></thead><tbody>"
                + "<tr><td>A</td><td>-</td></tr>"
                + "<tr><td>D</td><td>C</td></tr>"
                + "<tr><td>C</td><td>A</td></tr>"
                + "</tbody></table>", html);
        }

        private static IEnumerable<Person> Iterator() {
            yield return new Person { FirstName = "Foo", LastName = "Bar" };
        }

        [TestMethod]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionPassedImplementsEnumerable() {
            // Arrange
            IList<Person> listElements = new List<Person> { new Person { FirstName = "Foo", LastName = "Bar" } };
            HashSet<dynamic> setElements = new HashSet<dynamic> { new DynamicWrapper(new Person { FirstName = "Foo", LastName = "Bar" }) };

            // Act
            Type listType = WebGrid.GetElementType(listElements);
            Type setType = WebGrid.GetElementType(setElements);

            // Assert
            Assert.AreEqual(typeof(Person), listType);
            Assert.AreEqual(typeof(IDynamicMetaObjectProvider), setType);
        }

        [TestMethod]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionImplementsEnumerable() {
            // Arrange
            IEnumerable<Person> elements =  new NonGenericEnumerable(new[] { new Person { FirstName = "Foo", LastName = "Bar" } });;

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.AreEqual(typeof(Person), type);
        }

        [TestMethod]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionIsIEnumerable() {
            // Arrange
            IEnumerable<Person> elements = new GenericEnumerable<Person>(new[] { new Person { FirstName = "Foo", LastName = "Bar" } }); ;

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.AreEqual(typeof(Person), type);
        }

        [TestMethod]
        public void GetElementTypeDoesNotThrowIfTypeIsNotGeneric() {
            // Arrange
            IEnumerable<dynamic> elements = new[] { new Person { FirstName = "Foo", LastName = "Bar" } };

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.AreEqual(typeof(Person), type);
        }

        private static void AssertEqualsIgnoreLineBreaks(string expected, object actual) {
            Assert.AreEqual(expected, actual.ToString().Replace("\r\n", ""));
        }

        private static IEnumerable<dynamic> Dynamics(params object[] objects) {
            return (from o in objects select new DynamicWrapper(o)).ToArray();
        }

        private static HttpContextBase GetContext(NameValueCollection queryString = null) {
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.QueryString).Returns(queryString ?? new NameValueCollection());

            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            return contextMock.Object;
        }

        class Person {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class Employee {
            public string Name { get; set; }
            public int Salary { get; set; }
            public Employee Manager { get; set; }
        }

        class NonGenericEnumerable : IEnumerable<Person> {
            private IEnumerable<Person> _source;

            public NonGenericEnumerable(IEnumerable<Person> source) {
                _source = source;
            }

            public IEnumerator<Person> GetEnumerator() {
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        class GenericEnumerable<T> : IEnumerable<T> {
            private IEnumerable<T> _source;

            public GenericEnumerable(IEnumerable<T> source) {
                _source = source;
            }

            public IEnumerator<T> GetEnumerator() {
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

    }
}
using System.Collections;
using System.Drawing;
using System.IO;
using System.Web.Hosting;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DV = System.Web.UI.DataVisualization.Charting;

namespace System.Web.Helpers.Test {
    [TestClass]
    public class ChartTest {

        private byte[] _writeData;

        [TestInitialize]
        public void TestInitialize() {
            _writeData = null;
        }

        [TestMethod]
        public void BuildChartAddsDefaultArea() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.ChartAreas.Count);
                Assert.AreEqual("Default", c.ChartAreas[0].Name);
            });
        }

        [TestMethod]
        public void XAxisOverrides() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .SetXAxis("AxisX", 1, 100);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.ChartAreas.Count);
                Assert.AreEqual("AxisX", c.ChartAreas[0].AxisX.Title);
                Assert.AreEqual(1, c.ChartAreas[0].AxisX.Minimum);
                Assert.AreEqual(100, c.ChartAreas[0].AxisX.Maximum);
            });
        }

        [TestMethod]
        public void YAxisOverrides() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .SetYAxis("AxisY", 1, 100);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.ChartAreas.Count);
                Assert.AreEqual("AxisY", c.ChartAreas[0].AxisY.Title);
                Assert.AreEqual(1, c.ChartAreas[0].AxisY.Minimum);
                Assert.AreEqual(100, c.ChartAreas[0].AxisY.Maximum);
            });
        }

        [TestMethod]
        public void ConstructorLoadsTemplate() {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(2, c.BorderWidth);
            });
        }

        [TestMethod]
        public void ConstructorLoadsTheme() {
            //Vanilla theme
            /* 
             * <Chart Palette="SemiTransparent" BorderColor="#000" BorderWidth="2" BorderlineDashStyle="Solid">
                <ChartAreas>
                    <ChartArea _Template_="All" Name="Default">
                            <AxisX>
                                <MinorGrid Enabled="False" />
                                <MajorGrid Enabled="False" />
                            </AxisX>
                            <AxisY>
                                <MajorGrid Enabled="False" />
                                <MinorGrid Enabled="False" />
                            </AxisY>
                    </ChartArea>
                </ChartAreas>
                </Chart>
             */
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, theme: ChartTheme.Vanilla);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(c.Palette, DV.ChartColorPalette.SemiTransparent);
                Assert.AreEqual(c.BorderColor, Color.FromArgb(0, Color.Black));
                Assert.AreEqual(1, c.ChartAreas.Count);
                Assert.IsFalse(c.ChartAreas[0].AxisX.MajorGrid.Enabled);
                Assert.IsFalse(c.ChartAreas[0].AxisY.MinorGrid.Enabled);
            });
        }

        [TestMethod]
        public void ConstructorLoadsThemeAndTemplate() {
            //Vanilla theme
            /* 
             * <Chart Palette="SemiTransparent" BorderColor="#000" BorderWidth="2" BorderlineDashStyle="Solid">
                <ChartAreas>
                    <ChartArea _Template_="All" Name="Default">
                            <AxisX>
                                <MinorGrid Enabled="False" />
                                <MajorGrid Enabled="False" />
                            </AxisX>
                            <AxisY>
                                <MajorGrid Enabled="False" />
                                <MinorGrid Enabled="False" />
                            </AxisY>
                    </ChartArea>
                </ChartAreas>
                </Chart>
             */
            var template = WriteTemplate(@"<Chart BorderlineDashStyle=""DashDot""><Legends><Legend BackColor=""Red"" /></Legends></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, theme: ChartTheme.Vanilla, themePath: template);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(c.Palette, DV.ChartColorPalette.SemiTransparent);
                Assert.AreEqual(c.BorderColor, Color.FromArgb(0, Color.Black));
                Assert.AreEqual(c.BorderlineDashStyle, DV.ChartDashStyle.DashDot);
                Assert.AreEqual(1, c.ChartAreas.Count);
                Assert.AreEqual(c.Legends.Count, 1);
                Assert.AreEqual(c.Legends[0].BackColor, Color.Red);
                Assert.IsFalse(c.ChartAreas[0].AxisX.MajorGrid.Enabled);
                Assert.IsFalse(c.ChartAreas[0].AxisY.MinorGrid.Enabled);
            });
        }

        [TestMethod]
        public void ConstructorSetsWidthAndHeight() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 101, 102);
            Assert.AreEqual(101, chart.Width);
            Assert.AreEqual(102, chart.Height);
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(101, c.Width);
                Assert.AreEqual(102, c.Height);
            });
        }

        [TestMethod]
        public void ConstructorThrowsWhenHeightIsLessThanZero() {
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                new Chart(GetContext(), GetVirtualPathProvider(), 100, -1);
            }, "height", 0, null, true);
        }

        [TestMethod]
        public void ConstructorThrowsWhenTemplateNotFound() {
            var templateFile = @"FileNotFound.xml";
            ExceptionAssert.ThrowsArgumentException(() => {
                new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: templateFile);
            },
            "themePath",
            String.Format("The theme file \"{0}\" could not be found.", VirtualPathUtility.Combine(GetContext().Request.AppRelativeCurrentExecutionFilePath, templateFile)));
        }

        [TestMethod]
        public void ConstructorThrowsWhenWidthIsLessThanZero() {
            ExceptionAssert.ThrowsArgOutOfRange(() => {
                new Chart(GetContext(), GetVirtualPathProvider(), -1, 100);
            }, "width", 0, null, true);
        }

        [TestMethod]
        public void DataBindCrossTable() {
            var data = new[] {
                new { GroupBy = "1", YValue = 1 },
                new { GroupBy = "1", YValue = 2 },
                new { GroupBy = "2", YValue = 1 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindCrossTable(data, "GroupBy", xField: null, yFields: "YValue");
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(2, c.Series.Count);
                Assert.AreEqual(2, c.Series[0].Points.Count);
                Assert.AreEqual(1, c.Series[1].Points.Count);
            });
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenDataSourceIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNull(() => {
                chart.DataBindCrossTable(null, "GroupBy", xField: null, yFields: "yFields");
            }, "dataSource");
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenDataSourceIsString() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgumentException(() => {
                chart.DataBindCrossTable("DataSource", "GroupBy", xField: null, yFields: "yFields");
            }, "dataSource", "A series cannot be data-bound to a string object.");
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenGroupByIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.DataBindCrossTable(new object[0], null, xField: null, yFields: "yFields");
            }, "groupByField");
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenGroupByIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.DataBindCrossTable(new object[0], "", xField: null, yFields: "yFields");
            }, "groupByField");
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenYFieldsIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.DataBindCrossTable(new object[0], "GroupBy", xField: null, yFields: null);
            }, "yFields");
        }

        [TestMethod]
        public void DataBindCrossTableThrowsWhenYFieldsIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.DataBindCrossTable(new object[0], "GroupBy", xField: null, yFields: "");
            }, "yFields");
        }

        [TestMethod]
        public void DataBindTable() {
            var data = new[] {
                new { XValue = "1", YValue = 1 },
                new { XValue = "2", YValue = 2 },
                new { XValue = "3", YValue = 3 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindTable(data, xField: "XValue");
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.Series.Count);
                Assert.AreEqual(3, c.Series[0].Points.Count);
            });
        }

        [TestMethod]
        public void DataBindTableWhenXFieldIsNull() {
            var data = new[] {
                new { YValue = 1 },
                new { YValue = 2 },
                new { YValue = 3 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindTable(data, xField: null);
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.Series.Count);
                Assert.AreEqual(3, c.Series[0].Points.Count);
            });
        }

        [TestMethod]
        public void DataBindTableThrowsWhenDataSourceIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNull(() => {
                chart.DataBindTable(null);
            }, "dataSource");
        }

        [TestMethod]
        public void DataBindTableThrowsWhenDataSourceIsString() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgumentException(() => {
                chart.DataBindTable("");
            }, "dataSource", "A series cannot be data-bound to a string object.");
        }

        [TestMethod]
        public void GetBytesReturnsNonEmptyArray() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.IsTrue(chart.GetBytes().Length > 0);
        }

        [TestMethod]
        public void GetBytesThrowsWhenFormatIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.GetBytes(format: String.Empty);
            }, "format");
        }

        [TestMethod]
        public void GetBytesThrowsWhenFormatIsInvalid() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgumentException(() => {
                chart.GetBytes(format: "foo");
            }, "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [TestMethod]
        public void GetBytesThrowsWhenFormatIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.GetBytes(format: null);
            }, "format");
        }

        [TestMethod]
        public void LegendDefaults() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddLegend();
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.Legends.Count);
                // NOTE: Chart.Legends.Add will create default name
                Assert.AreEqual("Legend1", c.Legends[0].Name);
                Assert.AreEqual(1, c.Legends[0].BorderWidth);
            });
        }

        [TestMethod]
        public void LegendOverrides() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddLegend("Legend1")
                .AddLegend("Legend2", "Legend2Name");
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(2, c.Legends.Count);
                Assert.AreEqual("Legend1", c.Legends[0].Name);
                Assert.AreEqual("Legend2", c.Legends[1].Title);
                Assert.AreEqual("Legend2Name", c.Legends[1].Name);
            });
        }

        [TestMethod]
        public void SaveAndWriteFromCache() {
            var context1 = GetContext();
            var chart = new Chart(context1, GetVirtualPathProvider(), 100, 100);

            string key = chart.SaveToCache();
            Assert.AreEqual(chart, WebCache.Get(key));

            var context2 = GetContext();
            Assert.AreEqual(chart, Chart.GetFromCache(context2, key));

            Chart.WriteFromCache(context2, key);

            Assert.IsNull(context1.Response.ContentType);
            Assert.AreEqual("image/jpeg", context2.Response.ContentType);
        }

        [TestMethod]
        public void SaveThrowsWhenFormatIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.Save(GetContext(), "chartPath", format: String.Empty);
            }, "format");
        }

        [TestMethod]
        public void SaveWorksWhenFormatIsJPG() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);

            string fileName = "chartPath";

            chart.Save(GetContext(), "chartPath", format: "jpg");
            byte[] a = File.ReadAllBytes(fileName);

            chart.Save(GetContext(), "chartPath", format: "jpeg");
            byte[] b = File.ReadAllBytes(fileName);

            CollectionAssert.AreEqual(a, b);
        }

        [TestMethod]
        public void SaveThrowsWhenFormatIsInvalid() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgumentException(() => {
                chart.Save(GetContext(), "chartPath", format: "foo");
            }, "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [TestMethod]
        public void SaveThrowsWhenFormatIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.Save(GetContext(), "chartPath", format: null);
            }, "format");
        }

        [TestMethod]
        public void SaveThrowsWhenPathIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.Save(GetContext(), path: String.Empty, format: "jpeg");
            }, "path");
        }

        [TestMethod]
        public void SaveThrowsWhenPathIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.Save(GetContext(), path: null, format: "jpeg");
            }, "path");
        }

        [TestMethod]
        public void SaveWritesToFile() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            chart.Save(GetContext(), "SaveWritesToFile.jpg", format: "image/jpeg");
            Assert.AreEqual("SaveWritesToFile.jpg", Path.GetFileName(chart.FileName));
            Assert.IsTrue(File.Exists(chart.FileName));
        }

        [TestMethod]
        public void SaveXmlThrowsWhenPathIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.SaveXml(GetContext(), String.Empty);
            }, "path");
        }

        [TestMethod]
        public void SaveXmlThrowsWhenPathIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.SaveXml(GetContext(), null);
            }, "path");
        }

        [TestMethod]
        public void SaveXmlWritesToFile() {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            chart.SaveXml(GetContext(), "SaveXmlWritesToFile.xml");
            Assert.IsTrue(File.Exists("SaveXmlWritesToFile.xml"));
            string result = File.ReadAllText("SaveXmlWritesToFile.xml");
            Assert.IsTrue(result.Contains("BorderWidth=\"2\""));
        }

        [TestMethod]
        public void TemplateWithCommentsDoesNotThrow() {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""><!-- This is a XML comment.  --> </Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            Assert.IsNotNull(chart.ToWebImage());
        }

        [TestMethod]
        public void TemplateWithIncorrectPropertiesThrows() {
            var template = WriteTemplate(@"<Chart borderWidth=""2""><fjkjkgjklfg /></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            ExceptionAssert.Throws<InvalidOperationException>(() => chart.ToWebImage(),
                "Cannot deserialize property. Unknown property name 'borderWidth' in object \" System.Web.UI.DataVisualization.Charting.Chart");
        }

        [TestMethod]
        public void WriteWorksWithJPGFormat() {
            var response = new Mock<HttpResponseBase>();
            var stream = new MemoryStream();
            response.Setup(c => c.Output).Returns(new StreamWriter(stream));

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Response).Returns(response.Object);

            var chart = new Chart(context.Object, GetVirtualPathProvider(), 100, 100);
            chart.Write("jpeg");

            byte[] a = stream.GetBuffer();

            stream.SetLength(0);
            chart.Write("jpg");
            byte[] b = stream.GetBuffer();

            CollectionAssert.AreEqual(a, b);
        }

        [TestMethod]
        public void WriteThrowsWithInvalidFormat() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgumentException(() => chart.Write("foo"),
                "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [TestMethod]
        public void SeriesOverrides() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .AddSeries(chartType: "Bar");
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.Series.Count);
                Assert.AreEqual(DV.SeriesChartType.Bar, c.Series[0].ChartType);
            });
        }

        [TestMethod]
        public void SeriesThrowsWhenChartTypeIsEmpty() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.AddSeries(chartType: "");
            }, "chartType");
        }

        [TestMethod]
        public void SeriesThrowsWhenChartTypeIsNull() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            ExceptionAssert.ThrowsArgNullOrEmpty(() => {
                chart.AddSeries(chartType: null);
            }, "chartType");
        }

        [TestMethod]
        public void TitleDefaults() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddTitle();
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(1, c.Titles.Count);
                // NOTE: Chart.Titles.Add will create default name
                Assert.AreEqual("Title1", c.Titles[0].Name);
                Assert.AreEqual(String.Empty, c.Titles[0].Text);
                Assert.AreEqual(1, c.Titles[0].BorderWidth);
            });
        }

        [TestMethod]
        public void TitleOverrides() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddTitle(name: "Title1")
                .AddTitle("Title2Text", name: "Title2");
            AssertBuiltChartAction(chart, c => {
                Assert.AreEqual(2, c.Titles.Count);
                Assert.AreEqual("Title1", c.Titles[0].Name);
                Assert.AreEqual("Title2", c.Titles[1].Name);
                Assert.AreEqual("Title2Text", c.Titles[1].Text);
            });
        }

        [TestMethod]
        public void ToWebImage() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            var image = chart.ToWebImage();
            Assert.IsNotNull(image);
            Assert.AreEqual("jpeg", image.ImageFormat);
        }

        [TestMethod]
        public void ToWebImageUsesFormat() {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            var image = chart.ToWebImage(format: "png");
            Assert.IsNotNull(image);
            Assert.AreEqual("png", image.ImageFormat);
        }

        [TestMethod]
        public void WriteFromCacheIsNoOpIfNotSavedInCache() {
            var context = GetContext();
            Assert.IsNull(Chart.WriteFromCache(context, Guid.NewGuid().ToString()));
            Assert.IsNull(context.Response.ContentType);
        }



        [TestMethod]
        public void WriteUpdatesResponse() {
            var context = GetContext();
            var chart = new Chart(context, GetVirtualPathProvider(), 100, 100);
            chart.Write();
            Assert.AreEqual("", context.Response.Charset);
            Assert.AreEqual("image/jpeg", context.Response.ContentType);
            Assert.IsTrue((_writeData != null) && (_writeData.Length > 0));
        }

        private void AssertBuiltChartAction(Chart chart, Action<DV.Chart> action) {
            bool actionCalled = false;
            chart.ExecuteChartAction(c => {
                action(c);
                actionCalled = true;
            });
            Assert.IsTrue(actionCalled);
        }

        private HttpContextBase GetContext() {
            // Strip drive letter for VirtualPathUtility.Combine
            var testPath = Directory.GetCurrentDirectory().Substring(2) + "/Out";
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns(testPath);
            request.Setup(r => r.MapPath(It.IsAny<string>())).Returns((string path) => path);

            Mock<HttpResponseBase> response = new Mock<HttpResponseBase>();
            response.SetupProperty(r => r.ContentType);
            response.SetupProperty(r => r.Charset);
            response.Setup(r => r.BinaryWrite(It.IsAny<byte[]>())).Callback((byte[] data) => _writeData = data);

            Mock<HttpServerUtilityBase> server = new Mock<HttpServerUtilityBase>();
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns((string s) => s);

            var items = new Hashtable();
            

            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Server).Returns(server.Object);
            context.Setup(c => c.Items).Returns(items);
            return context.Object;
        }

        private string WriteTemplate(string xml) {
            var path = Guid.NewGuid() + ".xml";
            File.WriteAllText(path, xml);
            return path;
        }

        private MockVirtualPathProvider GetVirtualPathProvider() {
            return new MockVirtualPathProvider();
        }

        class MockVirtualPathProvider : VirtualPathProvider {
            class MockVirtualFile : VirtualFile {
                public MockVirtualFile(string virtualPath) : base(virtualPath) { }
                public override Stream Open() {
                    return File.Open(this.VirtualPath, FileMode.Open);
                }
            }
            public override bool FileExists(string virtualPath) {
                return File.Exists(virtualPath);
            }
            public override VirtualFile GetFile(string virtualPath) {
                return new MockVirtualFile(virtualPath);
            }
        }

    }
}

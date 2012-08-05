using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using System.Web.WebPages.Razor.Configuration;
using System.Web.WebPages.Razor.Resources;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.Razor.Test {
    [TestClass]
    public class WebRazorHostFactoryTest {
        public class TestFactory : WebRazorHostFactory {
            public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath = null) {
                return new TestHost();
            }
        }

        public class TestHost : WebPageRazorHost {
            public TestHost() : base("Foo.cshtml") { }

            public new void RegisterSpecialFile(string fileName, Type baseType ) {
                base.RegisterSpecialFile(fileName, baseType);
            }

            public new void RegisterSpecialFile(string fileName, string baseType) {
                base.RegisterSpecialFile(fileName, baseType);
            }

        }

        [TestMethod]
        public void CreateHostReturnsWebPageHostWithWebPageAsBaseClassIfVirtualPathIsNormalPage() {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
            Assert.AreEqual(WebPageRazorHost.PageBaseClass, host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostReturnsWebPageHostWithInitPageAsBaseClassIfVirtualPathIsPageStart() {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/_pagestart.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
            Assert.AreEqual(typeof(StartPage).FullName, host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostReturnsWebPageHostWithStartPageAsBaseClassIfVirtualPathIsAppStart() {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/_appstart.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
            Assert.AreEqual(typeof(ApplicationStartPage).FullName, host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostPassesPhysicalPathOnToWebCodeRazorHost() {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz/App_Code/Bar", @"C:\Foo.cshtml");

            // Assert
            Assert.AreEqual(@"C:\Foo.cshtml", host.PhysicalPath);
        }

        [TestMethod]
        public void CreateHostPassesPhysicalPathOnToWebPageRazorHost() {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz/Bar", @"C:\Foo.cshtml");

            // Assert
            Assert.AreEqual(@"C:\Foo.cshtml", host.PhysicalPath);
        }

        [TestMethod]
        public void CreateHostFromConfigRequiresNonNullVirtualPath() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebRazorHostFactory.CreateHostFromConfig(virtualPath: null, 
                                                                                                physicalPath: "foo"), "virtualPath");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebRazorHostFactory.CreateHostFromConfig(config: new RazorWebSectionGroup(),
                                                                                                virtualPath: null,
                                                                                                physicalPath: "foo"), "virtualPath");
        }

        [TestMethod]
        public void CreateHostFromConfigRequiresNonEmptyVirtualPath() {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebRazorHostFactory.CreateHostFromConfig(virtualPath: String.Empty,
                                                                                                physicalPath: "foo"), "virtualPath");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => WebRazorHostFactory.CreateHostFromConfig(config: new RazorWebSectionGroup(),
                                                                                                virtualPath: String.Empty,
                                                                                                physicalPath: "foo"), "virtualPath");
        }

        [TestMethod]
        public void CreateHostFromConfigRequiresNonNullSectionGroup() {
            ExceptionAssert.ThrowsArgNull(() => WebRazorHostFactory.CreateHostFromConfig(config: (RazorWebSectionGroup)null,
                                                                                         virtualPath: String.Empty,
                                                                                         physicalPath: "foo"), "config");

        }

        [TestMethod]
        public void CreateHostFromConfigReturnsWebCodeHostIfVirtualPathStartsWithAppCode() {
            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfigCore(null, "~/App_Code/Bar.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebCodeRazorHost));
        }

        [TestMethod]
        public void CreateHostFromConfigUsesDefaultFactoryIfNoRazorWebSectionGroupFound() {
            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfigCore(null, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
        }

        [TestMethod]
        public void CreateHostFromConfigUsesDefaultFactoryIfNoHostSectionFound() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() { 
                Host = null,
                Pages = null
            };

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
        }

        [TestMethod]
        public void CreateHostFromConfigUsesDefaultFactoryIfNullFactoryType() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = new HostSection() {
                    FactoryType = null
                },
                Pages = null
            };

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(WebPageRazorHost));
        }

        [TestMethod]
        public void CreateHostFromConfigUsesFactorySpecifiedInConfig() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = new HostSection() {
                    FactoryType = typeof(TestFactory).FullName
                },
                Pages = null
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsInstanceOfType(host, typeof(TestHost));
        }

        [TestMethod]
        public void CreateHostFromConfigThrowsInvalidOperationExceptionIfFactoryTypeNotFound() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = new HostSection() {
                    FactoryType = "Foo"
                },
                Pages = null
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            ExceptionAssert.Throws<InvalidOperationException>(
                () => WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null),
                String.Format(RazorWebResources.Could_Not_Locate_FactoryType, "Foo"));
        }

        [TestMethod]
        public void CreateHostFromConfigAppliesBaseTypeFromConfigToHost() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = null,
                Pages = new RazorPagesSection() {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.AreEqual("System.Foo.Bar", host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostFromConfigIgnoresBaseTypeFromConfigIfPageIsPageStart() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = null,
                Pages = new RazorPagesSection() {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/_pagestart.cshtml", null);

            // Assert
            Assert.AreEqual(typeof(StartPage).FullName, host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostFromConfigIgnoresBaseTypeFromConfigIfPageIsAppStart() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = null,
                Pages = new RazorPagesSection() {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/_appstart.cshtml", null);

            // Assert
            Assert.AreEqual(typeof(ApplicationStartPage).FullName, host.DefaultBaseClass);
        }

        [TestMethod]
        public void CreateHostFromConfigMergesNamespacesFromConfigToHost() {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup() {
                Host = null,
                Pages = new RazorPagesSection() {
                    Namespaces = new NamespaceCollection() {
                        new NamespaceInfo("System"),
                        new NamespaceInfo("Foo")
                    }
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsTrue(host.NamespaceImports.Contains("System"));
            Assert.IsTrue(host.NamespaceImports.Contains("Foo"));
        }

        [TestMethod]
        public void RegisterSpecialFile_ThrowsOnNullFileName() {
            TestHost host = new TestHost();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile(null, typeof(string)), "fileName");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile(null, "string"), "fileName");
        }

        [TestMethod]
        public void RegisterSpecialFile_ThrowsOnEmptyFileName() {
            TestHost host = new TestHost();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile(String.Empty, typeof(string)), "fileName");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile(String.Empty, "string"), "fileName");
        }

        [TestMethod]
        public void RegisterSpecialFile_ThrowsOnNullBaseType() {
            TestHost host = new TestHost();
            ExceptionAssert.ThrowsArgNull(() => host.RegisterSpecialFile("file", (Type)null), "baseType");
        }

        [TestMethod]
        public void RegisterSpecialFile_ThrowsOnNullBaseTypeName() {
            TestHost host = new TestHost();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile("file", (string)null), "baseTypeName");
        }

        [TestMethod]
        public void RegisterSpecialFile_ThrowsOnEmptyBaseTypeName() {
            TestHost host = new TestHost();
            ExceptionAssert.ThrowsArgNullOrEmpty(() => host.RegisterSpecialFile("file", String.Empty), "baseTypeName");
        }

        private static RazorWebSectionGroup GetRazorGroup() {
            return (RazorWebSectionGroup)ConfigurationManager.OpenExeConfiguration(null).GetSectionGroup(RazorWebSectionGroup.GroupName);
        }
    }
}

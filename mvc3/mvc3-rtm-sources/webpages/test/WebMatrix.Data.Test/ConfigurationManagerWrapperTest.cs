namespace WebMatrix.Data.Test {
    using System;
    using System.Collections.Generic;
    using WebMatrix.Data.Test.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages.TestUtils;
    using Moq;

    [TestClass]
    public class ConfigurationManagerWrapperTest {
        [TestMethod]
        public void GetConnectionGetsConnectionFromConfig() {
            // Arrange            
            var configManager = new ConfigurationManagerWrapper(new Dictionary<string, IDbFileHandler>(), "DataDirectory");
            Func<string, bool> fileExists = path => false;
            Func<string, IConnectionConfiguration> getFromConfig = name => new MockConnectionConfiguration("connection string");

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("foo", getFromConfig, fileExists);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.AreEqual("connection string", configuration.ConnectionString);
        }

        [TestMethod]
        public void GetConnectionGetsConnectionFromDataDirectoryIfFileWithSupportedExtensionExists() {
            // Arrange   
            var mockHandler = new Mock<MockDbFileHandler>();
            mockHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.foo")).Returns(new MockConnectionConfiguration("some file based connection"));
            var handlers = new Dictionary<string, IDbFileHandler> {
                {".foo", mockHandler.Object }
            };
            var configManager = new ConfigurationManagerWrapper(handlers, "DataDirectory");
            Func<string, bool> fileExists = path => path.Equals(@"DataDirectory\Bar.foo");
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("Bar", getFromConfig, fileExists);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.AreEqual("some file based connection", configuration.ConnectionString);
        }
        
        [TestMethod]
        public void GetConnectionSdfAndMdfFile_MdfFileWins() {
            // Arrange
            var mockSdfHandler = new Mock<MockDbFileHandler>();
            mockSdfHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.sdf")).Returns(new MockConnectionConfiguration("sdf connection"));
            var mockMdfHandler = new Mock<MockDbFileHandler>();
            mockMdfHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.mdf")).Returns(new MockConnectionConfiguration("mdf connection"));
            var handlers = new Dictionary<string, IDbFileHandler> {
                {".sdf", mockSdfHandler.Object },
                {".mdf", mockMdfHandler.Object },
            };
            var configManager = new ConfigurationManagerWrapper(handlers, "DataDirectory");
            Func<string, bool> fileExists = path => path.Equals(@"DataDirectory\Bar.mdf") || 
                                                    path.Equals(@"DataDirectory\Bar.sdf");
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("Bar", getFromConfig, fileExists);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.AreEqual("mdf connection", configuration.ConnectionString);
        }

        [TestMethod]
        public void GetConnectionReturnsNullIfNoConnectionFound() {
            // Act
            var configManager = new ConfigurationManagerWrapper(new Dictionary<string, IDbFileHandler>(), "DataDirectory");            
            Func<string, bool> fileExists = path => false;
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("test", getFromConfig, fileExists);

            // Assert
            Assert.IsNull(configuration);
        }
    }
}

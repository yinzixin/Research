namespace WebMatrix.Data.Test {
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileHandlerTest {
        [TestMethod]
        public void SqlCeFileHandlerReturnsDataDirectoryRelativeConnectionStringIfPathIsNotRooted() {
            // Act
            string connectionString = SqlCeDbFileHandler.GetConnectionString("foo.sdf");

            // Assert
            Assert.IsNotNull(connectionString);
            Assert.AreEqual(@"Data Source=|DataDirectory|\foo.sdf", connectionString);
        }

        [TestMethod]
        public void SqlCeFileHandlerReturnsFullPathConnectionStringIfPathIsNotRooted() {
            // Act
            string connectionString = SqlCeDbFileHandler.GetConnectionString(@"c:\foo.sdf");

            // Assert
            Assert.IsNotNull(connectionString);
            Assert.AreEqual(@"Data Source=c:\foo.sdf", connectionString);
        }

        [TestMethod]
        public void SqlServerFileHandlerReturnsDataDirectoryRelativeConnectionStringIfPathIsNotRooted() {
            // Act           
            string connectionString = SqlServerDbFileHandler.GetConnectionString("foo.mdf", "datadir");

            // Assert
            Assert.IsNotNull(connectionString);
            Assert.AreEqual(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\foo.mdf;Initial Catalog=datadir\foo.mdf;Integrated Security=True;User Instance=True;MultipleActiveResultSets=True", connectionString);
        }

        [TestMethod]
        public void SqlServerFileHandlerReturnsFullPathConnectionStringIfPathIsNotRooted() {
            // Act
            string connectionString = SqlServerDbFileHandler.GetConnectionString(@"c:\foo.mdf", "datadir");

            // Assert
            Assert.IsNotNull(connectionString);
            Assert.AreEqual(@"Data Source=.\SQLEXPRESS;AttachDbFilename=c:\foo.mdf;Initial Catalog=c:\foo.mdf;Integrated Security=True;User Instance=True;MultipleActiveResultSets=True", connectionString);
        }
    }
}

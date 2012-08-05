using System.IO;
using System.Web.WebPages;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Helpers.Test {
    /// <summary>
    ///This is a test class for Util is intended
    ///to contain all HelperResult Unit Tests
    ///</summary>
    [TestClass()]
    public class HelperResultTest {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod]
        public void HelperResultConstructorNullTest() {
            ExceptionAssert.ThrowsArgNull(() => { var helper = new HelperResult(null); }, "action");
        }

        [TestMethod]
        public void ToStringTest() {
            var text = "Hello";
            Action<TextWriter> action = tw => tw.Write(text);
            var helper = new HelperResult(action);
            Assert.AreEqual(text, helper.ToString());
        }

        [TestMethod]
        public void WriteToTest() {
            var text = "Hello";
            Action<TextWriter> action = tw => tw.Write(text);
            var helper = new HelperResult(action);
            var writer = new StringWriter();
            helper.WriteTo(writer);
            Assert.AreEqual(text, writer.ToString());
        }

        [TestMethod]
        public void ToHtmlStringDoesNotEncode() {
            // Arrange
            string text = "<strong>This is a test & it uses html.</strong>";
            Action<TextWriter> action = writer => writer.Write(text);
            HelperResult helperResult = new HelperResult(action);

            // Act
            string result = helperResult.ToHtmlString();

            // Assert
            Assert.AreEqual(result, text);
        }

        [TestMethod]
        public void ToHtmlStringReturnsSameResultAsWriteTo() {
            // Arrange
            string text = "<strong>This is a test & it uses html.</strong>";
            Action<TextWriter> action = writer => writer.Write(text);
            HelperResult helperResult = new HelperResult(action);
            StringWriter stringWriter = new StringWriter();
            
            // Act
            string htmlString = helperResult.ToHtmlString();
            helperResult.WriteTo(stringWriter);

            // Assert
            Assert.AreEqual(htmlString, stringWriter.ToString());
        }
    }
}
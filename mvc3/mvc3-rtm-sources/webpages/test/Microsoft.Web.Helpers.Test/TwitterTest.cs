using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.WebPages.TestUtils;
using System.Web;
using System.Web.Helpers.Test;


namespace Microsoft.Web.Helpers.Test
{
    
    
    /// <summary>
    ///This is a test class for TwitterTest and is intended
    ///to contain all TwitterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TwitterTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
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


        /// <summary>
        ///A test for Profile
        ///</summary>
        [TestMethod()]
        public void Profile_ReturnsValidData()
        {
            string[] insertedStrings = new string[9];
            for (int iter = 0; iter < insertedStrings.Length; iter++)
                insertedStrings[iter] = "string_" + iter.ToString();
            int pos = 0;
            string twitterUserName = insertedStrings[pos++]; 
            int width = 100; 
            int height = 100; 
            string backgroundShellColor = insertedStrings[pos++]; 
            string shellColor = insertedStrings[pos++]; 
            string tweetsBackgroundColor = insertedStrings[pos++]; 
            string tweetsColor = insertedStrings[pos++]; 
            string tweetsLinksColor = insertedStrings[pos++]; 
            bool scrollBar = false; 
            bool loop = false; 
            bool live = false; 
            bool hashTags = false; 
            bool timestamp = false; 
            bool avatars = false; 
            string behavior = insertedStrings[pos++]; 
            int searchInterval = 10; 
            string actual = Twitter.Profile(twitterUserName, width, height, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor, 5, scrollBar, loop, live, hashTags, timestamp, avatars, behavior, searchInterval).ToString();
            for (int iter = 0; iter < pos; iter++) {
                Assert.IsTrue(actual.Contains(insertedStrings[iter]));
            }
        }

        /// <summary>
        ///A test for Search
        ///</summary>
        [TestMethod()]
        public void Search_ReturnsValidData()
        {
            string[] insertedStrings = new string[9];
            for (int iter = 0; iter < insertedStrings.Length; iter++)
                insertedStrings[iter] = "string_" + iter.ToString();
            int pos = 0;
            string search = insertedStrings[pos++]; 
            int width = 100; 
            int height = 100; 
            string title = insertedStrings[pos++]; 
            string caption = insertedStrings[pos++]; 
            string backgroundShellColor = insertedStrings[pos++]; 
            string shellColor = insertedStrings[pos++]; 
            string tweetsBackgroundColor = insertedStrings[pos++]; 
            string tweetsColor = insertedStrings[pos++]; 
            string tweetsLinksColor = insertedStrings[pos++]; 
            bool scrollBar = false; 
            bool loop = false; 
            bool live = false; 
            bool hashTags = false; 
            bool timestamp = false; 
            bool avatars = false; 
            string behavior = insertedStrings[pos++]; 
            int searchInterval = 10; 
            string actual = Twitter.Search(search, width, height, title, caption, backgroundShellColor, shellColor, tweetsBackgroundColor, tweetsColor, tweetsLinksColor, scrollBar, loop, live, hashTags, timestamp, avatars, behavior, searchInterval).ToString();
            for (int iter = 0; iter < pos; iter++) {
                Assert.IsTrue(actual.Contains(insertedStrings[iter]));
            }
        }

        [TestMethod()]
        public void SearchWithInvalidArgs_ThrowsArgumentException()
        {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search(null), "searchQuery");
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Search("any term", width: -1));
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Search("any term", height: -1));
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Search("any term", searchInterval: 0));

            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", backgroundShellColor: null), "backgroundShellColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", shellColor: null), "shellColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", tweetsBackgroundColor: null), "tweetsBackgroundColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", tweetsColor: null), "tweetsColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", tweetsLinksColor: null), "tweetsLinksColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Search("any term", behavior: null), "behavior");
        }

        [TestMethod()]
        public void ProfileWithInvalidArgs_ThrowsArgumentException()
        {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile(null), "twitterUserName");
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Profile("anyName", width: -1));
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Profile("anyName", height: -1));
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Profile("anyName", searchInterval: 0));
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => Twitter.Profile("anyName", numberOfTweets: 0));

            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", backgroundShellColor: null), "backgroundShellColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", shellColor: null), "shellColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", tweetsBackgroundColor: null), "tweetsBackgroundColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", tweetsColor: null), "tweetsColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", tweetsLinksColor: null), "tweetsLinksColor");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Twitter.Profile("anyName", behavior: null), "behavior");
        }

        [TestMethod()]
        public void SearchEncodesSearchTerms() {
            // Act 
            string result = Twitter.Search("'any term'", backgroundShellColor: "\"bad-color").ToString();

            // Assert
            Assert.IsTrue(result.Contains(@"background: ""\""bad-color"","));
            Assert.IsTrue(result.Contains(@"search: ""&#39;any term&#39;"","));
        }

        [TestMethod()]
        public void ProfileEncodesSearchTerms() {
            // Act 
            string result = Twitter.Profile("'some user'", backgroundShellColor: "\"malformed-color").ToString();

            // Assert
            Assert.IsTrue(result.Contains(@"background: ""\""malformed-color"","));
            Assert.IsTrue(result.Contains(@"setUser(""&#39;some user&#39;"")"));
        }
    }
}

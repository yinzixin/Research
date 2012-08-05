using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.WebPages.Test.Helpers {
    [TestClass]
    public class UnvalidatedRequestValuesTest {

        [TestMethod]
        public void Constructor_SetsPropertiesCorrectly() {
            // Arrange
            NameValueCollection expectedForm = new NameValueCollection();
            NameValueCollection expectedQueryString = new NameValueCollection();

            // Act
            UnvalidatedRequestValues unvalidatedValues = new UnvalidatedRequestValues(null, () => expectedForm, () => expectedQueryString);

            // Assert
            Assert.AreSame(expectedForm, unvalidatedValues.Form, "Form was not set correctly.");
            Assert.AreSame(expectedQueryString, unvalidatedValues.QueryString, "QueryString was not set correctly.");
        }

        [TestMethod]
        public void Indexer_LooksUpValuesInCorrectOrder() {
            // Order should be QueryString, Form, Cookies, ServerVariables

            // Arrange
            NameValueCollection queryString = new NameValueCollection() {
                { "foo", "fooQueryString" }
            };

            NameValueCollection form = new NameValueCollection() {
                { "foo", "fooForm" },
                { "bar", "barForm" },
            };

            HttpCookieCollection cookies = new HttpCookieCollection() {
                new HttpCookie("foo", "fooCookie"),
                new HttpCookie("bar", "barCookie"),
                new HttpCookie("baz", "bazCookie")
            };

            NameValueCollection serverVars = new NameValueCollection() {
                { "foo", "fooServerVars" },
                { "bar", "barServerVars" },
                { "baz", "bazServerVars" },
                { "quux", "quuxServerVars" },
            };
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(o => o.Cookies).Returns(cookies);
            mockRequest.Setup(o => o.ServerVariables).Returns(serverVars);

            UnvalidatedRequestValues unvalidatedValues = new UnvalidatedRequestValues(mockRequest.Object, () => form, () => queryString);

            // Act
            string fooValue = unvalidatedValues["foo"];
            string barValue = unvalidatedValues["bar"];
            string bazValue = unvalidatedValues["baz"];
            string quuxValue = unvalidatedValues["quux"];
            string notFoundValue = unvalidatedValues["not-found"];

            // Assert
            Assert.AreEqual("fooQueryString", fooValue);
            Assert.AreEqual("barForm", barValue);
            Assert.AreEqual("bazCookie", bazValue);
            Assert.AreEqual("quuxServerVars", quuxValue);
            Assert.IsNull(notFoundValue);
        }

    }
}

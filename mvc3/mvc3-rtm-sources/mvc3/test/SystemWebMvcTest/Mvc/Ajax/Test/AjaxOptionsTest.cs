namespace System.Web.Mvc.Ajax.Test {
    using System.Collections.Generic;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AjaxOptionsTest {

        [TestMethod]
        public void InsertionModeProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestEnumProperty(options, "InsertionMode", InsertionMode.Replace, false);
        }

        [TestMethod]
        public void InsertionModePropertyExceptionText() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            ExceptionHelper.ExpectArgumentOutOfRangeException(
                delegate {
                    options.InsertionMode = (InsertionMode)4;
                },
                "value",
                @"Specified argument was out of the range of valid values.
Parameter name: value");
        }

        [TestMethod]
        public void InsertionModeStringTests() {
            // Act & Assert
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.Replace }.InsertionModeString, "Sys.Mvc.InsertionMode.replace");
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.InsertAfter }.InsertionModeString, "Sys.Mvc.InsertionMode.insertAfter");
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.InsertBefore }.InsertionModeString, "Sys.Mvc.InsertionMode.insertBefore");
        }

        [TestMethod]
        public void InsertionModeUnobtrusiveTests() {
            // Act & Assert
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.Replace }.InsertionModeUnobtrusive, "replace");
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.InsertAfter }.InsertionModeUnobtrusive, "after");
            Assert.AreEqual(new AjaxOptions { InsertionMode = InsertionMode.InsertBefore }.InsertionModeUnobtrusive, "before");
        }

        [TestMethod]
        public void HttpMethodProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestStringProperty(options, "HttpMethod", String.Empty);
        }

        [TestMethod]
        public void OnBeginProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestStringProperty(options, "OnBegin", String.Empty);
        }

        [TestMethod]
        public void OnFailureProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestStringProperty(options, "OnFailure", String.Empty);
        }

        [TestMethod]
        public void OnSuccessProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestStringProperty(options, "OnSuccess", String.Empty);
        }

        [TestMethod]
        public void ToJavascriptStringWithEmptyOptions() {
            string s = (new AjaxOptions()).ToJavascriptString();
            Assert.AreEqual("{ insertionMode: Sys.Mvc.InsertionMode.replace }", s);
        }

        [TestMethod]
        public void ToJavascriptString() {
            // Arrange
            AjaxOptions options = new AjaxOptions {
                InsertionMode = InsertionMode.InsertBefore,
                Confirm = "confirm",
                HttpMethod = "POST",
                LoadingElementId = "loadingElement",
                UpdateTargetId = "someId",
                Url = "http://someurl.com",
                OnBegin = "some_begin_function",
                OnComplete = "some_complete_function",
                OnFailure = "some_failure_function",
                OnSuccess = "some_success_function",
            };

            // Act
            string s = options.ToJavascriptString();

            // Assert
            Assert.AreEqual("{ insertionMode: Sys.Mvc.InsertionMode.insertBefore, " +
                            "confirm: 'confirm', " +
                            "httpMethod: 'POST', " +
                            "loadingElementId: 'loadingElement', " +
                            "updateTargetId: 'someId', " +
                            "url: 'http://someurl.com', " +
                            "onBegin: Function.createDelegate(this, some_begin_function), " +
                            "onComplete: Function.createDelegate(this, some_complete_function), " +
                            "onFailure: Function.createDelegate(this, some_failure_function), " +
                            "onSuccess: Function.createDelegate(this, some_success_function) }", s);
        }

        [TestMethod]
        public void ToJavascriptStringEscapesQuotesCorrectly() {
            // Arrange
            AjaxOptions options = new AjaxOptions {
                InsertionMode = InsertionMode.InsertBefore,
                Confirm = @"""confirm""",
                HttpMethod = "POST",
                LoadingElementId = "loading'Element'",
                UpdateTargetId = "someId",
                Url = "http://someurl.com",
                OnBegin = "some_begin_function",
                OnComplete = "some_complete_function",
                OnFailure = "some_failure_function",
                OnSuccess = "some_success_function",
            };

            // Act
            string s = options.ToJavascriptString();

            // Assert
            Assert.AreEqual("{ insertionMode: Sys.Mvc.InsertionMode.insertBefore, " +
                            @"confirm: '""confirm""', " +
                            "httpMethod: 'POST', " +
                            @"loadingElementId: 'loading\'Element\'', " +
                            "updateTargetId: 'someId', " +
                            "url: 'http://someurl.com', " +
                            "onBegin: Function.createDelegate(this, some_begin_function), " +
                            "onComplete: Function.createDelegate(this, some_complete_function), " +
                            "onFailure: Function.createDelegate(this, some_failure_function), " +
                            "onSuccess: Function.createDelegate(this, some_success_function) }", s);
        }

        [TestMethod]
        public void ToJavascriptStringWithOnlyUpdateTargetId() {
            // Arrange
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "someId" };

            // Act
            string s = options.ToJavascriptString();

            // Assert
            Assert.AreEqual("{ insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: 'someId' }", s);
        }

        [TestMethod]
        public void ToJavascriptStringWithUpdateTargetIdAndExplicitInsertionMode() {
            // Arrange
            AjaxOptions options = new AjaxOptions { InsertionMode = InsertionMode.InsertAfter, UpdateTargetId = "someId" };

            // Act
            string s = options.ToJavascriptString();

            // Assert
            Assert.AreEqual("{ insertionMode: Sys.Mvc.InsertionMode.insertAfter, updateTargetId: 'someId' }", s);
        }

        [TestMethod]
        public void ToUnobtrusiveHtmlAttributesWithEmptyOptions() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act
            IDictionary<string, object> attributes = options.ToUnobtrusiveHtmlAttributes();

            // Assert
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("true", attributes["data-ajax"]);
        }

        [TestMethod]
        public void ToUnobtrusiveHtmlAttributes() {
            // Arrange
            AjaxOptions options = new AjaxOptions {
                InsertionMode = InsertionMode.InsertBefore,
                Confirm = "confirm",
                HttpMethod = "POST",
                LoadingElementId = "loadingElement",
                LoadingElementDuration = 450,
                UpdateTargetId = "someId",
                Url = "http://someurl.com",
                OnBegin = "some_begin_function",
                OnComplete = "some_complete_function",
                OnFailure = "some_failure_function",
                OnSuccess = "some_success_function",
            };

            // Act
            var attributes = options.ToUnobtrusiveHtmlAttributes();

            // Assert
            Assert.AreEqual(12, attributes.Count);
            Assert.AreEqual("true", attributes["data-ajax"]);
            Assert.AreEqual("confirm", attributes["data-ajax-confirm"]);
            Assert.AreEqual("POST", attributes["data-ajax-method"]);
            Assert.AreEqual("#loadingElement", attributes["data-ajax-loading"]);
            Assert.AreEqual(450, attributes["data-ajax-loading-duration"]);
            Assert.AreEqual("http://someurl.com", attributes["data-ajax-url"]);
            Assert.AreEqual("#someId", attributes["data-ajax-update"]);
            Assert.AreEqual("before", attributes["data-ajax-mode"]);
            Assert.AreEqual("some_begin_function", attributes["data-ajax-begin"]);
            Assert.AreEqual("some_complete_function", attributes["data-ajax-complete"]);
            Assert.AreEqual("some_failure_function", attributes["data-ajax-failure"]);
            Assert.AreEqual("some_success_function", attributes["data-ajax-success"]);
        }

        [TestMethod]
        public void ToUnobtrusiveHtmlAttributesWithOnlyUpdateTargetId() {
            // Arrange
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "someId" };

            // Act
            var attributes = options.ToUnobtrusiveHtmlAttributes();

            // Assert
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual("true", attributes["data-ajax"]);
            Assert.AreEqual("#someId", attributes["data-ajax-update"]);
            Assert.AreEqual("replace", attributes["data-ajax-mode"]);  // Only added when UpdateTargetId is set
        }

        [TestMethod]
        public void ToUnobtrusiveHtmlAttributesWithUpdateTargetIdAndExplicitInsertionMode() {
            // Arrange
            AjaxOptions options = new AjaxOptions {
                InsertionMode = InsertionMode.InsertAfter,
                UpdateTargetId = "someId"
            };

            // Act
            var attributes = options.ToUnobtrusiveHtmlAttributes();

            // Assert
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual("true", attributes["data-ajax"]);
            Assert.AreEqual("#someId", attributes["data-ajax-update"]);
            Assert.AreEqual("after", attributes["data-ajax-mode"]);
        }

        [TestMethod]
        public void UpdateTargetIdProperty() {
            // Arrange
            AjaxOptions options = new AjaxOptions();

            // Act & Assert
            MemberHelper.TestStringProperty(options, "UpdateTargetId", String.Empty);
        }

    }
}

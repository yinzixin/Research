namespace Microsoft.Web.Helpers.Test {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.Helpers.Test;
    using System.Web.WebPages.Scope;
    using System.Web.WebPages.TestUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReCaptchaTest {

        [TestMethod]
        public void ReCaptchaOptionsMissingWhenNoOptionsAndDefaultRendering() {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY");
            AssertEqualsIgnoreLineBreaks(
                @"<script src=""http://api.recaptcha.net/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""http://api.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void ReCaptchaOptionsWhenOneOptionAndDefaultRendering() {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: new { theme = "white" });
            AssertEqualsIgnoreLineBreaks(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white""}</script>" +
                @"<script src=""http://api.recaptcha.net/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""http://api.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void ReCaptchaOptionsWhenMultipleOptionsAndDefaultRendering() {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: new { theme = "white", tabindex = 5 });
            AssertEqualsIgnoreLineBreaks(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white"",""tabindex"":5}</script>" +
                @"<script src=""http://api.recaptcha.net/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""http://api.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void ReCaptchaOptionsWhenMultipleOptionsFromDictionaryAndDefaultRendering() {
            // verifies that a dictionary will serialize the same as a projection
            var options = new Dictionary<string, object>() { { "theme", "white" }, { "tabindex", 5 } };
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: options);
            AssertEqualsIgnoreLineBreaks(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white"",""tabindex"":5}</script>" +
                @"<script src=""http://api.recaptcha.net/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""http://api.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void RenderUsesLastError() {
            HttpContextBase context = GetContext();
            ReCaptcha.HandleValidateResponse(context, "false\nincorrect-captcha-sol");
            var html = ReCaptcha.GetHtml(context, "PUBLIC_KEY");
            AssertEqualsIgnoreLineBreaks(
                @"<script src=""http://api.recaptcha.net/challenge?k=PUBLIC_KEY&amp;error=incorrect-captcha-sol"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""http://api.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void RenderWhenConnectionIsSecure() {
            var html = ReCaptcha.GetHtml(GetContext(isSecure: true), "PUBLIC_KEY");
            AssertEqualsIgnoreLineBreaks(
                @"<script src=""https://api-secure.recaptcha.net/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300"" src=""https://api-secure.recaptcha.net/noscript?k=PUBLIC_KEY"" width=""500""></iframe><br></br>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""></input>" +
                @"</noscript>",
                html);
        }

        [TestMethod]
        public void ValidateThrowsWhenRemoteAddressNotAvailable() {
            HttpContextBase context = GetContext();
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";
            context.Request.Form["recaptcha_response_field"] = "RESPONSE";

            ExceptionAssert.Throws<InvalidOperationException>(() => {
                ReCaptcha.Validate(context, privateKey: "PRIVATE_KEY");
            }, "The captcha cannot be validated because the remote address was not found in the request.");
        }

        [TestMethod]
        public void ValidateReturnsFalseWhenChallengeNotPosted() {
            HttpContextBase context = GetContext();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";

            Assert.IsFalse(ReCaptcha.Validate(context, privateKey: "PRIVATE_KEY"));
        }

        [TestMethod]
        public void ValidatePostData() {
            HttpContextBase context = GetContext();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";
            context.Request.Form["recaptcha_response_field"] = "RESPONSE";

            Assert.AreEqual("privatekey=PRIVATE_KEY&remoteip=127.0.0.1&challenge=CHALLENGE&response=RESPONSE",
                ReCaptcha.GetValidatePostData(context, "PRIVATE_KEY"));
        }

        [TestMethod]
        public void ValidatePostDataWhenNoResponse() {
            HttpContextBase context = GetContext();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";

            Assert.AreEqual("privatekey=PRIVATE_KEY&remoteip=127.0.0.1&challenge=CHALLENGE&response=", ReCaptcha.GetValidatePostData(context, "PRIVATE_KEY"));
        }

        [TestMethod]
        public void ValidateResponseReturnsFalseOnEmptyReCaptchaResponse() {
            HttpContextBase context = GetContext();
            Assert.IsFalse(ReCaptcha.HandleValidateResponse(context, ""));
            Assert.AreEqual(String.Empty, ReCaptcha.GetLastError(context));
        }

        [TestMethod]
        public void ValidateResponseReturnsTrueOnSuccess() {
            HttpContextBase context = GetContext();
            Assert.IsTrue(ReCaptcha.HandleValidateResponse(context, "true\nsuccess"));
            Assert.AreEqual(String.Empty, ReCaptcha.GetLastError(context));
        }

        [TestMethod]
        public void ValidateResponseReturnsFalseOnError() {
            HttpContextBase context = GetContext();
            Assert.IsFalse(ReCaptcha.HandleValidateResponse(context, "false\nincorrect-captcha-sol"));
            Assert.AreEqual("incorrect-captcha-sol", ReCaptcha.GetLastError(context));
        }

        [TestMethod]
        public void ReCapthaPrivateKeyThowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => ReCaptcha.PrivateKey = null, "value");
        }

        [TestMethod]
        public void ReCapthaPrivateKeyUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            ReCaptcha.PrivateKey = value;

            // Assert
            Assert.AreEqual(ReCaptcha.PrivateKey, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[ReCaptcha._privateKey], value);
        }

        [TestMethod]
        public void PublicKeyThowsWhenSetToNull() {
            ExceptionAssert.ThrowsArgNull(() => ReCaptcha.PublicKey = null, "value");
        }

        [TestMethod]
        public void ReCapthaPublicKeyUsesScopeStorage() {
            // Arrange
            var value = "value";

            // Act
            ReCaptcha.PublicKey = value;

            // Assert
            Assert.AreEqual(ReCaptcha.PublicKey, value);
            Assert.AreEqual(ScopeStorage.CurrentScope[ReCaptcha._publicKey], value);
        }

        private HttpContextBase GetContext(bool isSecure=false) {
            // mock HttpRequest
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.IsSecureConnection).Returns(isSecure);
            requestMock.Setup(request => request.Form).Returns(new NameValueCollection());
            requestMock.Setup(request => request.ServerVariables).Returns(new NameValueCollection());

            // mock HttpContext
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            return contextMock.Object;
        }

        private void AssertEqualsIgnoreLineBreaks(string expected, IHtmlString actual) {
            Assert.AreEqual(expected, actual.ToString().Replace("\r\n", ""));
        }

    }

}
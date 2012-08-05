using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.Helpers.Test {
    
    [TestClass]
    public class InfoTest {
        
        [TestMethod]
        public void ConfigurationReturnsExpectedInfo() {
            var configInfo = ServerInfo.Configuration();

            // verification
            // checks only subset of values
            Assert.IsNotNull(configInfo);
            VerifyKey(configInfo, "Machine Name");
            VerifyKey(configInfo, "OS Version");
            VerifyKey(configInfo, "ASP.NET Version");
        }

        [TestMethod]
        public void EnvironmentVariablesReturnsExpectedInfo() {
            var envVariables = ServerInfo.EnvironmentVariables();

            // verification
            // checks only subset of values
            Assert.IsNotNull(envVariables);
            VerifyKey(envVariables, "Path");
            VerifyKey(envVariables, "SystemDrive");
        }

        [TestMethod]
        public void ServerVariablesReturnsExpectedInfoWithNoContext() {
            var serverVariables = ServerInfo.ServerVariables();

            // verification
            // since there is no HttpContext this will be empty
            Assert.IsNotNull(serverVariables);
        }

        [TestMethod]
        public void ServerVariablesReturnsExpectedInfoWthContext() {
            
            var serverVariables = new NameValueCollection();
            serverVariables.Add("foo", "bar");

            var request = new Mock<HttpRequestBase>();
            request.Setup(c => c.ServerVariables).Returns(serverVariables);

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request).Returns(request.Object);

            // verification
            Assert.IsNotNull(serverVariables);

            IDictionary<string, string> returnedValues = ServerInfo.ServerVariables(context.Object);
            Assert.AreEqual(serverVariables.Count, returnedValues.Count);
            foreach (var item in returnedValues) {
                Assert.AreEqual(serverVariables[item.Key], item.Value);
            }
        }

        [TestMethod]
        public void HttpRuntimeInfoReturnsExpectedInfo() {
            var httpRuntimeInfo = ServerInfo.HttpRuntimeInfo();

            // verification
            // checks only subset of values
            Assert.IsNotNull(httpRuntimeInfo);
            VerifyKey(httpRuntimeInfo, "CLR Install Directory");
            VerifyKey(httpRuntimeInfo, "Asp Install Directory");
            VerifyKey(httpRuntimeInfo, "On UNC Share");
        }

        [TestMethod]
        public void ServerInfoDoesNotProduceLegacyCasForHomogenousAppDomain() {
            // Act and Assert
            Action action = () => {
                IDictionary<string, string> configValue = ServerInfo.LegacyCAS(AppDomain.CurrentDomain);


                Assert.IsNotNull(configValue);
                Assert.AreEqual(0, configValue.Count);
            };
            
            AppDomainUtils.RunInSeparateAppDomain(GetAppDomainSetup(legacyCasEnabled: false), action);
        }

        [TestMethod]
        public void ServerInfoProducesLegacyCasForNonHomogenousAppDomain() {
            // Arrange 
            Action action = () => {
                // Act and Assert
                IDictionary<string, string> configValue = ServerInfo.LegacyCAS(AppDomain.CurrentDomain);

                // Assert
                Assert.IsTrue(configValue.ContainsKey("Legacy Code Access Security"));
                Assert.AreEqual(configValue["Legacy Code Access Security"], "Legacy Code Access Security has been detected on your system. Microsoft WebPage features require the ASP.NET 4 Code Access Security model. For information about how to resolve this, contact your server administrator.");
            };  

            AppDomainUtils.RunInSeparateAppDomain(GetAppDomainSetup(legacyCasEnabled: true), action);
        }

        //[TestMethod]
        //public void SqlServerInfoReturnsExpectedInfo() {
        //    var sqlInfo = ServerInfo.SqlServerInfo();

        //    // verification
        //    // just verifies that we don't get any unexpected exceptions
        //    Assert.IsNotNull(sqlInfo);
        //}

        [TestMethod]
        public void RenderResultContainsExpectedTags() {
            var htmlString = ServerInfo.GetHtml().ToString();

            // just verify that the final HTML produced contains some expected info
            Assert.IsTrue(htmlString.Contains("<table class=\"server-info\" dir=\"ltr\">"));
            Assert.IsTrue(htmlString.Contains("</style>"));
            Assert.IsTrue(htmlString.Contains("Server Configuration"));
        }

        private void VerifyKey(IDictionary<string, string> info, string key) {
            Assert.IsTrue(info.ContainsKey(key));
            Assert.IsFalse(string.IsNullOrEmpty(info[key])); 
        }

        private AppDomainSetup GetAppDomainSetup(bool legacyCasEnabled) {
            var setup = new AppDomainSetup();
            if (legacyCasEnabled) {
                setup.SetCompatibilitySwitches(new[] { "NetFx40_LegacySecurityPolicy" });
            }
            return setup;
        }
    }
}

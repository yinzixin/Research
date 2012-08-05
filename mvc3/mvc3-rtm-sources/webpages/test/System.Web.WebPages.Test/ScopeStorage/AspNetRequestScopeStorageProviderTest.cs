using System.Collections.Generic;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test {
    [TestClass]
    public class AspNetRequestStorageProvider {
        [TestMethod]
        public void AspNetStorageProviderReturnsApplicationStateBeforeAppStart() {
            // Arrange
            var provider = GetProvider(() => false);
            
            // Act and Assert
            Assert.IsNotNull(provider.ApplicationScope);
            Assert.IsNotNull(provider.GlobalScope);
            Assert.AreEqual(provider.ApplicationScope, provider.GlobalScope);
        }

        [TestMethod]
        public void AspNetStorageProviderThrowsWhenAccessingRequestScopeBeforeAppStart() {
            // Arrange
            var provider = GetProvider(() => false);

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => { var x = provider.RequestScope; }, 
                "RequestScope cannot be created when _AppStart is executing.");
        }

        [TestMethod]
        public void AspNetStorageProviderThrowsWhenAssigningScopeBeforeAppStart() {
            // Arrange
            var provider = GetProvider(() => false);

            // Act and Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => { provider.CurrentScope = new ScopeStorageDictionary(); },
                "Storage scopes cannot be created when _AppStart is executing.");
        }

        [TestMethod]
        public void AspNetStorageProviderReturnsRequestScopeAfterAppStart() {
            // Arrange
            var provider = GetProvider();

            // Act and Assert 
            Assert.IsNotNull(provider.RequestScope);
            Assert.AreEqual(provider.RequestScope, provider.CurrentScope);
        }

        [TestMethod]
        public void AspNetStorageRetrievesRequestScopeAfterSettingAnonymousScopes() {
            // Arrange
            var provider = GetProvider();

            // Act 
            var requestScope = provider.RequestScope;
            
            var Scope = new ScopeStorageDictionary();
            provider.CurrentScope = Scope;

            Assert.AreEqual(provider.CurrentScope, Scope);
            Assert.AreEqual(provider.RequestScope, requestScope);
        }

        [TestMethod]
        public void AspNetStorageUsesApplicationScopeAsGlobalScope() {
            // Arrange
            var provider = GetProvider();

            // Act and Assert
            Assert.AreEqual(provider.GlobalScope, provider.ApplicationScope);
        }

        private AspNetRequestScopeStorageProvider GetProvider(Func<bool> appStartExecuted = null) {

            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
            appStartExecuted = appStartExecuted ?? (() => true);

            return new AspNetRequestScopeStorageProvider(context.Object, appStartExecuted);
        }
    }
}

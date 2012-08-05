namespace System.Web.Mvc.Test {
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FilterProvidersTest {
        [TestMethod]
        public void DefaultFilterProviders() {
            // Assert
            Assert.IsNotNull(FilterProviders.Providers.Single(fp => fp is GlobalFilterCollection));
            Assert.IsNotNull(FilterProviders.Providers.Single(fp => fp is FilterAttributeFilterProvider));
            Assert.IsNotNull(FilterProviders.Providers.Single(fp => fp is ControllerInstanceFilterProvider));
        }
    }
}

namespace System.Web.Mvc.Test {
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataAnnotationsModelMetadataProviderTest : DataAnnotationsModelMetadataProviderTestBase {
        protected override AssociatedMetadataProvider MakeProvider() {
            return new DataAnnotationsModelMetadataProvider();
        }
    }
}

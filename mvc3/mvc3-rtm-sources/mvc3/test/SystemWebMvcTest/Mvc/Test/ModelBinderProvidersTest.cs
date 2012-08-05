namespace System.Web.Mvc.Test {
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ModelBinderProvidersTest {

        [TestMethod]
        public void CollectionDefaults() {
            //Arrange
            Type[] expectedTypes = new Type[]{
            };

            //Act
            Type[] actualTypes = ModelBinderProviders.BinderProviders.Select(b => b.GetType()).ToArray();

            //Assert 
            CollectionAssert.AreEqual(expectedTypes, actualTypes);
        }
    }
}

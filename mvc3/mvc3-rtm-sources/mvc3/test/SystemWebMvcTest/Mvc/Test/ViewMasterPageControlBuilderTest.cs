namespace System.Web.Mvc.Test {
    using System.CodeDom;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ViewMasterPageControlBuilderTest {

        [TestMethod]
        public void BuilderWithoutInheritsDoesNothing() {
            // Arrange
            var builder = new ViewMasterPageControlBuilder();
            var derivedType = new CodeTypeDeclaration();
            derivedType.BaseTypes.Add("basetype");

            // Act
            builder.ProcessGeneratedCode(null, null, derivedType, null, null);

            // Assert
            Assert.AreEqual("basetype", derivedType.BaseTypes.Cast<CodeTypeReference>().Single().BaseType);
        }

        [TestMethod]
        public void BuilderWithInheritsSetsBaseType() {
            // Arrange
            var builder = new ViewMasterPageControlBuilder { Inherits = "inheritedtype" };
            var derivedType = new CodeTypeDeclaration();
            derivedType.BaseTypes.Add("basetype");

            // Act
            builder.ProcessGeneratedCode(null, null, derivedType, null, null);

            // Assert
            Assert.AreEqual("inheritedtype", derivedType.BaseTypes.Cast<CodeTypeReference>().Single().BaseType);
        }

    }
}

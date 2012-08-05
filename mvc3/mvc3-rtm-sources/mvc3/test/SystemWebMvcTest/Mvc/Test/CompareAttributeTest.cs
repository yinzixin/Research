namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CompareAttributeTest {

        [TestMethod]
        public void GuardClauses() {
            //Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new CompareAttribute(null);
                }, "otherProperty");

            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                delegate {
                    CompareAttribute.FormatPropertyForClientValidation(null);
                }, "property");
        }

        [TestMethod]
        public void FormatPropertyForClientValidationPrependsStarDot() {
            string prepended = CompareAttribute.FormatPropertyForClientValidation("test");
            Assert.AreEqual(prepended, "*.test");
        }

        [TestMethod]
        public void ValidateDoesNotThrowWhenComparedObjectsAreEqual() {
            object otherObject = new CompareObject("test");
            CompareObject currentObject = new CompareObject("test");
            ValidationContext testContext = new ValidationContext(otherObject, null, null);

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            attr.Validate(currentObject.CompareProperty, testContext);
        }

        [TestMethod]
        public void ValidateThrowsWhenComparedObjectsAreNotEqual() {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            ExceptionHelper.ExpectException<System.ComponentModel.DataAnnotations.ValidationException>(
                delegate {
                    attr.Validate(currentObject.CompareProperty, testContext);
                }, "'CurrentProperty' and 'CompareProperty' do not match.");
        }

        [TestMethod]
        public void ValidateThrowsWhenPropertyNameIsUnknown() {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("UnknownPropertyName");
            ExceptionHelper.ExpectException<System.ComponentModel.DataAnnotations.ValidationException>(
                () => attr.Validate(currentObject.CompareProperty, testContext),
                "Could not find a property named UnknownPropertyName."
            );
        }

        [TestMethod]
        public void GetClientValidationRulesReturnsModelClientValidationEqualToRule() {

            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, null, typeof(string), null);
            metadata.Setup(m => m.DisplayName).Returns("CurrentProperty");

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            List<ModelClientValidationRule> ruleList = new List<ModelClientValidationRule>(attr.GetClientValidationRules(metadata.Object, null));

            Assert.AreEqual(ruleList.Count, 1);

            ModelClientValidationEqualToRule actualRule = ruleList[0] as ModelClientValidationEqualToRule;

            Assert.AreEqual(actualRule.ErrorMessage, "'CurrentProperty' and 'CompareProperty' do not match.", "*.CompareProperty");
            Assert.AreEqual(actualRule.ValidationType, "equalto");
            Assert.AreEqual(actualRule.ValidationParameters["other"], "*.CompareProperty");
        }

        [TestMethod]
        public void CompareAttributeCanBeDerivedFromAndOverrideIsValid() {
            object otherObject = new CompareObject("a");
            CompareObject currentObject = new CompareObject("b");
            ValidationContext testContext = new ValidationContext(otherObject, null, null);

            DerivedCompareAttribute attr = new DerivedCompareAttribute("CompareProperty");
            attr.Validate(currentObject.CompareProperty, testContext);
        } 

        private class DerivedCompareAttribute : CompareAttribute {
            public DerivedCompareAttribute(string otherProperty)
                : base(otherProperty) {
            }

            public override bool IsValid(object value) {
                return false;
            }

            protected override ValidationResult IsValid(object value, ValidationContext context) {
                return null;
            }
        }

        private class CompareObject {
            public string CompareProperty { get; set; }

            public CompareObject(string otherValue) {
                CompareProperty = otherValue;
            }
        }
    }
}

namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;

    [TestClass]
    public class DataAnnotationsModelValidatorTest {

        [TestMethod]
        public void ConstructorGuards() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => new DataAnnotationsModelValidator(null, context, attribute),
                "metadata");
            ExceptionHelper.ExpectArgumentNullException(
                () => new DataAnnotationsModelValidator(metadata, null, attribute),
                "controllerContext");
            ExceptionHelper.ExpectArgumentNullException(
                () => new DataAnnotationsModelValidator(metadata, context, null),
                "attribute");
        }

        [TestMethod]
        public void ValuesSet() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.AreSame(attribute, validator.Attribute);
            Assert.AreEqual(attribute.FormatErrorMessage("Length"), validator.ErrorMessage);
        }

        [TestMethod]
        public void NoClientRulesByDefault() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.IsFalse(validator.GetClientValidationRules().Any());
        }

        [TestMethod]
        public void ValidateWithIsValidTrue() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(true);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void ValidateWithIsValidFalse() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(false);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            var validationResult = result.Single();
            Assert.AreEqual("", validationResult.MemberName);
            Assert.AreEqual(attribute.Object.FormatErrorMessage("Length"), validationResult.Message);
        }

        [TestMethod]
        public void ValidatateWithValidationResultSuccess() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                     .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                     .Returns(ValidationResult.Success);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void IsRequiredTests() {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();

            // Act & Assert
            Assert.IsFalse(new DataAnnotationsModelValidator(metadata, context, new RangeAttribute(10, 20)).IsRequired);
            Assert.IsTrue(new DataAnnotationsModelValidator(metadata, context, new RequiredAttribute()).IsRequired);
            Assert.IsTrue(new DataAnnotationsModelValidator(metadata, context, new DerivedRequiredAttribute()).IsRequired);
        }

        class DerivedRequiredAttribute : RequiredAttribute { }

        [TestMethod]
        public void AttributeWithIClientValidatableGetsClientValidationRules() {
            // Arrange
            var expected = new ModelClientValidationStringLengthRule("Error", 1, 10);
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            var attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.As<IClientValidatable>()
                     .Setup(cv => cv.GetClientValidationRules(metadata, context))
                     .Returns(new[] { expected })
                     .Verifiable();
            var validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            ModelClientValidationRule actual = validator.GetClientValidationRules().Single();

            // Assert
            attribute.Verify();
            Assert.AreSame(expected, actual);
        }
    }
}

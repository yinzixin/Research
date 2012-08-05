namespace Sys.Mvc {
    using System;

    public static class ValidatorRegistry {

        public static Dictionary Validators = GetDefaultValidators();

        public static Validator GetValidator(JsonValidationRule rule) {
            ValidatorCreator creator = (ValidatorCreator)Validators[rule.ValidationType];
            return (creator != null) ? creator(rule) : null;
        }

        private static Dictionary GetDefaultValidators() {
            return new Dictionary(
                "required", (ValidatorCreator)RequiredValidator.Create,
                "length", (ValidatorCreator)StringLengthValidator.Create,
                "regex", (ValidatorCreator)RegularExpressionValidator.Create,
                "range", (ValidatorCreator)RangeValidator.Create,
                "number", (ValidatorCreator)NumberValidator.Create
                );
        }

    }
}

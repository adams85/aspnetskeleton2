namespace System.ComponentModel.DataAnnotations
{
    public static class ValidatableObjectExtensions
    {
        private static readonly ValidationAttribute s_requiredValidatorDisallowingEmptyStrings = new RequiredAttribute();

        private static readonly ValidationAttribute s_requiredValidatorAllowingEmptyStrings = new RequiredAttribute() { AllowEmptyStrings = true };

        public static ValidationResult ValidateMember(this IValidatableObject obj, object? memberValue, string memberName,
            ValidationContext validationContext, ValidationAttribute validationAttribute)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (memberName == null)
                throw new ArgumentNullException(nameof(memberName));

            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));

            if (validationAttribute == null)
                throw new ArgumentNullException(nameof(validationAttribute));

            validationContext = new ValidationContext(obj, validationContext, validationContext.Items) { MemberName = memberName };

            var validationResult = validationAttribute.GetValidationResult(memberValue, validationContext);
            if (validationResult == ValidationResult.Success)
                return validationResult;

            return validationResult is ExtendedValidationResult ? validationResult : new ExtendedValidationResult(validationAttribute, validationResult);
        }

        public static ValidationResult RequireMember(this IValidatableObject obj, object? memberValue, string memberName,
            ValidationContext validationContext, bool allowEmptyStrings = false) =>
            obj.ValidateMember(memberValue, memberName, validationContext, allowEmptyStrings ? s_requiredValidatorAllowingEmptyStrings : s_requiredValidatorDisallowingEmptyStrings);
    }
}

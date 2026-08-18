namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PasswordAttribute : ServiceValidationAttribute
{
    public PasswordAttribute() : base(ValidationErrorMessages.PasswordAttribute_DefaultErrorMessage) { }
}

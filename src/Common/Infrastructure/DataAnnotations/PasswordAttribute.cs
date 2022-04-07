namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PasswordAttribute : ServiceValidationAttribute
{
    public PasswordAttribute() : base(ValidationErrorMessages.PasswordAttribute_DefaultErrorMessage) { }
}

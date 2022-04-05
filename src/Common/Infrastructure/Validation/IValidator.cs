using System.ComponentModel.DataAnnotations;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Common.Infrastructure.Validation
{
    public interface IValidator<in TAttribute>
        where TAttribute : ServiceValidationAttribute
    {
        string FormatErrorMessage(string localizedName, ITextLocalizer textLocalizer, TAttribute validationAttribute);
        ValidationResult? IsValid(object? value, ValidationContext validationContext, TAttribute validationAttribute);
    }
}

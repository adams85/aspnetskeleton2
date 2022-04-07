using System.ComponentModel.DataAnnotations;

namespace WebApp.Api.Infrastructure.DataAnnotations;

public static class ValidationAttributeExtensions
{
    private static readonly object s_gate = new object();

    public static void AdjustToMvcLocalization(this ValidationAttribute attribute)
    {
        if (attribute is not ExtendedValidationAttribute &&
            attribute.ErrorMessageResourceType == null &&
            string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
            string.IsNullOrEmpty(attribute.ErrorMessage))
        {
            // attribute instances can be cached or static and this method can be called from multiple threads concurrently,
            // so it's safer to lock during assignment (it's better to not make assumptions on the implementation of the setter of ErrorMessage);
            // the preceding check can be done outside the lock as multiple assignments do no harm (the same constant value is assigned anyway)
            lock (s_gate)
                attribute.ErrorMessage = attribute.GetDefaultErrorMessage();
        }
    }
}

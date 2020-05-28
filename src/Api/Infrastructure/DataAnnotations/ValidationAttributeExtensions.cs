using System.ComponentModel.DataAnnotations;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    public static class ValidationAttributeExtensions
    {
        private static readonly object s_gate = new object();

        public static bool AdjustToMvcLocalization(this ValidationAttribute attribute, out ExtendedValidationAttribute? extendedAttribute)
        {
            extendedAttribute = attribute as ExtendedValidationAttribute;

            var isLocalizable = attribute.ErrorMessageResourceType == null && string.IsNullOrEmpty(attribute.ErrorMessageResourceName);

            // default messages of built-in validation attributes aren't localized by default (https://github.com/aspnet/Localization/issues/286)
            // the least cumbersome solution to this problem is setting the ErrorMessage property, which will ensure that the message will run through the localizer
            if (extendedAttribute == null && isLocalizable && string.IsNullOrEmpty(attribute.ErrorMessage))
            {
                // attribute instances can be cached or static and this method can be called from multiple threads concurrently,
                // so it's safer to lock during assignment (it's better to not make assumptions on the implementation of the setter of ErrorMessage);
                // the preceding check can be done outside the lock as multiple assignments do no harm (the same constant value is assigned anyway)
                lock (s_gate)
                    attribute.ErrorMessage = attribute.GetDefaultErrorMessage();
            }

            return isLocalizable;
        }
    }
}

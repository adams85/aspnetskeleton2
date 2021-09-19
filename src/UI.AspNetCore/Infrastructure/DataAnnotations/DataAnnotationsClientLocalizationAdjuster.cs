using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WebApp.Api.Infrastructure.DataAnnotations;

namespace WebApp.UI.Infrastructure.DataAnnotations
{
    public class DataAnnotationsClientLocalizationAdjuster : IClientModelValidatorProvider
    {
        public void CreateValidators(ClientValidatorProviderContext context)
        {
            var results = context.Results;
            for (int i = 0, n = results.Count; i < n; i++)
            {
                var result = results[i];

                // default messages of built-in validation attributes aren't localized by default (https://github.com/aspnet/Localization/issues/286);
                // the least cumbersome solution to this problem is setting the ErrorMessage property, which will ensure that the message will run through the localizer

                if (result.ValidatorMetadata is ValidationAttribute validationAttribute)
                    validationAttribute.AdjustToMvcLocalization();
            }
        }
    }
}

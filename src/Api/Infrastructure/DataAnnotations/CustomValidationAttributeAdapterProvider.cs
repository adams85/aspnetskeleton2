using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations;

public sealed class CustomValidationAttributeAdapterProvider : ValidationAttributeAdapterProvider, IValidationAttributeAdapterProvider
{
    IAttributeAdapter? IValidationAttributeAdapterProvider.GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer) => attribute switch
    {
        ExtendedValidationAttribute extendedAttribute => new ExtendedValidationAttributeAdapter<ExtendedValidationAttribute>(extendedAttribute, stringLocalizer),
        _ => GetAttributeAdapter(attribute, stringLocalizer),
    };
}

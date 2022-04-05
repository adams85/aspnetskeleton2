using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    // based on: https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/Mvc/Mvc.DataAnnotations/src/ValidatableObjectAdapter.cs
    public sealed class CustomValidatableObjectAdapter : IModelValidator
    {
        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider;
        private readonly IStringLocalizer _stringLocalizer;

        public CustomValidatableObjectAdapter(IValidationAttributeAdapterProvider validationAttributeAdapterProvider, IStringLocalizer stringLocalizer)
        {
            _validationAttributeAdapterProvider = validationAttributeAdapterProvider;
            _stringLocalizer = stringLocalizer;
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var model = context.Model;
            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            if (!(model is IValidatableObject validatable))
            {
                throw new InvalidOperationException($"The model object inside the metadata claimed to be compatible with '{typeof(IValidatableObject).Name}', but was actually '{model.GetType()}'.");
            }

            // The constructed ValidationContext is intentionally slightly different from what
            // DataAnnotationsModelValidator creates. The instance parameter would be context.Container
            // (if non-null) in that class. But, DataAnnotationsModelValidator _also_ passes context.Model
            // separately to any ValidationAttribute.
            var validationContext = new ValidationContext(
                instance: validatable,
                serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                items: null)
            {
                DisplayName = context.ModelMetadata.GetDisplayName(),
                MemberName = context.ModelMetadata.Name,
            };

            var result = validatable.Validate(validationContext);

            return ConvertResults(context, validationContext, result);
        }

        private IEnumerable<ModelValidationResult> ConvertResults(ModelValidationContext validationContext, ValidationContext context, IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                    {
                        yield return new ModelValidationResult(memberName: null, message: GetErrorMessage(validationContext, context, result));
                    }
                    else
                    {
                        foreach (var memberName in result.MemberNames)
                        {
                            yield return new ModelValidationResult(memberName, GetErrorMessage(validationContext, context, result, memberName));
                        }
                    }
                }
            }
        }

        private string? GetErrorMessage(ModelValidationContext validationContext, ValidationContext context, ValidationResult result, string? memberName = null)
        {
            if (result is ExtendedValidationResult extendedValidationResult)
            {
                var adapter = _validationAttributeAdapterProvider.GetAttributeAdapter(extendedValidationResult.ValidationAttribute, _stringLocalizer);
                if (adapter != null)
                {
                    ModelMetadata? memberModelMetadata;
                    if (memberName != null && (memberModelMetadata = validationContext.ModelMetadata.Properties[memberName]) != null && memberModelMetadata.PropertyGetter != null)
                    {
                        var memberValue = memberModelMetadata.PropertyGetter(validationContext.Model!);
                        validationContext = new ModelValidationContext(validationContext.ActionContext, memberModelMetadata, validationContext.MetadataProvider, validationContext.Model, memberValue);
                    }

                    return adapter.GetErrorMessage(validationContext);
                }
            }

            return result.ErrorMessage;
        }
    }
}

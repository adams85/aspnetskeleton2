using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WebApp.Api.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    // based on: https://github.com/dotnet/aspnetcore/blob/v3.1.3/src/Mvc/Mvc.DataAnnotations/src/DataAnnotationsModelValidator.cs
    public sealed class CustomDataAnnotationsModelValidator : IModelValidator
    {
        private static readonly object s_emptyValidationContextInstance = new object();

        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider;
        private readonly StringLocalizerAdapter? _stringLocalizerAdapter;

        public CustomDataAnnotationsModelValidator(IValidationAttributeAdapterProvider validationAttributeAdapterProvider, ValidationAttribute attribute,
            StringLocalizerAdapter? stringLocalizerAdapter)
        {
            _validationAttributeAdapterProvider = validationAttributeAdapterProvider ?? throw new ArgumentNullException(nameof(validationAttributeAdapterProvider));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _stringLocalizerAdapter = stringLocalizerAdapter;
        }

        public ValidationAttribute Attribute { get; }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            if (validationContext.ModelMetadata == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(validationContext.ModelMetadata)}' property of '{typeof(ModelValidationContext)}' must not be null.",
                    nameof(validationContext));
            }

            if (validationContext.MetadataProvider == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(validationContext.MetadataProvider)}' property of '{typeof(ModelValidationContext)}' must not be null.",
                    nameof(validationContext));
            }

            var metadata = validationContext.ModelMetadata;
            var memberName = metadata.Name;
            var container = validationContext.Container;

            var context = new ValidationContext(
                instance: container ?? validationContext.Model ?? s_emptyValidationContextInstance,
                serviceProvider: validationContext.ActionContext?.HttpContext?.RequestServices,
                items: null)
            {
                DisplayName = metadata.GetDisplayName(),
                MemberName = memberName
            };

            var result = Attribute.GetValidationResult(validationContext.Model, context);
            if (result != ValidationResult.Success)
            {
                var errorMessage = GetErrorMessage(validationContext, context, result);

                var validationResults = new List<ModelValidationResult>();
                if (result.MemberNames != null)
                {
                    foreach (var resultMemberName in result.MemberNames)
                    {
                        // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to
                        // append construct the ModelKey for ModelStateDictionary. When validating at type level we
                        // want the returned MemberNames if specified (e.g. "person.Address.FirstName"). For property
                        // validation, the ModelKey can be constructed using the ModelMetadata and we should ignore
                        // MemberName (we don't want "person.Name.Name"). However the invoking validator does not have
                        // a way to distinguish between these two cases. Consequently we'll only set MemberName if this
                        // validation returns a MemberName that is different from the property being validated.
                        var newMemberName = string.Equals(resultMemberName, memberName, StringComparison.Ordinal) ?
                            null :
                            resultMemberName;
                        var validationResult = new ModelValidationResult(newMemberName, errorMessage);

                        validationResults.Add(validationResult);
                    }
                }

                if (validationResults.Count == 0)
                {
                    // result.MemberNames was null or empty.
                    validationResults.Add(new ModelValidationResult(memberName: null, message: errorMessage));
                }

                return validationResults;
            }

            return Enumerable.Empty<ModelValidationResult>();
        }

        private string GetErrorMessage(ModelValidationContext validationContext, ValidationContext context, ValidationResult result)
        {
            var isLocalizable = Attribute.AdjustToMvcLocalization(out var extendedAttribute);

            var adapter = _validationAttributeAdapterProvider.GetAttributeAdapter(Attribute, _stringLocalizerAdapter);
            if (adapter != null)
                return adapter.GetErrorMessage(validationContext);

            if (isLocalizable && _stringLocalizerAdapter != null && extendedAttribute != null)
                return extendedAttribute.FormatErrorMessage(validationContext.ModelMetadata.GetDisplayName(), _stringLocalizerAdapter, context);

            return result.ErrorMessage;
        }
    }
}

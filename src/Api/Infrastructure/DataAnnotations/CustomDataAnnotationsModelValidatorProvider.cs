using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    /// <summary>
    /// Customizes data annotation-based MVC model validation.
    /// </summary>
    /// <remarks>
    /// This implementation deals with some well-known quirks of MVC model validation like <see href="(https://github.com/aspnet/Localization/issues/286)"/> and
    /// adds support for our data annotation customizations (<see cref="ExtendedValidationAttribute"/> and <see cref="ExtendedValidationResult"/>).
    /// </remarks>
    // based on: https://github.com/dotnet/aspnetcore/blob/v3.1.3/src/Mvc/Mvc.DataAnnotations/src/DataAnnotationsModelValidatorProvider.cs
    public sealed class CustomDataAnnotationsModelValidatorProvider : IMetadataBasedModelValidatorProvider
    {
        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider;
        private readonly MvcDataAnnotationsLocalizationOptions _options;
        private readonly IStringLocalizerFactory? _stringLocalizerFactory;

        public CustomDataAnnotationsModelValidatorProvider(IValidationAttributeAdapterProvider validationAttributeAdapterProvider, IOptions<MvcDataAnnotationsLocalizationOptions> options,
            IStringLocalizerFactory? stringLocalizerFactory)
        {
            _validationAttributeAdapterProvider = validationAttributeAdapterProvider ?? throw new ArgumentNullException(nameof(validationAttributeAdapterProvider));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            StringLocalizerAdapter? stringLocalizerAdapter = null;
            if (_stringLocalizerFactory != null && _options.DataAnnotationLocalizerProvider != null)
            {
                var stringLocalizer = _options.DataAnnotationLocalizerProvider(
                    context.ModelMetadata.ContainerType ?? context.ModelMetadata.ModelType,
                    _stringLocalizerFactory);

                stringLocalizerAdapter = new StringLocalizerAdapter(stringLocalizer);
            }

            var results = context.Results;
            // Read interface .Count once rather than per iteration
            var resultsCount = results.Count;
            for (var i = 0; i < resultsCount; i++)
            {
                var validatorItem = results[i];
                if (validatorItem.Validator != null)
                {
                    continue;
                }

                if (!(validatorItem.ValidatorMetadata is ValidationAttribute attribute))
                {
                    continue;
                }

                var validator = new CustomDataAnnotationsModelValidator(
                    _validationAttributeAdapterProvider,
                    attribute,
                    stringLocalizerAdapter);

                validatorItem.Validator = validator;
                validatorItem.IsReusable = true;
                // Inserts validators based on whether or not they are 'required'. We want to run
                // 'required' validators first so that we get the best possible error message.
                if (attribute is RequiredAttribute)
                {
                    context.Results.Remove(validatorItem);
                    context.Results.Insert(0, validatorItem);
                }
            }

            // Produce a validator if the type supports IValidatableObject
            if (typeof(IValidatableObject).IsAssignableFrom(context.ModelMetadata.ModelType))
            {
                context.Results.Add(new ValidatorItem
                {
                    Validator = new CustomValidatableObjectAdapter(_validationAttributeAdapterProvider, stringLocalizerAdapter),
                    IsReusable = true
                });
            }
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            if (typeof(IValidatableObject).IsAssignableFrom(modelType))
            {
                return true;
            }

            // Read interface .Count once rather than per iteration
            var validatorMetadataCount = validatorMetadata.Count;
            for (var i = 0; i < validatorMetadataCount; i++)
            {
                if (validatorMetadata[i] is ValidationAttribute)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    /// <summary>
    /// Deals with some well-known quirks of MVC model validation like <see href="(https://github.com/aspnet/Localization/issues/286)"/> and
    /// adds support for our data annotation customizations (<see cref="ExtendedValidationAttribute"/> and <see cref="ExtendedValidationResult"/>).
    /// </summary>
    /// <remarks>
    /// An instance of this class must be added to the <see cref="MvcOptions.ModelValidatorProviders"/> list and it must be placed after
    /// the built-in <see cref="DataAnnotationsModelValidatorProvider"/>.
    /// </remarks>
    public sealed class DataAnnotationLocalizationAdjuster : IMetadataBasedModelValidatorProvider
    {
        private static readonly object s_gate = new object();

        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider;
        private readonly Func<Type, IStringLocalizerFactory, IStringLocalizer>? _dataAnnotationLocalizerProvider;
        private readonly IStringLocalizerFactory? _stringLocalizerFactory;

        public DataAnnotationLocalizationAdjuster(IValidationAttributeAdapterProvider validationAttributeAdapterProvider, IOptions<MvcDataAnnotationsLocalizationOptions> options,
            IStringLocalizerFactory? stringLocalizerFactory)
        {
            if (options?.Value == null)
                throw new ArgumentNullException(nameof(options));

            _validationAttributeAdapterProvider = validationAttributeAdapterProvider ?? throw new ArgumentNullException(nameof(validationAttributeAdapterProvider));
            _dataAnnotationLocalizerProvider = options.Value.DataAnnotationLocalizerProvider;
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            var stringLocalizerAdapter =
                _stringLocalizerFactory != null && _dataAnnotationLocalizerProvider != null && context.ModelMetadata.ModelType.HasInterface(typeof(IValidatableObject)) ?
                new StringLocalizerAdapter(_dataAnnotationLocalizerProvider(context.ModelMetadata.ContainerType ?? context.ModelMetadata.ModelType, _stringLocalizerFactory)) :
                null;

            var validatableObjectAdapterFound = false;

            var results = context.Results;
            for (int i = 0, n = results.Count; i < n; i++)
            {
                var result = results[i];

                // 1. default messages of built-in validation attributes aren't localized by default (https://github.com/aspnet/Localization/issues/286);
                // the least cumbersome solution to this problem is setting the ErrorMessage property, which will ensure that the message will run through the localizer

                if (result.ValidatorMetadata is ValidationAttribute validationAttribute && !(validationAttribute is ExtendedValidationAttribute))
                {
                    if (validationAttribute.ErrorMessageResourceType == null &&
                        string.IsNullOrEmpty(validationAttribute.ErrorMessageResourceName) &&
                        string.IsNullOrEmpty(validationAttribute.ErrorMessage))
                    {
                        // attribute instances can be cached or static and this method can be called from multiple threads concurrently,
                        // so it's safer to lock during assignment (it's better to not make assumptions on the implementation of the setter of ErrorMessage);
                        // the preceding check can be done outside the lock as multiple assignments do no harm (the same constant value is assigned anyway)
                        lock (s_gate)
                            validationAttribute.ErrorMessage = validationAttribute.GetDefaultErrorMessage();
                    }
                }

                // 2. support for ExtendedValidationResult localization in the case of IValidatableObject

                if (stringLocalizerAdapter != null)
                {
                    // we need to check type by name because unfortunately ValidatableObjectAdapter is internal
                    if (result.Validator != null && result.Validator.GetType().FullName == "Microsoft.AspNetCore.Mvc.DataAnnotations.ValidatableObjectAdapter")
                    {
                        results[i] = new ValidatorItem(result.ValidatorMetadata)
                        {
                            Validator = new CustomValidatableObjectAdapter(_validationAttributeAdapterProvider, stringLocalizerAdapter),
                            IsReusable = result.IsReusable,
                        };

                        validatableObjectAdapterFound = true;
                    }
                }
            }

            if (stringLocalizerAdapter != null)
                Debug.Assert(validatableObjectAdapterFound, "Microsoft.AspNetCore.Mvc.DataAnnotations internals have apparently changed.");
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata) => false;
    }
}

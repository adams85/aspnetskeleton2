using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using WebApp.Api.Infrastructure.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.DataAnnotations
{
    public class ExtendedValidationAttributeAdapter<TAttribute> : AttributeAdapterBase<TAttribute>
        where TAttribute : ExtendedValidationAttribute
    {
        private readonly ITextLocalizer _textLocalizer;

        public ExtendedValidationAttributeAdapter(TAttribute attribute, IStringLocalizer? stringLocalizer)
            : base(attribute, stringLocalizer)
        {
            _textLocalizer =
                stringLocalizer != null ?
                stringLocalizer as ITextLocalizer ?? new TextLocalizerAdapter(stringLocalizer) :
                NullTextLocalizer.Instance;
        }

        public override void AddValidation(ClientModelValidationContext context) { }

        public sealed override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));

            return Attribute.FormatErrorMessage(
                validationContext.ModelMetadata.GetDisplayName(),
                _textLocalizer,
                validationContext.ActionContext?.HttpContext?.RequestServices);
        }
    }
}


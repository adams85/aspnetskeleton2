using System.Collections.Concurrent;
using System.Reflection;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Common.Infrastructure.Validation;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Base class for custom validation attributes which delegates the task of validation to an <see cref="IValidator{TAttribute}"/> service.
    /// This is useful when the parameters of the validation logic are not known at compile time (e.g. they come from configuration).
    /// </summary>
    public abstract class ServiceValidationAttribute : ExtendedValidationAttribute
    {
        private static readonly ConcurrentDictionary<Type, ValidatorHelper> s_validatorHelperCache = new ConcurrentDictionary<Type, ValidatorHelper>();

        private readonly ValidatorHelper _validatorHelper;

        protected ServiceValidationAttribute(string errorMessage) : base(errorMessage)
        {
            _validatorHelper = s_validatorHelperCache.GetOrAdd(GetType(), type => new ValidatorHelper(type));
        }

        public bool IgnoreIfServiceUnavailable { get; set; }

        public override bool RequiresValidationContext => true;

        private object? GetValidator(IServiceProvider serviceProvider)
        {
            var validator = serviceProvider.GetService(_validatorHelper.ServiceType);

            if (validator == null && !IgnoreIfServiceUnavailable)
                throw new InvalidOperationException($"Validator service {_validatorHelper.ServiceType} has not been registered.");

            return validator;
        }

        public override string FormatErrorMessage(string localizedName, ITextLocalizer textLocalizer, IServiceProvider? serviceProvider = null)
        {
            var validator = serviceProvider != null ? GetValidator(serviceProvider) : null;

            if (validator == null)
                return base.FormatErrorMessage(localizedName, textLocalizer, serviceProvider);

            return _validatorHelper.FormatErrorMessage(validator, localizedName, textLocalizer, this);
        }

        protected sealed override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var validator = GetValidator(validationContext);

            if (validator == null)
                return ValidationResult.Success;

            return _validatorHelper.IsValid(validator, value, validationContext, this);
        }

        private delegate string FormatErrorMessageDelegate(object validator, string localizedName, ITextLocalizer textLocalizer, ServiceValidationAttribute validationAttribute);

        private delegate ValidationResult IsValidDelegate(object validator, object? value, ValidationContext validationContext, ValidationAttribute validationAttribute);

        private sealed class ValidatorHelper
        {
            private static readonly MethodInfo s_invokeFormatErrorMessageMethodDefinition =
                new FormatErrorMessageDelegate(InvokeFormatErrorMessage<ServiceValidationAttribute>).Method.GetGenericMethodDefinition();

            private static readonly MethodInfo s_invokeIsValidMethodDefinition =
                new IsValidDelegate(InvokeIsValid<ServiceValidationAttribute>).Method.GetGenericMethodDefinition();

            private static string InvokeFormatErrorMessage<TAttribute>(object validator, string localizedName, ITextLocalizer textLocalizer, ServiceValidationAttribute validationAttribute)
                where TAttribute : ServiceValidationAttribute =>
                ((IValidator<TAttribute>)validator).FormatErrorMessage(localizedName, textLocalizer, (TAttribute)validationAttribute);

            private static ValidationResult InvokeIsValid<TAttribute>(object validator, object? value, ValidationContext validationContext, ValidationAttribute validationAttribute)
                where TAttribute : ServiceValidationAttribute =>
                ((IValidator<TAttribute>)validator).IsValid(value, validationContext, (TAttribute)validationAttribute);

            public ValidatorHelper(Type attributeType)
            {
                ServiceType = typeof(IValidator<>).MakeGenericType(attributeType);

                FormatErrorMessage = (FormatErrorMessageDelegate)Delegate.CreateDelegate(typeof(FormatErrorMessageDelegate),
                    s_invokeFormatErrorMessageMethodDefinition.MakeGenericMethod(attributeType));

                IsValid = (IsValidDelegate)Delegate.CreateDelegate(typeof(IsValidDelegate),
                    s_invokeIsValidMethodDefinition.MakeGenericMethod(attributeType));
            }

            public Type ServiceType { get; }
            public FormatErrorMessageDelegate FormatErrorMessage { get; }
            public IsValidDelegate IsValid { get; }
        }
    }
}

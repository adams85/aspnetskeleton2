using WebApp.Common.Infrastructure.Localization;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Extended version of <see cref="ValidationAttribute"/> which enables formatting error messages using an arbitrary localization method.
    /// </summary>
    /// <remarks>
    /// Custom validation attributes should inherit from this class and should create <see cref="ExtendedValidationResult"/> instances on validation errors.
    /// </remarks>
    public abstract class ExtendedValidationAttribute : ValidationAttribute
    {
        protected ExtendedValidationAttribute(string errorMessage) : base(errorMessage) { }

        public virtual string GetErrorMessageFormat(string localizedName, out object[] args)
        {
            args = new[] { localizedName };
            return ErrorMessageString;
        }

        public virtual string FormatErrorMessage(string localizedName, ITextLocalizer textLocalizer, IServiceProvider? serviceProvider = null)
        {
            var errorMessageFormat = GetErrorMessageFormat(localizedName, out var errorMessageArgs);

            // when ErrorMessageResourceType and ErrorMessageResourceName is set, we assume that the error message is already localized
            // (we only need to check either of these properties because of the preliminary check which ErrorMessageString does under the hood)
            if (ErrorMessageResourceType != null)
                return NullTextLocalizer.Instance[errorMessageFormat, errorMessageArgs];

            return textLocalizer[errorMessageFormat, errorMessageArgs];
        }

        public string FormatErrorMessage(ValidationContext validationContext)
        {
            var textLocalizer = (ITextLocalizer?)validationContext.GetService(typeof(ITextLocalizer)) ?? NullTextLocalizer.Instance;

            return FormatErrorMessage(validationContext.DisplayName, textLocalizer, validationContext);
        }

        public sealed override string FormatErrorMessage(string name) =>
            FormatErrorMessage(name, NullTextLocalizer.Instance);
    }
}

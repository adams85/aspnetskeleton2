using System.Collections.Generic;
using System.Threading;

namespace System.ComponentModel.DataAnnotations
{
    public class ExtendedValidationResult : ValidationResult
    {
        private ExtendedValidationResult(ValidationAttribute validationAttribute, string errorMessage, IEnumerable<string>? memberNames = null)
            : base(errorMessage, memberNames)
        {
            ValidationAttribute = validationAttribute;
        }

        public ExtendedValidationResult(ExtendedValidationAttribute validationAttribute, ValidationContext validationContext, IEnumerable<string>? memberNames = null)
            : this(
                  validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute)),
                  validationAttribute.FormatErrorMessage(validationContext),
                  memberNames)
        { }

        public ExtendedValidationResult(ValidationAttribute validationAttribute, ValidationContext validationContext, IEnumerable<string>? memberNames = null)
            : this(
                  validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute)),
                  validationAttribute.FormatErrorMessage(validationContext.DisplayName),
                  memberNames)
        { }

        public ExtendedValidationResult(ValidationAttribute validationAttribute, ValidationResult validationResult)
            : this(
                  validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute)),
                  validationResult.ErrorMessage,
                  validationResult.MemberNames)
        { }

        public ValidationAttribute ValidationAttribute { get; }

        private IDictionary<object, object>? _properties;
        public IDictionary<object, object> Properties => LazyInitializer.EnsureInitialized(ref _properties, () => new Dictionary<object, object>())!;
    }
}

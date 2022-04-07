using System.Collections;

namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ItemsRequiredAttribute : ExtendedValidationAttribute
{
    public ItemsRequiredAttribute() : base(ValidationErrorMessages.ItemsRequiredAttribute_DefaultErrorMessage) { }

    public bool AllowEmptyStrings { get; init; }

    public override bool RequiresValidationContext => false;

    public override bool IsValid(object? value)
    {
        if (value is IList list)
        {
            for (int i = 0, n = list.Count; i < n; i++)
                if (!IsValidItem(list[i]))
                    return false;
        }
        // TODO: IDictionary<,>/IReadOnlyDictionary<,> support?
        else if (value is IDictionary dictionary)
        {
            var enumerator = dictionary.GetEnumerator();
            using (enumerator as IDisposable)
                while (enumerator.MoveNext())
                    if (!IsValidItem(enumerator.Value))
                        return false;
        }
        else if (value is IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            using (enumerator as IDisposable)
                while (enumerator.MoveNext())
                    if (!IsValidItem(enumerator.Current))
                        return false;
        }

        return true;

        bool IsValidItem(object item)
        {
            if (item == null)
                return false;

            if (!AllowEmptyStrings && item is string text && text.Trim().Length == 0)
                return false;

            return true;
        }
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (IsValid(value))
            return ValidationResult.Success;

        var memberNames = validationContext.MemberName != null ? new string[] { validationContext.MemberName } : null;
        return new ExtendedValidationResult(this, validationContext, memberNames);
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Roles;
using WebApp.Service.Settings;
using WebApp.Service.Users;

namespace WebApp.Service
{
    [DataContract]
    [ProtoInclude(11, typeof(ListSettingsQuery))]
    [ProtoInclude(12, typeof(ListRolesQuery))]
    [ProtoInclude(13, typeof(ListUsersQuery))]
    public class ListQuery : IQuery, IValidatableObject
    {
        [DataMember(Order = 1)] public string[]? OrderColumns { get; set; }
        public bool IsOrdered => OrderColumns != null && OrderColumns.Length > 0;

        [DataMember(Order = 2)] public int PageIndex { get; set; }
        [DataMember(Order = 3)] public int PageSize { get; set; }
        [DataMember(Order = 4)] public int MaxPageSize { get; set; }
        public bool IsPaged => PageSize > 0;

        [DataMember(Order = 5)] public bool SkipTotalItemCount { get; set; }

        [Translatable] private const string NonNegativeIntegerValidatorErrorMessage = "The field {0} must be a non-negative integer.";

        [Translatable] private const string ItemsRequiredValidatorErrorMessage = "The field {0} must contain non-empty strings.";

        private static readonly ValidationAttribute s_nonNegativeIntegerValidator = new RangeAttribute(0, int.MaxValue)
        {
            ErrorMessage = NonNegativeIntegerValidatorErrorMessage
        };

        private static readonly ValidationAttribute s_itemsRequiredValidator = new ItemsRequiredAttribute()
        {
            ErrorMessage = ItemsRequiredValidatorErrorMessage
        };

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (IsPaged)
                yield return this.ValidateMember(PageIndex, nameof(PageIndex), validationContext, s_nonNegativeIntegerValidator);

            if (IsOrdered)
                yield return this.ValidateMember(OrderColumns, nameof(OrderColumns), validationContext, s_itemsRequiredValidator);

            yield return ValidationResult.Success;
        }
    }
}

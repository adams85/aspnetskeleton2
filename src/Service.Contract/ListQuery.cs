using System;
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
    public abstract class ListQuery<TResult> : IListQuery, IQuery<TResult>, IValidatableObject
        where TResult : IListResult
    {
        [DataMember(Order = 1)] public string[]? OrderBy { get; set; }
        public bool IsOrdered => OrderBy != null && OrderBy.Length > 0;

        [DataMember(Order = 2)] public int PageIndex { get; set; }
        [DataMember(Order = 3)] public int PageSize { get; set; }
        [DataMember(Order = 4)] public int MaxPageSize { get; set; }
        public bool IsPaged => PageSize > 0;

        [DataMember(Order = 5)] public bool SkipTotalItemCount { get; set; }

        [Localized] private const string NonNegativeIntegerValidatorErrorMessage = "The field {0} must be a non-negative integer.";

        [Localized] private const string ItemsRequiredValidatorErrorMessage = "The field {0} must contain non-empty strings.";

        private static readonly ValidationAttribute s_nonNegativeIntegerValidator = new RangeAttribute(0, int.MaxValue)
        {
            ErrorMessage = NonNegativeIntegerValidatorErrorMessage
        };

        private static readonly ValidationAttribute s_itemsRequiredValidator = new ItemsRequiredAttribute()
        {
            ErrorMessage = ItemsRequiredValidatorErrorMessage
        };

        protected virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsPaged)
                yield return this.ValidateMember(PageIndex, nameof(PageIndex), validationContext, s_nonNegativeIntegerValidator);

            if (IsOrdered)
                yield return this.ValidateMember(OrderBy, nameof(OrderBy), validationContext, s_itemsRequiredValidator);
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext) => Validate(validationContext);

        public void EnsurePaging(int defaultPageSize, int maxPageSize)
        {
            if (defaultPageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultPageSize));

            if (maxPageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPageSize));

            if (!IsPaged)
            {
                PageIndex = 0;
                PageSize = defaultPageSize;
            }

            MaxPageSize = maxPageSize;
        }
    }
}

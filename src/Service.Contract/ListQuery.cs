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
    public abstract record class ListQuery<TResult> : IListQuery, IQuery<TResult>
        where TResult : IListResult
    {
        [DataMember(Order = 1)] public string[]? OrderBy { get; init; }

        public bool IsOrdered => OrderBy != null && OrderBy.Length > 0;

        private int _pageIndex;
        [DataMember(Order = 2)]
        public int PageIndex
        {
            get => _pageIndex;
            init => _pageIndex = value;
        }

        private int _pageSize;
        [DataMember(Order = 3)]
        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value;
        }

        private int _maxPageSize;
        [DataMember(Order = 4)]
        public int MaxPageSize
        {
            get => _maxPageSize;
            init => _maxPageSize = value;
        }

        public bool IsPaged => PageSize > 0;

        [DataMember(Order = 5)] public bool SkipTotalItemCount { get; init; }

        public virtual int DefaultPageSize => 0;

        public void EnsurePaging(int maxPageSize)
        {
            if (maxPageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPageSize));

            if (!IsPaged)
            {
                _pageIndex = 0;
                _pageSize = DefaultPageSize > 0 ? DefaultPageSize : maxPageSize;
            }

            _maxPageSize = maxPageSize;
        }

        [Localized] private const string NonNegativeIntegerValidatorErrorMessage = "The field {0} must be a non-negative integer.";
        private static readonly ValidationAttribute s_nonNegativeIntegerValidator = new RangeAttribute(0, int.MaxValue)
        {
            ErrorMessage = NonNegativeIntegerValidatorErrorMessage
        };

        [Localized] private const string ItemsRequiredValidatorErrorMessage = "The field {0} must contain non-empty strings.";
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
    }
}

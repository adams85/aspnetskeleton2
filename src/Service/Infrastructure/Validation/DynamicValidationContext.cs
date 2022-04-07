using System;
using System.Linq.Expressions;
using Karambolo.Common;

namespace WebApp.Service.Infrastructure.Validation
{
    internal abstract class DynamicValidationContext<TModel>
    {
        private readonly Func<string>? _getMemberPathBase;

        protected DynamicValidationContext(Func<string>? getMemberPathBase = null)
        {
            _getMemberPathBase = getMemberPathBase;
        }

        public ServiceErrorCode? ErrorCode { get; private set; }
        public Func<object[]>? ErrorArgsFactory { get; private set; }

        public string GetMemberPath<TProperty>(Expression<Func<TModel, TProperty>> memberPathExpression)
        {
            var memberPath = Lambda.MemberPath(memberPathExpression);
            var memberPathBase = _getMemberPathBase?.Invoke() ?? string.Empty;
            return DataAnnotationsValidator.GetPropertyMemberPath(memberPathBase, memberPath);
        }

        public void SetError(ServiceErrorCode errorCode, Func<object[]>? errorArgsFactory = null)
        {
            ErrorCode = errorCode;
            ErrorArgsFactory = errorArgsFactory;
        }

        public void SetError<TProperty>(ServiceErrorCode errorCode, Expression<Func<TModel, TProperty>> memberPathExpression)
        {
            ErrorCode = errorCode;
            ErrorArgsFactory = () => new[] { GetMemberPath(memberPathExpression) };
        }
    }
}

using System;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;

namespace WebApp.Service
{
    internal abstract class QueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        protected QueryHandler() { }

        public abstract Task<TResult> HandleAsync(TQuery query, QueryContext context, CancellationToken cancellationToken);

        #region Helpers for manual validation

        protected static void Require(bool condition, ServiceErrorCode errorCode, Func<object[]>? argsFactory = null)
        {
            if (!condition)
                throw new ServiceErrorException(errorCode, argsFactory?.Invoke());
        }

        protected static void Require<T>(bool condition, ServiceErrorCode errorCode, Expression<Func<TQuery, T>> memberPathExpression)
        {
            Require(condition, errorCode, () => new[] { memberPathExpression.MemberPath() });
        }

        protected static void RequireSpecified<T>(T @param, Expression<Func<TQuery, T>> memberPathExpression, bool emptyAllowed = false)
        {
            Require(
                @param != null &&
                (emptyAllowed || !(@param is string paramString) || paramString.Length > 0) &&
                (emptyAllowed || !(@param is ICollection paramCollection) || paramCollection.Count > 0),
                ServiceErrorCode.ParamNotSpecified, memberPathExpression);
        }

        protected static void RequireValid<T>(bool condition, Expression<Func<TQuery, T>> memberPathExpression)
        {
            Require(condition, ServiceErrorCode.ParamNotValid, memberPathExpression);
        }

        protected static void RequireExisting<T>(object? entity, Expression<Func<TQuery, T>> memberPathExpression)
        {
            RequireExisting(entity != null, memberPathExpression);
        }

        protected static void RequireExisting<T>(bool entityExists, Expression<Func<TQuery, T>> memberPathExpression)
        {
            Require(entityExists, ServiceErrorCode.EntityNotFound, memberPathExpression);
        }

        #endregion
    }
}

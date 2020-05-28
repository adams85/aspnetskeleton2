using System;
using System.Collections;
using System.Linq.Expressions;
using Karambolo.Common;

namespace WebApp.Service
{
    internal abstract class CommandHandler<TCommand>
        where TCommand : ICommand
    {
        protected CommandHandler() { }

        #region Helpers for manual validation

        protected static void Require(bool condition, ServiceErrorCode errorCode, Func<object[]>? argsFactory = null)
        {
            if (!condition)
                throw new ServiceErrorException(errorCode, argsFactory?.Invoke());
        }

        protected static void Require<T>(bool condition, ServiceErrorCode errorCode, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(condition, errorCode, () => new[] { memberPathExpression.MemberPath() });
        }

        protected static void RequireSpecified<T>(T @param, Expression<Func<TCommand, T>> memberPathExpression, bool emptyAllowed = false)
        {
            Require(
                @param != null &&
                (emptyAllowed || !(@param is string paramString) || paramString.Length > 0) &&
                (emptyAllowed || !(@param is ICollection paramCollection) || paramCollection.Count > 0),
                ServiceErrorCode.ParamNotSpecified, memberPathExpression);
        }

        protected static void RequireValid<T>(bool condition, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(condition, ServiceErrorCode.ParamNotValid, memberPathExpression);
        }

        protected static void RequireExisting<T>(object? entity, Expression<Func<TCommand, T>> memberPathExpression)
        {
            RequireExisting(entity != null, memberPathExpression);
        }

        protected static void RequireExisting<T>(bool entityExists, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(entityExists, ServiceErrorCode.EntityNotFound, memberPathExpression);
        }

        protected static void RequireUnique<T>(bool entityExists, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(!entityExists, ServiceErrorCode.EntityNotUnique, memberPathExpression);
        }

        protected static void RequireIndependent<T>(bool entityHasDependencies, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(!entityHasDependencies, ServiceErrorCode.EntityDependent, memberPathExpression);
        }

        #endregion
    }
}

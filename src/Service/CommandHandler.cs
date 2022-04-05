using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;

namespace WebApp.Service
{
    internal abstract class CommandHandler<TCommand>
        where TCommand : ICommand
    {
        protected CommandHandler() { }

        public abstract Task HandleAsync(TCommand command, CommandContext context, CancellationToken cancellationToken);

        #region Helpers for manual validation

        protected static void Require([DoesNotReturnIf(false)] bool condition, ServiceErrorCode errorCode, Func<object[]>? argsFactory = null)
        {
            if (!condition)
                throw new ServiceErrorException(errorCode, argsFactory?.Invoke());
        }

        protected static void Require<T>([DoesNotReturnIf(false)] bool condition, ServiceErrorCode errorCode, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(condition, errorCode, () => new[] { memberPathExpression.MemberPath() });
        }

        protected static void RequireSpecified<T>([NotNull] T @param, Expression<Func<TCommand, T>> memberPathExpression, bool emptyAllowed = false)
        {
            Require(
                @param != null &&
                (emptyAllowed ||
                    (!(@param is string paramString) || paramString.Length > 0) &&
                    (!(@param is ICollection paramCollection) || paramCollection.Count > 0)),
                ServiceErrorCode.ParamNotSpecified, memberPathExpression);

            Debug.Assert(@param != null);
        }

        protected static void RequireValid<T>([DoesNotReturnIf(false)] bool condition, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(condition, ServiceErrorCode.ParamNotValid, memberPathExpression);
        }

        protected static void RequireExisting<T>([NotNull] object? entity, Expression<Func<TCommand, T>> memberPathExpression)
        {
            RequireExisting(entity != null, memberPathExpression);

            Debug.Assert(entity != null);
        }

        protected static void RequireExisting<T>([DoesNotReturnIf(false)] bool entityExists, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(entityExists, ServiceErrorCode.EntityNotFound, memberPathExpression);
        }

        protected static void RequireUnique<T>([DoesNotReturnIf(true)] bool entityExists, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(!entityExists, ServiceErrorCode.EntityNotUnique, memberPathExpression);
        }

        protected static void RequireIndependent<T>([DoesNotReturnIf(true)] bool entityHasDependencies, Expression<Func<TCommand, T>> memberPathExpression)
        {
            Require(!entityHasDependencies, ServiceErrorCode.EntityDependent, memberPathExpression);
        }

        #endregion
    }
}

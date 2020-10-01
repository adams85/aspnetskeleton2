using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Karambolo.Common;

namespace WebApp.Core.Helpers
{
    public static class ExpressionHelper
    {
        public static Expression<TDelegate> BuildWrapperLambda<TDelegate>(this MethodInfo method,
            Func<ParameterExpression[], Expression?> getTargetInstance,
            Func<ParameterExpression[], ParameterInfo[], IEnumerable<Expression>> getMethodCallArguments,
            Func<MethodCallExpression, Type, Expression>? convertReturnValue = null)
            where TDelegate : Delegate
        {
            var delegateInvokeMethod = typeof(TDelegate).GetMethod(nameof(Action.Invoke));

            var delegateParams = delegateInvokeMethod.GetParameters()
                .Select(param => Expression.Parameter(param.ParameterType))
                .ToArray();

            var targetInstance = getTargetInstance(delegateParams);
            var methodCallArguments = getMethodCallArguments(delegateParams, method.GetParameters());
            var methodCall = Expression.Call(targetInstance, method, methodCallArguments);

            var body =
                convertReturnValue != null ? convertReturnValue(methodCall, delegateInvokeMethod.ReturnType) :
                method.ReturnType != delegateInvokeMethod.ReturnType ? Expression.Convert(methodCall, delegateInvokeMethod.ReturnType) :
                (Expression)methodCall;

            return Expression.Lambda<TDelegate>(body, delegateParams);
        }

        public static MemberExpression MakeBoxedAccess<T>(T value) =>
            Expression.Field(Expression.Constant(new StrongBox<T> { Value = value }), BoxHelper<T>.ValueField);

        private static class BoxHelper<T>
        {
            public static readonly FieldInfo ValueField = Lambda.Field((StrongBox<T> box) => box.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Core.Helpers
{
    public static class DependencyInjectionHelper
    {
        private static readonly MethodInfo s_getRequiredServiceMethod =
            new Func<IServiceProvider, object>(ServiceProviderServiceExtensions.GetRequiredService<object>).Method.GetGenericMethodDefinition();

        public static TDelegate BuildMethodInjectionDelegate<TDelegate>(this MethodInfo method,
            Func<ParameterExpression[], Expression?> getTargetInstance,
            Func<ParameterExpression[], ParameterInfo, int, Expression?> getStaticArguments,
            Func<ParameterExpression[], Expression> getServiceProvider,
            Func<MethodCallExpression, Type, Expression>? convertReturnValue = null)
            where TDelegate : Delegate
        {
            return method.BuildWrapperLambda<TDelegate>(getTargetInstance, GetMethodParams, convertReturnValue).Compile();

            IEnumerable<Expression> GetMethodParams(ParameterExpression[] delegateParams, ParameterInfo[] methodParams)
            {
                Expression? serviceProvider = null;

                for (int i = 0, n = methodParams.Length; i < n; i++)
                {
                    var methodParam = methodParams[i];

                    yield return
                        getStaticArguments(delegateParams, methodParam, i) ??
                        BuildServiceResolutionExpression(serviceProvider ??= getServiceProvider(delegateParams), methodParam.ParameterType);
                }
            }

            static Expression BuildServiceResolutionExpression(Expression serviceProviderExpression, Type serviceType) =>
                Expression.Call(s_getRequiredServiceMethod.MakeGenericMethod(serviceType), serviceProviderExpression);
        }
    }
}

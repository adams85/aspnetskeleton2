using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.UI.Infrastructure.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AnonymousOnlyAttribute : Attribute, IAuthorizeData
    {
        private const string PolicyName = "AnonymousOnly";

        public static void Configure(AuthorizationOptions options) => options
            .AddPolicy(PolicyName, builder => builder
                .RequireAssertion(context =>
                {
                    if (context.User?.Identity?.IsAuthenticated ?? false)
                    {
                        context.Fail();
                        return false;
                    }

                    return true;
                }));

        string? IAuthorizeData.AuthenticationSchemes { get => null; [DoesNotReturn] set => throw new NotSupportedException(); }
        string? IAuthorizeData.Policy { get => PolicyName; [DoesNotReturn] set => throw new NotSupportedException(); }
        string? IAuthorizeData.Roles { get => null; [DoesNotReturn] set => throw new NotSupportedException(); }
    }
}

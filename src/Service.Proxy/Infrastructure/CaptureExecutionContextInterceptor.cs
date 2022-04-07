using System;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

using static WebApp.Service.Host.ServiceHostConstants;

namespace WebApp.Service.Infrastructure;

internal sealed class CaptureExecutionContextInterceptor : Interceptor
{
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public CaptureExecutionContextInterceptor(IExecutionContextAccessor executionContextAccessor)
    {
        _executionContextAccessor = executionContextAccessor ?? throw new ArgumentNullException(nameof(executionContextAccessor));
    }

    private Metadata EnsureHeaders<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        var headers = context.Options.Headers;

        if (headers == null)
        {
            headers = new Metadata();
            var options = context.Options.WithHeaders(headers);
            context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
        }

        return headers;
    }

    private void SaveExecutionContextTo(Metadata headers)
    {
        var executionContext = _executionContextAccessor.ExecutionContext;

        var identity = executionContext.User?.Identity;
        if (identity != null && identity.IsAuthenticated)
        {
            headers.Add(IdentityAuthenticationTypeHeaderName, identity.AuthenticationType!);
            headers.Add(IdentityNameHeaderName, Encoding.UTF8.GetBytes(identity.Name!));
        }

        headers.Add(CultureNameHeaderName, executionContext.Culture.Name);
        headers.Add(UICultureNameHeaderName, executionContext.UICulture.Name);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        SaveExecutionContextTo(EnsureHeaders(ref context));
        return base.AsyncClientStreamingCall(context, continuation);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        SaveExecutionContextTo(EnsureHeaders(ref context));
        return base.AsyncDuplexStreamingCall(context, continuation);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        SaveExecutionContextTo(EnsureHeaders(ref context));
        return base.AsyncServerStreamingCall(request, context, continuation);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        SaveExecutionContextTo(EnsureHeaders(ref context));
        return base.AsyncUnaryCall(request, context, continuation);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        SaveExecutionContextTo(EnsureHeaders(ref context));
        return base.BlockingUnaryCall(request, context, continuation);
    }
}

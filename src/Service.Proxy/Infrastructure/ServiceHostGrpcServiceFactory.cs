using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Configuration;
using WebApp.Service.Host;

namespace WebApp.Service.Infrastructure;

internal sealed class ServiceHostGrpcServiceFactory : IServiceHostGrpcServiceFactory, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly CallInvoker _callInvoker;
    private readonly ClientFactory _clientFactory;

    public ServiceHostGrpcServiceFactory(IExecutionContextAccessor executionContextAccessor, IOptions<ServiceProxyApplicationOptions> options, ILoggerFactory? loggerFactory)
    {
        if (executionContextAccessor == null)
            throw new ArgumentNullException(nameof(executionContextAccessor));

        if (options?.Value == null)
            throw new ArgumentNullException(nameof(options));

        var baseUrl =
            options.Value.ServiceBaseUrl ??
            throw new ArgumentException($"{nameof(ServiceProxyApplicationOptions.ServiceBaseUrl)} must be specified.", nameof(options));

        var channelOptions = new GrpcChannelOptions
        {
            LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance,
            // https://github.com/grpc/grpc-dotnet/issues/407#issuecomment-529705056
            ThrowOperationCanceledOnCancellation = true
        };

        _channel = GrpcChannel.ForAddress(baseUrl, channelOptions);
        _callInvoker = _channel.Intercept(new CaptureExecutionContextInterceptor(executionContextAccessor));
        _clientFactory = ClientFactory.Create(ServiceHostContractSerializer.CreateBinderConfiguration());
    }

    public void Dispose() => _channel.Dispose();

    public TService CreateGrpcService<TService>() where TService : class =>
        _clientFactory.CreateClient<TService>(_callInvoker);
}

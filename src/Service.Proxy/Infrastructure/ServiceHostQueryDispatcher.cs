using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ProtoBuf.Grpc;
using WebApp.Core.Helpers;
using WebApp.Service.Host;
using WebApp.Service.Host.Models;
using WebApp.Service.Host.Services;

namespace WebApp.Service.Infrastructure;

internal sealed class ServiceHostQueryDispatcher : IQueryDispatcher
{
    private readonly IQueryService _queryService;

    public ServiceHostQueryDispatcher(IServiceHostGrpcServiceFactory serviceFactory)
    {
        _queryService = serviceFactory.CreateGrpcService<IQueryService>();
    }

    public async Task<object?> DispatchAsync(IQuery query, CancellationToken cancellationToken)
    {
        var queryType = query.GetType();

        var queryRequest = new QueryRequest
        {
            QueryTypeName = queryType.FullNameWithoutAssemblyDetails(),
            SerializedQuery = ServiceHostContractSerializer.Default.Serialize(query, queryType)
        };

        var callContext = new CallContext(new CallOptions(cancellationToken: cancellationToken));

        QueryResponse? response = null;

        if (query is IEventProducerQuery eventProducerQuery && eventProducerQuery.OnEvent != null)
        {
            await foreach (var currentResponse in _queryService.InvokeWithEventNotification(queryRequest, callContext).ConfigureAwait(false))
            {
                if (currentResponse is QueryResponse.Notification notificationResponse)
                    eventProducerQuery.OnEvent.Invoke(query, notificationResponse.Event.Value);

                response = currentResponse;
            }
        }
        else
            response = await _queryService.Invoke(queryRequest, callContext).ConfigureAwait(false);

        switch (response)
        {
            case QueryResponse.Success successResponse:
                if (successResponse.IsResultNull)
                    return null;

                return ServiceHostContractSerializer.Default.Deserialize(successResponse.SerializedResult ?? Array.Empty<byte>(), QueryContext.GetResultType(queryType));
            case QueryResponse.Failure failureResponse:
                throw ServiceErrorException.From(failureResponse.Error);
            case null:
                throw new InvalidOperationException("No query response was received.");
            default:
                throw new InvalidOperationException($"A query response of unexpected type {response.GetType()} was received.");
        }
    }

    public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken) =>
        (TResult)(await DispatchAsync((IQuery)query, cancellationToken).ConfigureAwait(false))!;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Karambolo.Common;
using ProtoBuf.Grpc;
using WebApp.Service.Host;
using WebApp.Service.Host.Models;
using WebApp.Service.Host.Services;
using WebApp.Service.Infrastructure;

namespace WebApp.Service.Host.Services
{
    public sealed class QueryService : IQueryService
    {
        private static readonly string s_serviceContractAssemblyName = typeof(IQuery).Assembly.GetName().Name!;

        private readonly IQueryDispatcher _queryDispatcher;

        public QueryService(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        }

        private async IAsyncEnumerable<QueryResponse> InvokeCore(QueryRequest request, bool notifyEvents, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request.QueryTypeName == null)
                throw new ArgumentException("Query type is not specified.", nameof(request));

            var queryType = Type.GetType(request.QueryTypeName + "," + s_serviceContractAssemblyName, throwOnError: true)!;

            if (!queryType.HasClosedInterface(typeof(IQuery<>)))
                throw new ArgumentException("Query type is invalid.", nameof(request));

            var query = (IQuery)ServiceHostContractSerializer.Default.Deserialize(request.SerializedQuery ?? Array.Empty<byte>(), queryType);

            object? result = null;
            ServiceErrorException? errorException = null;

            Task<object?> dispatchTask;

            if (notifyEvents && query is IEventProducerQuery eventProducerQuery)
            {
                var channel = Channel.CreateUnbounded<Event>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                });

                eventProducerQuery.OnEvent = (_, @event) => channel.Writer.TryWrite(@event);

                dispatchTask = Task.Run(async () =>
                {
                    try { return await _queryDispatcher.DispatchAsync(query, cancellationToken); }
                    finally { channel.Writer.Complete(); }
                });

                while (await channel.Reader.WaitToReadAsync(cancellationToken))
                    while (channel.Reader.TryRead(out var @event))
                        yield return new QueryResponse.Notification
                        {
                            Event = new EventData { Value = @event }
                        };
            }
            else
                dispatchTask = _queryDispatcher.DispatchAsync(query, cancellationToken);

            try { result = await dispatchTask; }
            catch (ServiceErrorException ex) { errorException = ex; }

            if (errorException != null)
                yield return new QueryResponse.Failure { Error = errorException.ToData() };
            else
                yield return
                    result != null ?
                    new QueryResponse.Success { SerializedResult = ServiceHostContractSerializer.Default.Serialize(result, result.GetType()) } :
                    new QueryResponse.Success { IsResultNull = true };
        }

        public async ValueTask<QueryResponse> Invoke(QueryRequest request, CallContext context = default)
        {
            await using var enumerator = InvokeCore(request, notifyEvents: false, context.CancellationToken).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
                throw new InvalidOperationException();

            return enumerator.Current;
        }

        public IAsyncEnumerable<QueryResponse> InvokeWithEventNotification(QueryRequest request, CallContext context = default) =>
            InvokeCore(request, notifyEvents: true, context.CancellationToken);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private async IAsyncEnumerable<QueryResponse> InvokeCore(QueryRequest request, bool reportProgress, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request.QueryTypeName == null)
                throw new ArgumentException("Query type is not specified.", nameof(request));

            var queryType = Type.GetType(request.QueryTypeName + "," + s_serviceContractAssemblyName, throwOnError: true)!;

            if (!queryType.HasClosedInterface(typeof(IQuery<>)))
                throw new ArgumentException("Query type is invalid.", nameof(request));

            var query = (IQuery)ServiceHostContractSerializer.Default.Deserialize(request.SerializedQuery ?? Array.Empty<byte>(), queryType);

            ServiceErrorException? errorException = null;
            object? result = null;

            if (reportProgress && query is IProgressReporterQuery progressReporterQuery)
            {
                using (var progressEventSubject = new Subject<ProgressEventData>())
                {
                    progressReporterQuery.Progress = new Progress<ProgressEventData>(@event => progressEventSubject.OnNext(@event));

                    var dispatchStatus = Observable.FromAsync(() => _queryDispatcher.DispatchAsync(query, cancellationToken))
                        .Select(result => (executed: true, result))
                        .StartWith((executed: false, result: null));

                    var progressEvents = progressEventSubject
                        .StartWith(default(ProgressEventData)!)
                        .CombineLatest(dispatchStatus, (@event, status) => (@event, status.executed, status.result))
                        .Skip(1);

                    await using (var enumerator = progressEvents.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken))
                    {
                        for (; ; )
                        {
                            bool success;
                            try { success = await enumerator.MoveNextAsync(); }
                            catch (ServiceErrorException ex)
                            {
                                errorException = ex;
                                break;
                            }

                            if (!success || enumerator.Current.executed)
                            {
                                result = enumerator.Current.result;
                                break;
                            }

                            yield return new QueryResponse.Progress { Event = enumerator.Current.@event };
                        }
                    }
                }
            }
            else
                try { result = await _queryDispatcher.DispatchAsync(query, cancellationToken); }
                catch (ServiceErrorException ex) { errorException = ex; }

            if (errorException != null)
                yield return new QueryResponse.Failure { Error = errorException.ToData() };
            else
                yield return
                    result != null ?
                    new QueryResponse.Success { SerializedResult = ServiceHostContractSerializer.Default.Serialize(result, result.GetType()) } :
                    new QueryResponse.Success { IsResultNull = true };
        }

        public ValueTask<QueryResponse> Invoke(QueryRequest request, CallContext context = default)
        {
            return InvokeCore(request, reportProgress: false, context.CancellationToken).SingleAsync();
        }

        public IAsyncEnumerable<QueryResponse> InvokeWithProgressReporting(QueryRequest request, CallContext context = default)
        {
            return InvokeCore(request, reportProgress: true, context.CancellationToken);
        }
    }
}

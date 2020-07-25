using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Infrastructure.Logging
{
    internal sealed class QueryPerformanceLoggerInterceptor : IQueryInterceptor
    {
        private readonly QueryExecutionDelegate _next;
        private readonly IGuidProvider _guidProvider;
        private readonly ILogger _logger;

        public QueryPerformanceLoggerInterceptor(QueryExecutionDelegate next, IGuidProvider guidProvider, ILogger<QueryPerformanceLoggerInterceptor>? logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        public async Task<object?> InvokeAsync(QueryContext context, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope(new LogScopeState(context.QueryType, _guidProvider.NewGuid())))
            {
                _logger.LogInformation("{QUERY_NAME} execution started.", context.QueryType.Name);

                var startTimestamp = Stopwatch.GetTimestamp();
                Exception? exception = null;
                try
                {
                    return await _next(context, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var endTimestamp = Stopwatch.GetTimestamp();
                    var elapsed = TimeSpan.FromSeconds((endTimestamp - startTimestamp) / (double)Stopwatch.Frequency);

                    _logger.LogInformation("{QUERY_NAME} execution ended with {STATUS} in {ELAPSED}.", context.QueryType.Name, GetOperationStatus(exception), elapsed);
                }
            }

            static string GetOperationStatus(Exception? ex) => ex switch
            {
                null => "success",
                OperationCanceledException _ => "cancellation",
                ServiceErrorException serviceErrorEx => $"failure ({serviceErrorEx.ErrorCode})",
                _ => "unexpected error"
            };
        }

        private struct LogScopeState
        {
            private readonly Type _queryType;
            private readonly Guid _executionId;

            private string? _cachedToString;

            public LogScopeState(Type queryType, Guid executionId) =>
                (_queryType, _executionId, _cachedToString) = (queryType, executionId, null);

            public override string ToString() => _cachedToString ??= $"{_queryType}, ExecutionId:{_executionId}";
        }
    }
}

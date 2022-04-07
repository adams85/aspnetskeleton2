using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Infrastructure.Logging;

internal sealed class CommandPerformanceLoggerInterceptor : ICommandInterceptor
{
    private readonly CommandExecutionDelegate _next;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger _logger;

    public CommandPerformanceLoggerInterceptor(CommandExecutionDelegate next, IGuidProvider guidProvider, ILogger<CommandPerformanceLoggerInterceptor>? logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
        _logger = logger ?? (ILogger)NullLogger.Instance;
    }

    public async Task InvokeAsync(CommandContext context, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new LogScopeState(context.CommandType, _guidProvider.NewGuid())))
        {
            _logger.LogInformation("{COMMAND_NAME} execution started.", context.CommandType.Name);

            var startTimestamp = Stopwatch.GetTimestamp();
            Exception? exception = null;
            try
            {
                await _next(context, cancellationToken).ConfigureAwait(false);
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

                _logger.LogInformation("{COMMAND_NAME} execution ended with {STATUS} in {ELAPSED}.", context.CommandType.Name, GetOperationStatus(exception), elapsed);
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
        private readonly Type _commandType;
        private readonly Guid _executionId;

        private string? _cachedToString;

        public LogScopeState(Type commandType, Guid executionId) =>
            (_commandType, _executionId, _cachedToString) = (commandType, executionId, null);

        public override string ToString() => _cachedToString ??= $"{_commandType}, ExecutionId:{_executionId}";
    }
}

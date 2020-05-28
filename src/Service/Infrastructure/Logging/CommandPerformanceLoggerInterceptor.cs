using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Service.Infrastructure.Logging
{
    internal sealed class CommandPerformanceLoggerInterceptor
    {
        private readonly CommandExecutionDelegate _next;
        private readonly ILoggerFactory? _loggerFactory;

        public CommandPerformanceLoggerInterceptor(CommandExecutionDelegate next, ILoggerFactory? loggerFactory)
        {
            _next = next;
            _loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var logger = _loggerFactory?.CreateLogger(context.CommandType) ?? NullLogger.Instance;
            logger.LogInformation("Command execution started.");

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

                logger.LogInformation("Command execution ended with {STATUS} in {ELAPSED}.", GetOperationStatus(exception), elapsed);
            }

            static string GetOperationStatus(Exception? ex) => ex switch
            {
                null => "success",
                OperationCanceledException _ => "cancellation",
                ServiceErrorException serviceErrorEx => $"failure ({serviceErrorEx.ErrorCode})",
                _ => "unexpected error"
            };
        }
    }
}

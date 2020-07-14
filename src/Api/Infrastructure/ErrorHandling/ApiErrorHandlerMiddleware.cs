using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Service;

namespace WebApp.Api.Infrastructure.ErrorHandling
{
    public sealed class ApiErrorHandlerMiddleware
    {
        private static readonly Action<ILogger, Exception> s_logUnhandledException =
            LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");

        private static readonly ActionDescriptor s_emptyActionDescriptor = new ActionDescriptor();

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ApiErrorHandlerMiddleware(RequestDelegate next, ILogger<ApiErrorHandlerMiddleware>? logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        private async Task NextAsync(HttpContext context, Task nextTask)
        {
            ExceptionDispatchInfo? edi = null;
            try { await nextTask; }
            catch (Exception exception) { edi = ExceptionDispatchInfo.Capture(exception); }

            if (edi != null)
                await HandleExceptionAsync(context, edi);
        }

        public Task Invoke(HttpContext context)
        {
            ExceptionDispatchInfo edi;
            try
            {
                var nextTask = _next(context);
                return nextTask.IsCompletedSuccessfully ? Task.CompletedTask : NextAsync(context, nextTask);
            }
            catch (Exception exception) { edi = ExceptionDispatchInfo.Capture(exception); }

            return HandleExceptionAsync(context, edi);
        }

        public async Task HandleExceptionAsync(HttpContext context, ExceptionDispatchInfo edi)
        {
            if (context.Response.HasStarted)
                edi.Throw();

            try
            {
                IActionResult result;

                var exception = edi.SourceException;
                switch (exception)
                {
                    case ServiceErrorException serviceErrorException:
                        result = new ObjectResult(serviceErrorException.ToData())
                        {
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                        break;
                    default:
                        s_logUnhandledException(_logger, exception);

                        result = new StatusCodeWithReasonResult(StatusCodes.Status500InternalServerError,
                            "A server error occurred. Try again or contact the system administrator if the problem persists.");
                        break;
                }

                var routeData = context.GetRouteData() ?? new RouteData();
                var actionContext = new ActionContext(context, routeData, s_emptyActionDescriptor);

                await result.ExecuteResultAsync(actionContext);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while handling an exception.");
            }

            edi.Throw();
        }
    }
}

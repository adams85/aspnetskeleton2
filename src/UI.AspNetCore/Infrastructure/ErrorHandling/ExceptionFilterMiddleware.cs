using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using WebApp.Service;
using WebApp.UI.Helpers;

namespace WebApp.UI.Infrastructure.ErrorHandling;

public class ExceptionFilterMiddleware
{
    private static readonly Action<ILogger, Exception> s_logUnhandledException =
        LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");

    private static readonly ActionDescriptor s_emptyActionDescriptor = new ActionDescriptor();

    private static Task ClearCacheHeaders(object state)
    {
        var response = (HttpResponse)state;
        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);
        return Task.CompletedTask;
    }

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    private readonly Func<object, Task> _clearCacheHeadersDelegate;

    public ExceptionFilterMiddleware(RequestDelegate next, ILogger<ExceptionFilterMiddleware>? logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? (ILogger)NullLogger.Instance;

        _clearCacheHeadersDelegate = ClearCacheHeaders;
    }

    private bool TryHandleException(Exception ex, HttpContext context, [MaybeNullWhen(false)] out IActionResult result)
    {
        switch (ex)
        {
            case ServiceErrorException serviceErrorException:
                switch (serviceErrorException.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        result = new StatusCodeResult(StatusCodes.Status404NotFound);
                        return true;
                }
                break;
            case OperationCanceledException operationCanceledException when operationCanceledException.CancellationToken == context.RequestAborted:
                // preventing aborted requests from littering the log by swallowing the exception,
                // it doesn't matter much what status code are returned but we use Nginx's non-standard status code:
                // https://stackoverflow.com/questions/46234679/what-is-the-correct-http-status-code-for-a-cancelled-request#answer-46361806
                result = new StatusCodeResult(499);
                return true;
        }

        if (context.Request.IsAjaxRequest())
        {
            s_logUnhandledException(_logger, ex);
            result = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Content = "A server error occurred. Try again or contact the system administrator if the problem persists."
            };
            return true;
        }

        result = null;
        return false;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted && TryHandleException(ex, context, out var result))
        {
            context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

            var routeData = context.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(context, routeData, s_emptyActionDescriptor);

            await result.ExecuteResultAsync(actionContext);
        }
    }
}

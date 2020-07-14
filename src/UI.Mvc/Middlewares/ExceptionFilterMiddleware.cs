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

namespace WebApp.UI.Middlewares
{
    public class ExceptionFilterMiddleware
    {
        private static readonly Action<ILogger, Exception> s_logUnhandledException =
            LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");

        private static readonly RouteData s_emptyRouteData = new RouteData();
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
            var isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ex is ServiceErrorException serviceErrorException)
                switch (serviceErrorException.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        result = new StatusCodeResult(StatusCodes.Status404NotFound);
                        return true;
                }

            if (isAjaxRequest)
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
                await result.ExecuteResultAsync(new ActionContext(context, s_emptyRouteData, s_emptyActionDescriptor));
            }
        }
    }
}

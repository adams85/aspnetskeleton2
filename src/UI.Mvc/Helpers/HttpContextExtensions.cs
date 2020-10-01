using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Helpers
{
    public static class HttpContextExtensions
    {
        public static bool IsAjaxRequest(this HttpContext httpContext) =>
            httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}

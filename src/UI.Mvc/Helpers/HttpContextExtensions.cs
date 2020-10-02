using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Helpers
{
    public static class HttpContextExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest httpRequest) =>
            httpRequest.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}

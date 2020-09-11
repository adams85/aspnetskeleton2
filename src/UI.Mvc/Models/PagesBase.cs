using Microsoft.AspNetCore.Http;
using WebApp.UI.Helpers;

namespace WebApp.UI.Models
{
    public abstract class PagesBase
    {
        protected PagesBase() { }

        protected abstract PageInfo? GetPageByRouteCore((string, string, string) routeValues);

        public PageInfo? GetPageByRoute(string action, string controller, string? area = null) =>
            GetPageByRouteCore((action, controller, area ?? string.Empty));

        public PageInfo? GetPageByRoute(HttpContext httpContext) =>
            GetPageByRouteCore(httpContext.GetCurrentRouteValues());
    }
}

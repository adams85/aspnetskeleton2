using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Infrastructure.Navigation
{
    public class PageInfo
    {
        public PageInfo(string routeName, string? areaName = null) =>
            (RouteName, AreaName) = (routeName, areaName ?? string.Empty);

        public string RouteName { get; }
        public string AreaName { get; }

        public string? LayoutName { get; set; }

        public Func<HttpContext, Task<bool>>? IsAccessAllowedAsync { get; set; }
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; set; } = null!;
    }
}

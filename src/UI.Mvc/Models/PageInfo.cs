using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Models
{
    public class PageInfo
    {
        public PageInfo(string action, string controller, string? area = null) =>
            RouteValues = (action, controller, area ?? string.Empty);

        public (string Action, string Controller, string Area) RouteValues { get; }

        public string Action => RouteValues.Action;
        public string Controller => RouteValues.Controller;
        public string Area => RouteValues.Area;

        public Func<HttpContext, bool>? IsAccessAllowed { get; set; }
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; set; } = null!;
    }
}

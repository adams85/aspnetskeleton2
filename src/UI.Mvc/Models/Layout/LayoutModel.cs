using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Models.Layout
{
    [BindNever, ValidateNever]
    public abstract class LayoutModel
    {
        private static readonly string? s_defaultAuthor = typeof(Program).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        private static readonly string? s_defaultDescription = typeof(Program).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        public string ApplicationName { get; set; } = Program.ApplicationName;
        public string ApplicationVersion { get; set; } = Program.ApplicationVersion;

        public string? Author { get; set; } = s_defaultAuthor;
        public string? Descriptions { get; set; } = s_defaultDescription;
        public string? Keywords { get; set; }

        public PageInfo? PageInfo { get; set; }
        public LocalizedHtmlString? Title { get; set; }
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString>? GetTitle { get; set; }
        public object? BodyCssClasses { get; set; }

        public virtual void Initialize(ActionContext actionContext, IPageCatalog pages, IHtmlLocalizer htmlLocalizer)
        {
            PageInfo ??= pages.FindPage(actionContext.ActionDescriptor);
            Title ??= (GetTitle ?? PageInfo?.GetDefaultTitle)?.Invoke(actionContext.HttpContext, htmlLocalizer);
        }
    }
}

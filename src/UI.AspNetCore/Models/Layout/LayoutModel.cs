using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public PageDescriptor? PageDescriptor { get; private set; }
        public string? LayoutName { get; set; }
        public LocalizedHtmlString? Title { get; set; }
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString>? GetTitle { get; set; }
        public object? BodyCssClasses { get; set; }

        protected virtual LocalizedHtmlString? GetActualTitle(HttpContext httpContext, IHtmlLocalizer htmlLocalizer) =>
            Title ?? (GetTitle ?? PageDescriptor?.GetDefaultTitle)?.Invoke(httpContext, htmlLocalizer);

        protected virtual string? GetActualLayoutName(HttpContext httpContext) =>
            LayoutName ?? PageDescriptor?.LayoutName;

        public virtual void Initialize(ViewContext viewContext, IHtmlLocalizer htmlLocalizer)
        {
            PageDescriptor = (viewContext.ViewData.Model as IPageDescriptorProvider)?.PageDescriptor;
            LayoutName = GetActualLayoutName(viewContext.HttpContext);
            Title = GetActualTitle(viewContext.HttpContext, htmlLocalizer);
        }
    }
}

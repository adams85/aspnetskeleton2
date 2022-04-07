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
        public string ApplicationName { get; init; } = Program.ApplicationName;
        public string ApplicationVersion { get; init; } = Program.ApplicationVersion;

        public string? Author { get; init; } = s_defaultAuthor;
        public string? Descriptions { get; init; } = s_defaultDescription;
        public string? Keywords { get; init; }

        public PageDescriptor? PageDescriptor { get; private set; }

        private string? _layoutName;
        public string? LayoutName
        {
            get => _layoutName;
            init => _layoutName = value;
        }

        private LocalizedHtmlString? _title;
        public LocalizedHtmlString? Title
        {
            get => _title;
            init => _title = value;
        }

        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString>? GetTitle { get; init; }

        public object? BodyCssClasses { get; set; }

        protected virtual LocalizedHtmlString? GetActualTitle(HttpContext httpContext, IHtmlLocalizer htmlLocalizer) =>
            Title ?? GetTitle?.Invoke(httpContext, htmlLocalizer) ?? PageDescriptor?.GetDefaultTitle(httpContext, htmlLocalizer);

        protected virtual string? GetActualLayoutName(HttpContext httpContext) =>
            LayoutName ?? PageDescriptor?.LayoutName;

        public virtual void Initialize(ViewContext viewContext, IHtmlLocalizer htmlLocalizer)
        {
            PageDescriptor = (viewContext.ViewData.Model as IPageDescriptorProvider)?.PageDescriptor;
            _layoutName = GetActualLayoutName(viewContext.HttpContext);
            _title = GetActualTitle(viewContext.HttpContext, htmlLocalizer);
        }
    }
}

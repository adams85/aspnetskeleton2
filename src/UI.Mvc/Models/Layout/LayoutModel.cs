using System.Reflection;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Models.Layout
{
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
        public object? BodyCssClasses { get; set; }
    }
}

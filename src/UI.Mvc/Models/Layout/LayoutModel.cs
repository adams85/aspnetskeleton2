using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Models.Layout
{
    public abstract class LayoutModel
    {
        public string ApplicationName { get; set; } = Program.ApplicationName;
        public string ApplicationVersion { get; set; } = Program.ApplicationVersion;

        public PageInfo? PageInfo { get; set; }
        public LocalizedHtmlString? Title { get; set; }
        public object? BodyCssClasses { get; set; }
    }
}

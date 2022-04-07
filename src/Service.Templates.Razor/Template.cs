using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Options;
using WebApp.Core;

namespace WebApp.Service
{
    [AssociatedAssemblyName(ApplicationConstants.AssemblyNamePrefix + ".Service.Templates.Razor")]
    public abstract class Template<TModel> : RazorPage<TModel>
    {
        protected Template() { }

        [RazorInject]
        public IViewLocalizer T { get; init; } = null!;

        public string GetLanguageCode(CultureInfo culture) =>
            culture.Name.Length > 0 ? culture.TwoLetterISOLanguageName : "en";

        public string FormatDateTime(DateTime value, bool longTime = false) => value.ToString(longTime ? "g" : "G");

        public string GetUIBaseUrl(IOptions<ApplicationOptions> applicationOptions) =>
            applicationOptions?.Value?.UIBaseUrl ?? throw new InvalidOperationException($"{nameof(ApplicationOptions.UIBaseUrl)} must be specified.");
    }
}

using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.Service
{
    public abstract class Template<TModel> : RazorPage<TModel>
    {
        protected Template() { }

        public string FormatDateTime(DateTime value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:d MMM yyyy} {0:HH:mm} (GMT)", value);
        }

        public string GetUIBaseUrl()
        {
            var applicationOptions = ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<ApplicationOptions>>();
            return applicationOptions?.Value?.UIBaseUrl ?? throw new InvalidOperationException($"{nameof(ApplicationOptions.UIBaseUrl)} must be specified.");
        }
    }
}

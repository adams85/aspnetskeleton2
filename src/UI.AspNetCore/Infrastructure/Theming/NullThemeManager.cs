using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.AspNetCore.Http;
using WebApp.Service.Settings;

namespace WebApp.UI.Infrastructure.Theming
{
    public sealed class NullThemeManager : IThemeManager
    {
        public NullThemeManager(ISettingsProvider settingsProvider)
        {
            if (settingsProvider == null)
                throw new ArgumentNullException(nameof(settingsProvider));

            AvailableThemes = settingsProvider.AvailableThemes(out var defaultTheme);
            DefaultTheme = defaultTheme;
        }

        public IReadOnlyList<string> AvailableThemes { get; }
        public string DefaultTheme { get; }

        public Task<string> GetCurrentThemeAsync(HttpContext httpContext) => Task.FromResult(DefaultTheme);

        public Task<bool> TrySetCurrentThemeAsync(string theme, HttpContext httpContext) =>
            theme == DefaultTheme ? CachedTasks.True.Task : CachedTasks.False.Task;
    }
}

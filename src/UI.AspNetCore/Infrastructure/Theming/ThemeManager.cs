using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Service.Settings;

namespace WebApp.UI.Infrastructure.Theming
{
    public sealed class ThemeManager : IThemeManager
    {
        private readonly IThemeProvider _themeProvider;
        private readonly ILogger _logger;

        public ThemeManager(IThemeProvider themeProvider, ISettingsProvider settingsProvider, ILogger<ThemeManager>? logger)
        {
            if (settingsProvider == null)
                throw new ArgumentNullException(nameof(settingsProvider));

            _themeProvider = themeProvider ?? throw new ArgumentNullException(nameof(themeProvider));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            AvailableThemes = settingsProvider.AvailableThemes(out var defaultTheme);
            DefaultTheme = defaultTheme;
        }

        public IReadOnlyList<string> AvailableThemes { get; }
        public string DefaultTheme { get; }

        // TODO: implement if necessary
        public Task<string> GetCurrentThemeAsync(HttpContext httpContext) => Task.FromResult(DefaultTheme);

        // TODO: implement if necessary
        private Task SetCurrentThemeAsync(string theme, HttpContext httpContext) => throw new NotImplementedException();

        public async Task<bool> TrySetCurrentThemeAsync(string theme, HttpContext httpContext)
        {
            if (!AvailableThemes.Contains(theme))
                return false;

            if (!_themeProvider.GetThemes().Contains(theme))
                _logger.ThemeNotAvailable(theme, _themeProvider.GetThemePath(ThemeProvider.ThemesBasePath, theme));

            await SetCurrentThemeAsync(theme, httpContext);
            return true;
        }
    }
}

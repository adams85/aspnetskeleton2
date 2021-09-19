using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Infrastructure.Theming
{
    public interface IThemeManager
    {
        IReadOnlyList<string> AvailableThemes { get; }
        string DefaultTheme { get; }

        Task<string> GetCurrentThemeAsync(HttpContext httpContext);
        Task<bool> TrySetCurrentThemeAsync(string theme, HttpContext httpContext);
    }
}

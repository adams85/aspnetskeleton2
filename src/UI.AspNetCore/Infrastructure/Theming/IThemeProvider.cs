using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Infrastructure.Theming
{
    public interface IThemeProvider
    {
        IReadOnlyList<string> GetThemes();
        PathString GetThemePath(PathString basePath, string? name = null);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Infrastructure.Theming
{
    public sealed class ThemeProvider : IThemeProvider
    {
        private const string ThemesDirPath = "/themes";

        public static readonly PathString ThemesBasePath = "/scss";

        private readonly IWebHostEnvironment _env;

        private IReadOnlyList<string>? _themes;

        public ThemeProvider(IWebHostEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public IReadOnlyList<string> GetThemes() => LazyInitializer.EnsureInitialized(ref _themes, () =>
            _env.WebRootFileProvider.GetDirectoryContents(GetThemePath(ThemesBasePath))
                .Where(fileInfo => fileInfo.IsDirectory)
                .Select(fileInfo => fileInfo.Name)
                .ToArray());

        public PathString GetThemePath(PathString basePath, string? name = null) =>
            basePath.Add(name != null ? ThemesDirPath + "/" + name : ThemesDirPath);
    }
}

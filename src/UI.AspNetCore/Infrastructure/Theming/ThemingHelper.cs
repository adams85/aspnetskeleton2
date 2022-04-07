using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WebApp.UI.Infrastructure.Theming;

public static class ThemingHelper
{
    private static readonly ConcurrentDictionary<string, object?> s_unavailableThemes = new ConcurrentDictionary<string, object?>();

    internal static void ThemeNotAvailable(this ILogger logger, string name, string path)
    {
        if (s_unavailableThemes.TryAdd(name, default))
            logger.LogWarning("Theme '{NAME}' was not found at the following path: \"{PATH}\".", name, path);
    }
}

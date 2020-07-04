using System;
using System.Collections.Concurrent;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Logging;

namespace WebApp.Service.Infrastructure.Localization
{
    internal static class LocalizationHelper
    {
        private static readonly ConcurrentDictionary<(string, string?), object?> s_unavailableTranslations = new ConcurrentDictionary<(string, string?), object?>();

        public static (Plural, TextContext) GetSpecialArgs(object[] args)
        {
            var plural = (Plural?)Array.Find(args, arg => arg is Plural);
            var context = args.Length > 0 ? args[^1] as TextContext? : null;

            return (plural ?? default, context ?? default);
        }

        public static void TranslationNotFound(this ILogger logger, string name, string? searchedLocation)
        {
            if (s_unavailableTranslations.TryAdd((name, searchedLocation), default))
                logger.LogWarning("Translation for \"{NAME}\" was not found at the following location(s): {LOCATION}.", name, searchedLocation);
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Service.Infrastructure.Localization
{
    public interface IExtendedStringLocalizer : IStringLocalizer, ITextLocalizer
    {
        string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound);
        bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, [MaybeNullWhen(false)] out string value);

        bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
        bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
    }
}

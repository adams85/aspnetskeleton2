using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Localization;

namespace WebApp.Service.Infrastructure.Localization
{
    public interface IExtendedStringLocalizer : IStringLocalizer
    {
        bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
        bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
    }
}

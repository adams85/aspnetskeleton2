using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Service.Infrastructure.Localization;

public sealed class NullStringLocalizer : IExtendedStringLocalizer
{
    public static readonly NullStringLocalizer Instance = new NullStringLocalizer();

    private NullStringLocalizer() { }

    public LocalizedString this[string name]
    {
        get
        {
            TryLocalize(name, out var searchedLocation, out var value);
            return new LocalizedString(name, value, resourceNotFound: false, searchedLocation);
        }
    }

    string ITextLocalizer.this[string hint] => this[hint].Value;

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            TryLocalize(name, arguments, out var searchedLocation, out var value);
            return new LocalizedString(name, value, resourceNotFound: false, searchedLocation);
        }
    }

    string ITextLocalizer.this[string hint, params object[] args] => this[hint, args].Value;

    public string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound)
    {
        TryGetTranslation(name, plural, context, out searchedLocation, out var value);
        resourceNotFound = false;
        return value!;
    }

    public bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out string value)
    {
        searchedLocation = null;
        value = plural.Id != null && plural.Count != 1 ? plural.Id : name;
        return true;
    }

    public bool TryLocalize(string name, out string? searchedLocation, out string value)
    {
        TryGetTranslation(name, default, default, out searchedLocation, out value!);
        return true;
    }

    public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, out string value)
    {
        var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
        TryGetTranslation(name, plural, context, out searchedLocation, out value);
        value = string.Format(value, arguments);
        return true;
    }

    [DoesNotReturn]
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotSupportedException();
}

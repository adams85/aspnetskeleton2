using System;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Service.Tests.Helpers;

public class DelegatedTextLocalizer : ITextLocalizer
{
    private readonly Func<string, object[]?, string> _localizer;

    public DelegatedTextLocalizer(Func<string, object[]?, string> localizer)
    {
        _localizer = localizer;
    }

    public string this[string hint] => _localizer(hint, null);

    public string this[string hint, params object[] args] => _localizer(hint, args);
}

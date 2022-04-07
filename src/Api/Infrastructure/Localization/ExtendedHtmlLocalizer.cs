﻿using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.Service.Infrastructure.Localization;

#if SERVICE_HOST
namespace WebApp.Service.Host.Infrastructure.Localization;
#else
namespace WebApp.Api.Infrastructure.Localization;
#endif

public sealed class ExtendedHtmlLocalizer : HtmlLocalizer
{
    private readonly IExtendedStringLocalizer _stringLocalizer;

    public ExtendedHtmlLocalizer(IExtendedStringLocalizer stringLocalizer) : base(stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    public override LocalizedHtmlString this[string name]
    {
        get
        {
            var translation = _stringLocalizer.GetTranslation(name, default, default, out var _, out var resourceNotFound);
            return new LocalizedHtmlString(name, translation, resourceNotFound);
        }
    }

    public override LocalizedHtmlString this[string name, params object[] arguments]
    {
        get
        {
            var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
            var translation = _stringLocalizer.GetTranslation(name, plural, context, out var _, out var resourceNotFound);
            return new LocalizedHtmlString(name, translation, resourceNotFound, arguments);
        }
    }
}

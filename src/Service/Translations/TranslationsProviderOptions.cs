using System;

namespace WebApp.Service.Translations;

public class TranslationsProviderOptions
{
    public static readonly TimeSpan DefaultDelayOnRefreshError = TimeSpan.FromSeconds(5);

    public TimeSpan? DelayOnRefreshError { get; set; }
}

using System;

namespace WebApp.Service.Translations;

public class FileTranslationsSourceOptions
{
    public static readonly TimeSpan DefaultDelayOnWatcherError = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan DefaultReloadDelay = TimeSpan.FromMilliseconds(250);
    public static readonly bool DefaultReloadOnChange = true;

    public string? TranslationsBasePath { get; set; }
    public bool? ReloadOnChange { get; set; }
    public TimeSpan? ReloadDelay { get; set; }

    public TimeSpan? DelayOnWatcherError { get; set; }
}

using System;

namespace WebApp.Service.Settings
{
    public class SettingsSourceOptions
    {
        public static readonly TimeSpan DefaultDelayOnLoadError = TimeSpan.FromSeconds(1);

        public TimeSpan? DelayOnLoadError { get; set; }
    }
}

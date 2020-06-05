using System;

namespace WebApp.Service.Settings
{
    public class SettingsProviderOptions
    {
        public static readonly TimeSpan DefaultDelayOnRefreshError = TimeSpan.FromSeconds(5);

        public TimeSpan? DelayOnRefreshError { get; set; }
    }
}

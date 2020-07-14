using System;

namespace WebApp.UI.Infrastructure.Security
{
    public class UISecurityOptions
    {
        public static readonly string DefaultSectionName = "Security:UI";

        public static readonly TimeSpan DefaultPasswordTokenExpirationTime = TimeSpan.FromDays(1);

        public TimeSpan? PasswordTokenExpirationTime { get; set; }
    }
}

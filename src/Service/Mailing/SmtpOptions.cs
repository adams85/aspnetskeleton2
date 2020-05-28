using System;
using MailKit.Security;

namespace WebApp.Service.Mailing
{
    public class SmtpOptions
    {
        public static readonly string DefaultSectionName = "Smtp";

        public static readonly int DefaultPort = 25;
        public static readonly SecureSocketOptions DefaultSecurity = SecureSocketOptions.Auto;
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

        public bool UsePickupDir { get; set; }
        public string? PickupDirPath { get; set; }

        public string Host { get; set; } = null!;
        public int? Port { get; set; }
        public SecureSocketOptions? Security { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public TimeSpan? Timeout { get; set; }
    }
}

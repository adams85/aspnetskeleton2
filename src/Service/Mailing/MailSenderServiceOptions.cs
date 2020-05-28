using System;

namespace WebApp.Service.Mailing
{
    public class MailSenderServiceOptions
    {
        public static readonly ushort DefaultBatchSize = 10;
        public static readonly TimeSpan DefaultSleepTime = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan DefaultInitialRetryTime = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan DefaultMaxRetryTime = TimeSpan.FromDays(1);
        public static readonly TimeSpan DefaultErrorWaitTime = TimeSpan.FromMilliseconds(500);

        public ushort? BatchSize { get; set; }
        public TimeSpan? SleepTime { get; set; }
        public TimeSpan? InitialRetryTime { get; set; }
        public TimeSpan? MaxRetryTime { get; set; }
        public TimeSpan? ErrorWaitTime { get; set; }
    }
}

using System;

namespace WebApp.Service.Mailing;

public class MailSenderServiceOptions
{
    public static readonly ushort DefaultBatchSize = 10;
    public static readonly TimeSpan DefaultMaxSleepTime = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan DefaultInitialSendRetryTime = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan DefaultMaxSendRetryTime = TimeSpan.FromDays(1);
    public static readonly TimeSpan DefaultDelayOnUnexpectedError = TimeSpan.FromMilliseconds(500);

    public ushort? BatchSize { get; set; }
    public TimeSpan? MaxSleepTime { get; set; }
    public TimeSpan? InitialSendRetryTime { get; set; }
    public TimeSpan? MaxSendRetryTime { get; set; }
    public TimeSpan? DelayOnUnexpectedError { get; set; }
}

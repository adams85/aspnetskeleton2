using System;

namespace WebApp.Service.Infrastructure.Events
{
    public class ServiceHostEventListenerOptions
    {
        public static readonly TimeSpan DefaultDelayOnDropout = TimeSpan.FromMilliseconds(500);

        public TimeSpan? DelayOnDropout { get; set; }
    }
}

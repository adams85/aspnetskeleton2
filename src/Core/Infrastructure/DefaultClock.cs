using System;
using System.Diagnostics;

namespace WebApp.Core.Infrastructure;

public sealed class DefaultClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public long TimestampTicks => Stopwatch.GetTimestamp();
    public long TicksPerSecond => Stopwatch.Frequency;
}

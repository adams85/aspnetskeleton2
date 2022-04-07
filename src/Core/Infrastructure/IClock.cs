using System;

namespace WebApp.Core.Infrastructure;

public interface IClock
{
    DateTime UtcNow { get; }
    long TimestampTicks { get; }
    long TicksPerSecond { get; }
}

using System;

namespace WebApp.Core.Infrastructure;

public interface IRandom
{
    int Next(int maxValue);
    void NextBytes(Span<byte> buffer);
    double NextDouble();
}

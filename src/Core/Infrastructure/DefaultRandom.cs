using System;

namespace WebApp.Core.Infrastructure
{
    public sealed class DefaultRandom : IRandom
    {
        public int Next(int maxValue) => Random.Shared.Next(maxValue);
        public void NextBytes(Span<byte> buffer) => Random.Shared.NextBytes(buffer);
        public double NextDouble() => Random.Shared.NextDouble();
    }
}

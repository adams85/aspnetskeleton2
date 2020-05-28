using System;
using System.Security.Cryptography;
using System.Threading;

namespace WebApp.Core.Infrastructure
{
    public sealed class DefaultRandom : IRandom, IDisposable
    {
        private readonly RNGCryptoServiceProvider _master;
        private readonly ThreadLocal<Random> _random;

        public DefaultRandom()
        {
            _master = new RNGCryptoServiceProvider();
            _random = new ThreadLocal<Random>(CreateRandom, trackAllValues: false);
        }

        private Random CreateRandom()
        {
            Span<byte> buffer = stackalloc byte[4];
            _master.GetBytes(buffer);
            return new Random(BitConverter.ToInt32(buffer));
        }

        public void Dispose()
        {
            _random.Dispose();
            _master.Dispose();
        }

        public int Next(int maxValue) => _random.Value.Next(maxValue);
        public void NextBytes(Span<byte> buffer) => _random.Value.NextBytes(buffer);
        public double NextDouble() => _random.Value.NextDouble();
    }
}

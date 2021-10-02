using System;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace WebApp.UI.Infrastructure
{
    public class SingleValueCache<T> : IDisposable where T : class
    {
        private T? _value;
        private IDisposable? _invalidationTokenDisposable;

        public SingleValueCache(Func<IChangeToken>? invalidationTokenProducer = null)
        {
            if (invalidationTokenProducer != null)
                _invalidationTokenDisposable = ChangeToken.OnChange(invalidationTokenProducer, () => Volatile.Write(ref _value, null));
        }

        public void Dispose()
        {
            _invalidationTokenDisposable?.Dispose();
            _invalidationTokenDisposable = null;
        }

        public T GetOrCreate(Func<T> valueFactory) => LazyInitializer.EnsureInitialized(ref _value, valueFactory);
    }
}

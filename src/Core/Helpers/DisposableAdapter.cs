using System;
using System.Threading.Tasks;

namespace WebApp.Core.Helpers
{
    public static class DisposableAdapter
    {
        public static DisposableAdapter<T> From<T>(T value) where T : IDisposable =>
            new DisposableAdapter<T>(value);
    }

    public readonly struct DisposableAdapter<T> : IDisposable, IAsyncDisposable
        where T : IDisposable
    {
        public DisposableAdapter(T value) => Value = value;

        public T Value { get; }

        public void Dispose() => Value?.Dispose();

        public ValueTask DisposeAsync()
        {
            try
            {
                if (Value is IAsyncDisposable asyncDisposable)
                    return asyncDisposable.DisposeAsync();

                Dispose();
                return default;
            }
            catch (Exception ex)
            {
                return new ValueTask(Task.FromException(ex));
            }
        }
    }
}

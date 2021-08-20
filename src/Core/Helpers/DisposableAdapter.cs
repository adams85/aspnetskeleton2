using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WebApp.Core.Helpers
{
    public static class DisposableAdapter
    {
        public static DisposableAdapter<T> From<T>([AllowNull] T value) where T : notnull, IDisposable =>
            new DisposableAdapter<T>(value);

        public static DisposableAdapter<T> From<T>([AllowNull] T value, [MaybeNull, NotNullIfNotNull("value")] out T valueOut) where T : notnull, IDisposable =>
            new DisposableAdapter<T>(valueOut = value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAsyncDisposable AsAsyncDisposable<T>(this T value, out T valueOut) where T : notnull, IAsyncDisposable =>
            valueOut = value;
    }

    public readonly struct DisposableAdapter<T> : IDisposable, IAsyncDisposable
        where T : notnull, IDisposable
    {
        public DisposableAdapter([AllowNull] T value) => Value = value;

        [AllowNull]
        public T Value { get; }

        public void Dispose() => Value?.Dispose();

        public ValueTask DisposeAsync()
        {
            if (Value is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            Dispose();
            return default;
        }
    }
}

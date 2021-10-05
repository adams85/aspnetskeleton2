using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WebApp.Core.Helpers
{
    public static class AsyncDisposableAdapter
    {
        public static AsyncDisposableAdapter<T> From<T>([AllowNull] T value) where T : notnull, IDisposable =>
            new AsyncDisposableAdapter<T>(value);

        public static AsyncDisposableAdapter<T> From<T>([AllowNull] T value, [MaybeNull, NotNullIfNotNull("value")] out T valueOut) where T : notnull, IDisposable =>
            new AsyncDisposableAdapter<T>(valueOut = value);

        // https://github.com/dotnet/csharplang/discussions/2661
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAsyncDisposable AsAsyncDisposable<T>(this T value, out T valueOut) where T : notnull, IAsyncDisposable =>
            valueOut = value;
    }

    public readonly struct AsyncDisposableAdapter<T> : IDisposable, IAsyncDisposable
        where T : notnull, IDisposable
    {
        public AsyncDisposableAdapter([AllowNull] T value) => Value = value;

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

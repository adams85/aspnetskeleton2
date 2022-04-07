using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;

namespace WebApp.Core.Helpers;

// based on: https://github.com/StephenCleary/AsyncEx/blob/v5.0.0/src/Nito.AsyncEx.Interop.WaitHandles/Interop/WaitHandleAsyncFactory.cs
public static class WaitHandleExtensions
{
    public static Task WaitAsync(this WaitHandle handle)
    {
        return WaitAsync(handle, Timeout.InfiniteTimeSpan, CancellationToken.None);
    }

    public static Task<bool> WaitAsync(this WaitHandle handle, TimeSpan timeout)
    {
        return WaitAsync(handle, timeout, CancellationToken.None);
    }

    public static Task WaitAsync(this WaitHandle handle, CancellationToken token)
    {
        return WaitAsync(handle, Timeout.InfiniteTimeSpan, token);
    }

    public static Task<bool> WaitAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken token)
    {
        // Handle synchronous cases.
        var alreadySignalled = handle.WaitOne(0);
        if (alreadySignalled)
            return CachedTasks.True.Task;
        if (timeout == TimeSpan.Zero)
            return CachedTasks.False.Task;
        if (token.IsCancellationRequested)
            return Task.FromCanceled<bool>(token);

        // Register all asynchronous cases.
        return WaitCoreAsync(handle, timeout, token);
    }

    private static async Task<bool> WaitCoreAsync(WaitHandle handle, TimeSpan timeout, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<bool>();

        var registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
            (state, timedOut) => ((TaskCompletionSource<bool>)state!).TrySetResult(!timedOut),
            tcs, timeout, executeOnlyOnce: true);

        try
        {
            using (token.Register(state => ((TaskCompletionSource<bool>)state!).TrySetCanceled(), tcs, useSynchronizationContext: false))
                return await tcs.Task.ConfigureAwait(false);
        }
        finally { registeredWaitHandle.Unregister(null); }
    }
}

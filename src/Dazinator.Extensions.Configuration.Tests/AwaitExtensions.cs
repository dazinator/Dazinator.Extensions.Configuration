namespace Dazinator.Extensions.Configuration.Tests;

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

/// <summary>
/// See https://gist.github.com/AArnott/1084951
/// </summary>
public static class AwaitExtensions
{
    /// <summary>
    /// Provides await functionality for ordinary <see cref="WaitHandle"/>s.
    /// </summary>
    /// <param name="handle">The handle to wait on.</param>
    /// <returns>The awaiter.</returns>
    public static TaskAwaiter GetAwaiter(this WaitHandle handle) => handle.ToTask().GetAwaiter();

    /// <summary>
    /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
    /// </summary>
    /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
    /// <returns>A Task that is completed after the handle is signaled.</returns>
    /// <remarks>
    /// There is a (brief) time delay between when the handle is signaled and when the task is marked as completed.
    /// </remarks>
    public static Task ToTask(this WaitHandle handle)
    {
        var tcs = new TaskCompletionSource<object>();
        var localVariableInitLock = new object();
        lock (localVariableInitLock)
        {
            RegisteredWaitHandle callbackHandle = null;
            callbackHandle = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) =>
                {
                    tcs.SetResult(null);
                    // We take a lock here to make sure the outer method has completed setting the local variable callbackHandle.
                    lock (localVariableInitLock)
                    {
                        callbackHandle.Unregister(null);
                    }
                },
                state: null,
                millisecondsTimeOutInterval: Timeout.Infinite,
                executeOnlyOnce: true);
        }

        return tcs.Task;
    }
}

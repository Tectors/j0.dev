using System;
using System.Threading.Tasks;

using Avalonia.Threading;

using vj0.Framework;

namespace vj0.Services;

public class TaskService : IService
{
    public delegate void ExceptionDelegate(Exception exception);
    public event ExceptionDelegate? Exception;

    /* ~~~ Background ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

    public void Run(Func<Task> function)
    {
        Task.Run(async () =>
        {
            try
            {
                await function().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
            }
        });
    }

    public Task Run(Action function)
    {
        return Task.Run(() =>
        {
            try
            {
                function();
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
            }
        });
    }

    /* ~~~ Dispatcher (UI thread) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

    public void RunDispatcher(Action function, DispatcherPriority priority = default)
    {
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                try
                {
                    function();
                }
                catch (Exception e)
                {
                    Exception?.Invoke(e);
                }
            }, priority);
        }
        catch (Exception e)
        {
            Exception?.Invoke(e);
        }
    }

    public Task RunDispatcherAsync(Action function, DispatcherPriority priority = default)
    {
        var tcs = new TaskCompletionSource<object?>();

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                function();
                tcs.SetResult(null);
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
                tcs.SetException(e);
            }
        }, priority);

        return tcs.Task;
    }

    public Task RunDispatcherAsync(Func<Task> function, DispatcherPriority priority = default)
    {
        var tcs = new TaskCompletionSource<object?>();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await function();
                tcs.SetResult(null);
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
                tcs.SetException(e);
            }
        }, priority);

        return tcs.Task;
    }
}

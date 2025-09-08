using System;

using Avalonia.Threading;

namespace vj0.Models;

public static class RelativeTimeClock
{
    public static event EventHandler? Tick;

    public static DateTime Now { get; private set; } = DateTime.UtcNow;

    static RelativeTimeClock()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            Now = DateTime.UtcNow;
            Tick?.Invoke(null, EventArgs.Empty);
        };
        timer.Start();
    }
}
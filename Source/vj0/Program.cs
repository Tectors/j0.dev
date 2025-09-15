using Avalonia;

using System;
using System.Threading;

using vj0.Application;

namespace vj0;

internal sealed class Program
{
    private static Mutex? mutex;

    [STAThread]
    public static void Main(string[] args)
    {
        /* Stops the app being opened more than once */
        mutex = new Mutex(true, INSTANCE_NAME, out var isNewInstance);
        if (!isNewInstance)
        {
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        mutex.ReleaseMutex();
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<AppInstance>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseSkia()
            .With(new SkiaOptions
            {
                MaxGpuResourceSizeBytes = 144_691_200,
                UseOpacitySaveLayer = false,
            })
            .LogToTrace();
    }
}

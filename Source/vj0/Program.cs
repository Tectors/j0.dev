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
        const string mutexName = Globals.INSTANCE_NAME;

        mutex = new Mutex(true, mutexName, out var isNewInstance);
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
                MaxGpuResourceSizeBytes = null,
                UseOpacitySaveLayer = false,
            })
            .LogToTrace();
    }
}
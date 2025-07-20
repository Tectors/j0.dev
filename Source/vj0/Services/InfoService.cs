using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using vj0.Application;
using vj0.Framework;
using vj0.Models.Information;
using vj0.Shared.Extensions;

namespace vj0.Services;

public partial class InfoService : ObservableObject, ILogEventSink, IService
{
    private string LogFilePath = null!;
    
    [ObservableProperty] private ObservableCollection<MessageInfo> _messages = [];

    public InfoService()
    {
        Dispatcher.UIThread.UnhandledException += (_, args) =>
        {
            args.Handled = true;
            
            OnException(args.Exception);
        };
    }

    public void Create()
    {
        Globals.LogsFolder.Create();
        
        LogFilePath = Path.Combine(Globals.LogsFolder.FullName, $"{Globals.CODENAME}-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.log");
        var loggerConfiguration = new LoggerConfiguration();

        loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
            .WriteTo.Sink(this)
            .WriteTo.File(LogFilePath);
        
        Log.Logger = loggerConfiguration.CreateLogger();
    }

    private void OnException(Exception ex)
    {
        var exceptionAsString = ex.ToString();
        Log.Error(exceptionAsString);
        
        Tasks.RunDispatcher(async void () =>
        {
            var dialog = new ContentDialog
            {
                Title = "An unhandled exception has occurred",
                Content = exceptionAsString,
                
                CloseButtonText = "Continue",
            };
            
            await dialog.ShowAsync();
        });
    }
    
    public void Emit(LogEvent logEvent)
    {
    }
    
    public void Message(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, bool autoClose = true, string id = "", float closeTime = 5.0f, bool useButton = false, string buttonTitle = "", Action? buttonCommand = null)
    {
        Message(new MessageInfo
        {
            Title = title,
            Message = message,
            Severity = severity,
            AutoClose = autoClose,
            CloseTime = closeTime,
            Id = id,
            ButtonCommand = new RelayCommand(buttonCommand ?? (() => { })),
            ButtonTitle = buttonTitle,
            UseButton = useButton
        });
    }

    private void Message(MessageInfo data)
    {
        if (!string.IsNullOrEmpty(data.Id))
        {
            Messages.RemoveAll(bar => bar.Id.Equals(data.Id));
        }
        
        Messages.Add(data);
        if (!data.AutoClose) return;
        
        Tasks.Run(async () =>
        {
            await Task.Delay((int)(data.CloseTime * 1000));
            
            Messages.Remove(data);
        });
    }
}
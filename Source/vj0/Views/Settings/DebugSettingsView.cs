using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using vj0.Application;
using vj0.Extensions;
using vj0.Framework.Models;
using vj0.Services;
using vj0.Services.Framework;
using vj0.ViewModels.Settings;
using vj0.Windows;

namespace vj0.Views.Settings;

public partial class DebugSettingsView : ViewBase<DebugSettingsViewModel>
{
    public DebugSettingsView() : base(AppServices.Settings.Debug)
    {
        InitializeComponent();
    }

    private void OpenLogsFolder(object? sender, RoutedEventArgs e)
    {
        var folder = Globals.LogsFolder.FullName;

        if (Directory.Exists(folder))
        {
            AppService.OpenLink(folder);
        }
    }

    private void OpenStartup(object? sender, RoutedEventArgs e)
    {
        var win = new OnboardingWindow();

        if (this.GetVisualRoot() is not Window window) return;
        
        win.CenterToScreen(window);
        win.Show();
    }
}
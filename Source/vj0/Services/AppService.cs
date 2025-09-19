using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

using FluentAvalonia.UI.Controls;

using Microsoft.WindowsAPICodePack.Taskbar;

using vj0.Framework;
using vj0.Models;

namespace vj0.Services;

public class AppService : IService
{
    private IClassicDesktopStyleApplicationLifetime Lifetime = null!;
    private IClipboard Clipboard => Lifetime.MainWindow!.Clipboard!;

    public void Initialize(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Lifetime = desktop;

        Info.Create();
        Settings.Load();

        Lifetime.Exit += OnAppClose;
    }

    private void OnAppClose(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Settings.Save();
        
#pragma warning disable IL3002
        App.RefreshWindowJumpList();
#pragma warning restore IL3002
    }

    private IStorageProvider StorageProvider => Lifetime.MainWindow!.StorageProvider;

    public async Task<string?> BrowseFolderDialog(string startLocation = "")
    {
        var suggestedStart = !string.IsNullOrWhiteSpace(startLocation)
            ? await StorageProvider.TryGetFolderFromPathAsync(startLocation)
            : null;

        var folder = (await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            SuggestedStartLocation = suggestedStart,
            Title = "Select a Folder"
        })).FirstOrDefault();

        return folder is not null ? Uri.UnescapeDataString(folder.Path.AbsolutePath) : null;
    }

    public async Task<string?> BrowseFileDialog(string suggestedFileName = "", params FilePickerFileType[] fileTypes)
    {
        var file = (await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = fileTypes,
            SuggestedFileName = suggestedFileName,
            Title = "Select a File"
        })).FirstOrDefault();

        return file is not null ? Uri.UnescapeDataString(file.Path.AbsolutePath) : null;
    }

    public static void OpenLink(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open link: {ex.Message}");
        }
    }

    public void CopyText(string Text)
    {
        Info.Message($"Copied to Clipboard", "", InfoBarSeverity.Success, closeTime: 0.35f);
        App.Clipboard.SetTextAsync(Text);
    }

    public void SetAppProgressState(TaskbarProgressBarState State)
    {
        if (TaskbarManager.IsPlatformSupported || !HasActiveWindow())
        {
            TaskbarManager.Instance.SetProgressState(State);
        }
    }

    private static bool HasActiveWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow is { IsVisible: true };
        }
        return false;
    }

    [RequiresAssemblyFiles]
    private void RefreshWindowJumpList()
    {
        if (!TaskbarManager.IsPlatformSupported || !HasActiveWindow())
        {
            return;
        }

        var jumpList = JumpList.CreateJumpList();
        var exePath = Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe");
        
        var recentCategory = new JumpListCustomCategory("Recent");

        foreach (var Profile in GameDetection.GetRecentlyUsedProfiles(5))
        {
            var profileLink = new JumpListLink(exePath, Profile.Name)
            {
                Arguments = $"--launchProfile={Profile.FileID}"
            };

            recentCategory.AddJumpListItems(profileLink);
        }
        
        jumpList.AddCustomCategories(recentCategory);
        jumpList.Refresh();
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

using FluentAvalonia.UI.Controls;

using vj0.Framework;

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
}

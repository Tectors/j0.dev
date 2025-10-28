using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

using FluentAvalonia.UI.Controls;
using SharpCompress.Archives;
using SharpCompress.Common;
using vj0.Framework;

namespace vj0.Services;

public class UpdateService : IService
{
    public async void Initialize()
    {
        if (!Version.TryParse(VERSION, out var currentVersion))
        {
            return;
        }
        
        var latestRelease = await RestAPI.GitHub.GetLatestRelease();
        if (latestRelease is null) return;

        var latestVersion = new Version(latestRelease.Name);
        
        if (Globals.HideVersionPrompt) return;
        
        if (currentVersion < latestVersion)
        {
            var dialog = new ContentDialog
            {
                Title = "New Version Available!",
                Content = $"Get the latest features and improvements for {APP_NAME}.",
                CloseButtonText = "Dismiss",
                PrimaryButtonText = "Download & Install",
                PrimaryButtonCommand = new RelayCommand(async () =>
                {
                    var asset = latestRelease.Assets.FirstOrDefault();
                    if (asset != null)
                    {
                        await DownloadAndInstall(latestRelease.Name, asset.DownloadUrl);
                    }
                })
            };
            
            _ = dialog.ShowAsync();
        }

        if (currentVersion > latestVersion)
        {
#if !DEBUG
            var dialog = new ContentDialog
            {
                Title = "Development Build",
                Content = $"You are currently running a developmental build of {APP_NAME}.\n\nThis issued version may be unstable.",
                CloseButtonText = "Dismiss"
            };
            
            _ = dialog.ShowAsync();
#endif
        }
    }

    private static async Task DownloadAndInstall(string versionName, string downloadUrl)
    {
        try
        {
            var installationFolder = new DirectoryInfo(Path.Combine(InstallationFolder.ToString(), versionName));
            
            if (!installationFolder.Exists)
            {
                installationFolder.Create();
            }
            
            var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
            var installPath = Path.Combine(installationFolder.FullName, fileName);
            
            var downloaded = await RestAPI.DownloadFileAsync(downloadUrl, installPath, _ => { });

            if (downloaded is null)
            {
                throw new InvalidOperationException("Download returned no data.");
            }
            
            using var archive = ArchiveFactory.Open(installPath);
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                entry.WriteToDirectory(
                    installationFolder.FullName,
                    new ExtractionOptions { ExtractFullPath = true, Overwrite = true }
                );
            }
            
            var exe = installationFolder
                .EnumerateFiles("*.exe", SearchOption.AllDirectories)
                .OrderByDescending(f => f.Length)
                .FirstOrDefault();

            if (exe != null && exe.Exists)
            {
                try
                {
                    Program.ReleaseMutex();

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exe.FullName
                    };
                    Process.Start(startInfo);
                }
                catch (Exception launchEx)
                {
                    AppService.OpenLink(installationFolder.FullName);
                    throw new InvalidOperationException($"Failed to launch the new executable:\n{launchEx.Message}");
                }
            }
            else
            {
                AppService.OpenLink(installationFolder.FullName);
            }
            
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Update Failed",
                Content = $"Could not download or install the update:\n\n{ex.Message}",
                CloseButtonText = "Dismiss"
            };

            _ = dialog.ShowAsync();
        }
    }
}

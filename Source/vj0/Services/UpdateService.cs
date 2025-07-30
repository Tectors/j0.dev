using System;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using vj0.Application;
using vj0.Framework;

namespace vj0.Services;

public class UpdateService : IService
{
    public async void Initialize()
    {
        var latestRelease = await RestAPI.GitHub.GetLatestRelease();
        if (latestRelease is null) return;

        var latestVersion = new Version(latestRelease.Name);
        var currentVersion = new Version(Globals.VERSION);
        
        if (true)
        {
            var dialog = new ContentDialog
            {
                Title = "New Version Available!",
                Content = $"Get the latest features and improvements for {Globals.APP_NAME}.",
                CloseButtonText = "Dismiss",
                PrimaryButtonText = "View Release Page",
                PrimaryButtonCommand = new RelayCommand(() =>
                {
                    AppService.OpenLink(latestRelease.TagUrl);
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
                Content = $"You are currently running a developmental build of {Globals.APP_NAME}.\n\nThis issued version may be unstable.",
                CloseButtonText = "Dismiss"
            };
            
            _ = dialog.ShowAsync();
#endif
        }
    }
}
using System;
using vj0.Application;
using vj0.Framework;

namespace vj0.Services;

public partial class UpdateService : IService
{
    public async void Initialize()
    {
        var latestRelease = await RestAPI.GitHub.GetLatestRelease();
        if (latestRelease is null) return;

        var latestVersion = new Version(latestRelease.Name);
        var currentVersion = new Version(Globals.VERSION);
        
        if (true) // currentVersion < latestVersion)
        {
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CUE4Parse.UE4.Versions;
using Serilog;
using vj0.Models.Profiles;
using vj0.Shared.Framework.Base;

namespace vj0.Models;

public static class GameDetection
{
    public static List<Profile> LoadedProfiles = [];
    
    public static List<Profile> GetRecentlyUsedProfiles(int count)
    {
        return LoadedProfiles
            .Where(p => p.Display.LastUsed.HasValue)
            .OrderByDescending(p => p.Display.LastUsed!.Value)
            .Take(count)
            .ToList();
    }

    public static async Task LoadAllAsync()
    {
        LoadedProfiles = await Profile.LoadAllAsync();
    }
    
    public static async Task DetectAllProfilesAsync(Action<Profile>? onDetected = null)
    {
        await TryDetectGameAsync(
            gameId: EDetectedGameId.Fortnite,
            detectFunc: () =>
            {
                var latestVersion = EGame.GAME_UE5_LATEST;

                /* ReSharper disable once ConditionIsAlwaysTrueOrFalse */
                if (latestVersion == EGame.GAME_UE5_6)
                {
                    latestVersion = EGame.GAME_UE5_7;
                }

                return DetectEpicGame("Fortnite", @"\FortniteGame\Content\Paks", latestVersion, EDetectedGameId.Fortnite);
            },
            onDetected
        );

        await TryDetectGameAsync(
            gameId: EDetectedGameId.Valorant,
            detectFunc: () =>
                DetectValorantGame("VALORANT", @"\ShooterGame\Content\Paks", EGame.GAME_Valorant, EDetectedGameId.Valorant),
            onDetected
        );
    }
    
    private static async Task TryDetectGameAsync(
        EDetectedGameId gameId,
        Func<Profile?> detectFunc,
        Action<Profile>? onDetected)
    {
        var existingProfile = LoadedProfiles.FirstOrDefault(p => p.AutoDetectedGameId == gameId);
        if (existingProfile is not null)
        {
            await existingProfile.Save();
            return;
        }

        var detectedProfile = detectFunc();
        if (detectedProfile is not null)
        {
            Log.Information($"Detected {detectedProfile.Name} at {detectedProfile.ArchiveDirectory}");

            await detectedProfile.Save();
            LoadedProfiles.Add(detectedProfile);

            onDetected?.Invoke(detectedProfile);
        }
    }
    
#pragma warning disable CA1416
    private static Profile? DetectValorantGame(string appName, string pakPath, EGame version, EDetectedGameId gameId)
    {
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\tof_launcher");
            if (key is not null)
            {
                var installPath = key.GetValue("GameInstallPath") as string;
                if (!string.IsNullOrWhiteSpace(installPath))
                {
                    var fullPath = Path.Combine(installPath, pakPath.TrimStart('\\'));
                    if (Directory.Exists(fullPath))
                    {
                        return new Profile
                        {
                            Name = appName,
                            ArchiveDirectory = fullPath,
                            Version = version,
                            AutoDetectedGameId = gameId
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to detect VALORANT");
        }

        return null;
    }
#pragma warning restore CA1416

    private static Profile? DetectEpicGame(string appName, string pakPath, EGame version, EDetectedGameId gameId)
    {
        var launcherPath = GetLauncherInstalledPath();
        if (launcherPath is null) return null;

        return (from install in launcherPath.InstallationList where install.AppName!.Equals(appName, StringComparison.OrdinalIgnoreCase) select Path.Combine(install.InstallLocation!, pakPath.TrimStart('\\')) 
        into fullPath where Directory.Exists(fullPath) 
        select new Profile
        {
            Name = appName,
            ArchiveDirectory = fullPath,
            Version = version,
            AutoDetectedGameId = gameId
        }).FirstOrDefault();
    }
    
    private static LauncherInstalled? GetLauncherInstalledPath()
    {
        return (from drive in DriveInfo.GetDrives() select Path.Combine(drive.Name, "ProgramData", "Epic", "UnrealEngineLauncher", "LauncherInstalled.dat") into path where File.Exists(path) select File.ReadAllText(path) into json select JsonSerializer.Deserialize<LauncherInstalled>(json)).FirstOrDefault();
    }

    private class LauncherInstalled
    {
        public Installation[]? InstallationList { get; set; }
    }

    /* ReSharper disable once ClassNeverInstantiated.Local */
    private class Installation
    {
        /* ReSharper disable once UnusedAutoPropertyAccessor.Local */
        public string? InstallLocation { get; set; }
        /* ReSharper disable once UnusedAutoPropertyAccessor.Local */
        public string? AppName { get; set; }
    }
}

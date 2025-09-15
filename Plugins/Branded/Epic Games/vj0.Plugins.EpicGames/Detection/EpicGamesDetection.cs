using System.Text.Json;

using CUE4Parse.UE4.Versions;

using vj0.Plugins.Interfaces;
using vj0.Core.Framework.Base;

namespace vj0.Plugins.EpicGames.Detection;

public interface IEpicGamesDetection : IGameDetectionPlugin
{
    public static BaseProfile? DetectGame(string appName, string pakPath, EGame version, string gameId)
    {
        var launcherPath = GetLauncherInstalledPath();
        if (launcherPath is null) return null;

        return (from install in launcherPath.InstallationList where install.AppName!.Equals(appName, StringComparison.OrdinalIgnoreCase) select Path.Combine(install.InstallLocation!, pakPath.TrimStart('\\')) 
            into fullPath where Directory.Exists(fullPath) 
            select new BaseProfile
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

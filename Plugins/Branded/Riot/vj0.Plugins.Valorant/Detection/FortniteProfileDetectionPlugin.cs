using CUE4Parse.UE4.Versions;
using Serilog;
using vj0.Plugins.Interfaces;
using vj0.Core.Framework.Base;

namespace vj0.Plugins.Valorant.Detection;

public sealed class ValorantProfileDetectionPlugin : IGameDetectionPlugin
{
    public string Name => "Valorant Profile Detection";
    
    public async void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null)
    {
        await IGameDetectionPlugin.TryDetectGameAsync(
            gameId: "Valorant",
            detectFunc: () =>
                DetectValorantGame("VALORANT", @"\ShooterGame\Content\Paks", EGame.GAME_Valorant, "Valorant"),
            onDetected,
            LoadedProfiles
        );
    }
    
#pragma warning disable CA1416
    private static BaseProfile? DetectValorantGame(string appName, string pakPath, EGame version, string gameId)
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
                        return new BaseProfile
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
}

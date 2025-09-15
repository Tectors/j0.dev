using Serilog;
using vj0.Core.Framework.Base;

namespace vj0.Plugins.Interfaces;

public interface IGameDetectionPlugin : IPlugin
{
    void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null);
    
    public static Task TryDetectGameAsync(
        EDetectedGameId gameId,
        Func<BaseProfile?> detectFunc,
        Action<BaseProfile>? onDetected,
        List<BaseProfile> LoadedProfiles)
    {
        var detectedProfile = detectFunc();
        if (detectedProfile is not null)
        {
            Log.Information($"Detected {detectedProfile.Name} at {detectedProfile.ArchiveDirectory}");

            LoadedProfiles.Add(detectedProfile);

            onDetected?.Invoke(detectedProfile);
        }

        return Task.CompletedTask;
    }
}

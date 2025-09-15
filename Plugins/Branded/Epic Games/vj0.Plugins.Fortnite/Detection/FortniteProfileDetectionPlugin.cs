using CUE4Parse.UE4.Versions;

using vj0.Plugins.EpicGames.Detection;
using vj0.Core.Framework.Base;

namespace vj0.Plugins.Fortnite.Detection;

public sealed class FortniteProfileDetectionPlugin : IEpicGamesDetection
{
    public string Name => "Fortnite Profile Detection";
    
    public async void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null)
    {
        await IEpicGamesDetection.TryDetectGameAsync(
            gameId: "Fortnite",
            detectFunc: () => IEpicGamesDetection.DetectGame("Fortnite", @"\FortniteGame\Content\Paks", EGame.GAME_UE5_LATEST, "Fortnite"),
            onDetected,
            LoadedProfiles
        );
    }
}

using CUE4Parse.UE4.Versions;

using vj0.Plugins.EpicGames.Detection;
using vj0.Core.Framework.Base;
using vj0.Plugins.Interfaces;

namespace vj0.Plugins.Fortnite.Detection;

public sealed class FortniteProfileDetectionPlugin : IEpicGamesDetection, IGameVersionUpdatePlugin
{
    public string GameId => "Fortnite";
    public string Name => "Fortnite Profile Detection";
    public EGame TargetVersion => EGame.GAME_UE5_LATEST;
    
    public async void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null)
    {
        await IEpicGamesDetection.TryDetectGameAsync(
            gameId: "Fortnite",
            detectFunc: () => IEpicGamesDetection.DetectGame("Fortnite", @"\FortniteGame\Content\Paks", TargetVersion, "Fortnite"),
            onDetected,
            LoadedProfiles
        );
    }
}

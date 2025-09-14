using CUE4Parse.UE4.Versions;

using vj0.Plugins.EpicGames.Detection;
using vj0.Shared.Framework.Base;

namespace vj0.Plugins.Fortnite.Detection;

public sealed class FortniteProfileDetectionPlugin : IEpicGamesDetection
{
    public string Name => "Fortnite Profile Detection";
    
    public async void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null)
    {
        await IEpicGamesDetection.TryDetectGameAsync(
            gameId: EDetectedGameId.Fortnite,
            detectFunc: () =>
            {
                var latestVersion = EGame.GAME_UE5_LATEST;

                /* ReSharper disable once ConditionIsAlwaysTrueOrFalse */
                if (latestVersion == EGame.GAME_UE5_6)
                {
                    latestVersion = EGame.GAME_UE5_7;
                }

                return IEpicGamesDetection.DetectGame("Fortnite", @"\FortniteGame\Content\Paks", latestVersion, EDetectedGameId.Fortnite);
            },
            onDetected,
            LoadedProfiles
        );
    }
}
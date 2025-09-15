using vj0.Plugins.EpicGames.API;
using vj0.Plugins.EpicGames.API.Responses;

namespace vj0.Plugins.EpicGames;

public static class Globals
{
    public static EpicAuthResponse? EpicAuth;
    
    public static EpicGamesAPI API { get; } = new(vj0.API.Globals.RestClient);
}
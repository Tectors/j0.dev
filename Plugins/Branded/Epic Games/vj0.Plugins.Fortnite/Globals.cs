using vj0.API.UEDB.API;

namespace vj0.Plugins.Fortnite;

public static class Globals
{
    public static UEDB API { get; } = new(vj0.API.Globals.RestClient, "fortnite");
}
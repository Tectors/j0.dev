namespace vj0.API.UEDB;

public static class Globals
{
    /* Use mappings from FN */
    public static API.UEDB API { get; } = new(vj0.API.Globals.RestClient, "fortnite");
}
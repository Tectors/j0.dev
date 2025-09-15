using vj0.API.UEDB.API;

namespace vj0.API.UEDB;

public static class Globals
{
    public static CentralAPI API { get; } = new(vj0.API.Globals.RestClient);
}
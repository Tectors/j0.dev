using CUE4Parse.UE4.Versions;

namespace vj0.Converters.Enum;

public static class EGameNameConverter
{
    public static string ToName(EGame value)
    {
        var name = System.Enum.GetName(typeof(EGame), value);
        return name is not null && name.StartsWith("GAME_") ? name[5..] : name ?? string.Empty;
    }

    public static bool TryParse(string shortName, out EGame value)
    {
        var fullName = "GAME_" + shortName;
        return System.Enum.TryParse(fullName, out value);
    }
}

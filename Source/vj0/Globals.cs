global using static vj0.Services.Framework.AppServices;
global using static vj0.Core.Globals;

using Avalonia.Platform.Storage;

namespace vj0;

public static class Globals
{
    /* future. ?? */
    public const bool IsReadyToExplore = false;
    public const bool IsReadyToMeshExport = false;

    public static readonly FilePickerFileType MappingsFileType = new(".USMAP Files") { Patterns = [ "*.usmap" ] };

    public const bool HideAllProfileCardInformation = false;
}

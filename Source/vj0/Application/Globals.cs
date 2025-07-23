global using static vj0.Services.Framework.AppServices;

using System;
using System.IO;
using Avalonia.Platform.Storage;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace vj0.Application;

public static class Globals
{
    /* future. 🤫 */
    public static readonly bool IsReadyToExplore = false;
    
    public const string APP_NAME = "j0.dev";
    public const string CODENAME = "vj0";
    public const string INSTANCE_NAME = $"{APP_NAME}.SingleInstance";
    
    public static readonly FGuid ZERO_GUID = new();
    public const string EMPTY_CHAR = "0x0000000000000000000000000000000000000000000000000000000000000000";
    
    public const string VERSION = "1.0.0";
    public const string COMMIT = "b846caa";
    
    public const string DISCORD_LINK = "https://discord.gg/eV9DF6sBsz";
    public const string GITHUB_LINK = "https://github.com/Tectors/j0.dev";
    public const string X_LINK = "https://x.com/t3ctor";
    public const string DONATE_LINK = "https://ko-fi.com/t4ctor";
    public const string DISCORD_ACTIVITY_ID = "1386505366061453533";
    
    public static readonly FilePickerFileType MappingsFileType = new(".USMAP Files") { Patterns = [ "*.usmap" ] };
    
    private static readonly string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    
    private static readonly string DataFolder = Path.Combine(ApplicationDataFolder, CODENAME);
    public static readonly DirectoryInfo ProfilesFolder = new(Path.Combine(DataFolder, "Profiles"));
    public static readonly DirectoryInfo RuntimeFolder = new(Path.Combine(DataFolder, "Runtime"));
    public static readonly DirectoryInfo OnDemandFolder = new(Path.Combine(RuntimeFolder.ToString(), "Demand"));
    public static readonly DirectoryInfo MappingsFolder = new(Path.Combine(RuntimeFolder.ToString(), "Mappings"));
    public static readonly DirectoryInfo LogsFolder = new(Path.Combine(RuntimeFolder.ToString(), "Logs"));
    
    public static readonly bool HideAllProfileCardInformation = false;
}
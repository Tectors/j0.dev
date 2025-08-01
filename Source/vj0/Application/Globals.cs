﻿global using static vj0.Services.Framework.AppServices;

using System;
using System.IO;
using Avalonia.Platform.Storage;
using CUE4Parse.UE4.Objects.Core.Misc;
using Microsoft.IdentityModel.Tokens;

namespace vj0.Application;

public static class Globals
{
    /* Format: 0.0.0 */
    public const string VERSION = "dev";
    public const string COMMIT = "";
    public static bool IS_COMMIT_AVAILABLE => !COMMIT.IsNullOrEmpty();
    public static bool IS_COMMIT_UNAVAILABLE => !IS_COMMIT_AVAILABLE;
    
    /* future. 🤫 */
    public const bool IsReadyToExplore = true;

    /* Application Metadata */
    public const string CODENAME = "vj0";
    
    public const string APP_NAME = "j0.dev";
    public const string INSTANCE_NAME = $"{APP_NAME}.SingleInstance";

    /* GitHub Metadata */
    private const string AUTHOR_NAME = "Tectors";
    public const string GITHUB_REPO_NAME = APP_NAME;
    private const string AUTHOR_AND_GITHUB = $"{AUTHOR_NAME}/{GITHUB_REPO_NAME}";
    
    public const string GITHUB_LINK = $"https://github.com/{AUTHOR_AND_GITHUB}";
    public const string GITHUB_API_LINK = $"https://api.github.com/repos/{AUTHOR_AND_GITHUB}";
    public const string GITHUB_RELEASES_LINK = $"{GITHUB_LINK}/releases";
    public const string GITHUB_COMMIT_LINK = $"{GITHUB_LINK}/commit";
    
    /* Discord */
    public const string DISCORD_LINK = "https://discord.gg/eV9DF6sBsz";
    public const string DISCORD_ACTIVITY_ID = "1386505366061453533";
    
    /* General Links */
    public const string X_LINK = "https://x.com/t3ctor";
    public const string DONATE_LINK = "https://ko-fi.com/t4ctor";
    
    /* Application Folders */
    private static readonly string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string DataFolder = Path.Combine(ApplicationDataFolder, CODENAME);
    
    public static readonly DirectoryInfo ProfilesFolder = new(Path.Combine(DataFolder, "Profiles"));
    public static readonly DirectoryInfo RuntimeFolder = new(Path.Combine(DataFolder, "Runtime"));
    public static readonly DirectoryInfo OnDemandFolder = new(Path.Combine(RuntimeFolder.ToString(), "Demand"));
    public static readonly DirectoryInfo MappingsFolder = new(Path.Combine(RuntimeFolder.ToString(), "Mappings"));
    public static readonly DirectoryInfo LogsFolder = new(Path.Combine(RuntimeFolder.ToString(), "Logs"));
    
    /* Other Statics */
    public static readonly FGuid ZERO_GUID = new();
    public const string EMPTY_CHAR = "0x0000000000000000000000000000000000000000000000000000000000000000";
    
    public static readonly FilePickerFileType MappingsFileType = new(".USMAP Files") { Patterns = [ "*.usmap" ] };

    public const bool HideAllProfileCardInformation = false;
}
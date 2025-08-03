using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.AssetRegistry;
using CUE4Parse.UE4.AssetRegistry.Objects;
using CUE4Parse.UE4.IO;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.Utils;
using FluentAvalonia.UI.Controls;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UE4Config.Parsing;
using vj0.Application;
using vj0.Extensions;
using vj0.Models.Profiles.Display;
using vj0.Shared.Extensions;
using vj0.Shared.Framework.Base;
using vj0.Shared.Framework.CUEParse;
using vj0.Windows;

namespace vj0.Models.Profiles;

public class Profile : BaseProfileDisplay
{
    [JsonIgnore] public readonly List<FAssetData> AssetRegistry = [];
    [JsonIgnore] public Action<Profile>? OnInitialized { get; set; }
    [JsonIgnore] public Action<Profile>? OnInitializationFailure { get; set; }

    public void ResetEvents()
    {
        OnInitialized = null;
        OnInitializationFailure = null;
    }
    
    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        ExplorerVM.Reset();
        ScopeVM.Reset();

        if (AutoDetectedGameId == EDetectedGameId.Fortnite)
        {
            await RestAPI.EpicGames.VerifyAuthAsync();
        }

        CheckStatusNotifies();
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        Status.SetState(EProfileStatus.Active);
        
        UpdateStatus("Loading Native Libraries");
        
        IsInitialized = false;
        Log.Information($"Initializing profile {Name}.. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        UpdateStatus("Loading Files");
        
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        await InitializeProvider();
        
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        await InitializeTextureStreaming();
        InitializeCache();

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        await LoadKeys(cancellationToken);
        
        if (Provider is not null && Provider.Files.Count == 0 && Provider.Keys.Count == 0 && Provider.RequiredKeys.Count == 0)
        {
            Status.OnFailure("Please enter a valid AES encryption key in the profile settings.");
            OnInitializationFailure?.Invoke(this);
            
            return;
        }
        
        if (Provider is not null && Provider.Files.Count == 0)
        {
            Status.OnFailure("No files were found in the archive or the selected folder.");
            OnInitializationFailure?.Invoke(this);
            
            return;
        }

        if (Provider is not null)
        {
            Provider.LoadVirtualPaths();
            
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            await Provider.MountAsync();
            
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            SetLanguage(Settings.Application.GameLanguage);
        }

        LoadMappings(cancellationToken);
        
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        await ExplorerVM.FinalizeWhenProviderExplorerReady();

        IsInitialized = true;
        
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        Log.Information($"Initialized profile {Name} successfully");
        Info.Message($"Loaded profile {Name} successfully", "", InfoBarSeverity.Success, closeTime: 0.95f);
        UpdateStatus(string.Empty);
        
        OnInitialized?.Invoke(this);
    }

    public void SetLanguage(ELanguage language)
    {
        if (!Provider.TryChangeCulture(Provider.GetLanguageCode(language)))
        {
            Log.Information($"Failed to load language \"{language.GetDescription()}\"");
        }
        else
        {
            Log.Information($"Changed profile's provider language to \"{language.GetDescription()}\"");
        }
    }

    public void InitializeCache(bool shouldSave = true)
    {
        if (Provider is null) return;
        
        var previousPakFiles = PakFileEntries;
        PakFileEntries = GetPakFiles();

        if (!previousPakFiles.OrderBy(x => x.FileName).SequenceEqual(PakFileEntries.OrderBy(x => x.FileName)))
        {
            if (shouldSave)
            {
                _ = Save();
            }
        }
            
        var keysToRemove = new List<EncryptionKey>();

        foreach (var unknownKey in Encryption.UnknownKeys)
        {
            var pakFile = PakFileEntries.FirstOrDefault(p => p.Guid == unknownKey.Guid);
            if (pakFile is null) continue;
                
            var newKey = new EncryptionKey()
            {
                Guid = unknownKey.Guid,
                Name = pakFile.FileName,
                Key = unknownKey.Key
            };
                    
            Encryption.Keys.Add(newKey);
            keysToRemove.Add(unknownKey);
        }

        if (keysToRemove.Count > 0)
        {
            if (shouldSave)
            {
                _ = Save();
            }
        }

        foreach (var key in keysToRemove)
        {
            Encryption.UnknownKeys.Remove(key);
        }
    }
    
    public void UpdateStatus(string status) => MainWM.UpdateStatus(status);
    
    private Task InitializeProvider()
    {
        if (ArchiveDirectory.Length != 0)
        {
            Provider = new BaseProvider(ArchiveDirectory, new VersionContainer(Version, TexturePlatform));
        }

        if (!Encryption.IsValid) Encryption.MainKey = Globals.EMPTY_CHAR;
        
        Provider.VfsMounted += (sender, _) =>
        {
            MainWM.UpdateLoadedFilesDisplay();
            
            if (sender is not IAesVfsReader reader) return;

            UpdateStatus($"Loading {reader.Name}");
        };
        Provider.VfsMounted += (sender, _) =>
        {
            if (sender is not IAesVfsReader reader) return;
            ScopeVM.Verify(reader);
        };
        Provider.VfsRegistered += (sender, _) =>
        {
            if (sender is not IAesVfsReader reader) return;
            ScopeVM.Add(reader);
        };
        Provider.VfsUnmounted += (sender, _) =>
        {
            if (sender is not IAesVfsReader reader) return;
            ScopeVM.Disable(reader);
        };
        
        Provider.ReadScriptData = Settings.Serialization.ReadBlueprintBytecode;
        Provider.ReadShaderMaps = Settings.Serialization.ReadMaterialShaderMaps;
        
        Provider.Initialize();

        return Task.CompletedTask;
    }
    
    private async Task LoadKeys(CancellationToken cancellationToken = default)
    {
        if (Provider is not null)
        {
            await Provider.SubmitKeyAsync(Globals.ZERO_GUID, Encryption.MainAESKey);
            Log.Information($"Submitted AES Key: {Encryption.MainAESKey}");
            
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            if (Encryption.HasKeys && Provider is not null)
            {
                foreach (var vfs in Provider.UnloadedVfs.ToArray())
                {
                    foreach (var extraKey in Encryption.Keys.Where(extraKey => extraKey.IsValid && extraKey.Key != "").Where(extraKey => vfs.TestAesKey(extraKey.AESKey)))
                    {
                        if (Provider is null) continue;
                        
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        
                        await Provider.SubmitKeyAsync(vfs.EncryptionKeyGuid, extraKey.AESKey);
                        Log.Information($"Submitted Dynamic AES Key: {extraKey.AESKey}");
                    }
                }
            }
        }
    }
    
    private async void LoadMappings(CancellationToken cancellationToken = default)
    {
        var mapping = await RestAPI.Central.FetchMappingAsync(token: cancellationToken);

        if (Provider is null) return;

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var MappingFile = MappingsContainer.Path;

        if (!MappingsContainer.Override && string.IsNullOrEmpty(MappingFile) || !File.Exists(MappingFile))
        {
            if (mapping is { LocalPath: not null })
            {
                MappingFile = mapping.LocalPath;
            }
            else
            {
                MappingFile = GetLocallyRecentCreatedMappings();
            }
        }
        
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (MappingFile is not null && File.Exists(MappingFile))
        {
            Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(MappingFile);
        
            Log.Information($"Loaded Mappings: {MappingFile}");
        }
    }
    
    private static string? GetLocallyRecentCreatedMappings()
    {
        if (!Globals.MappingsFolder.Exists)
        {
            return null;
        }
        
        var usmapFiles = Globals.MappingsFolder.GetFiles("*.usmap");
        return usmapFiles.Length <= 0 ? null : usmapFiles.MaxBy(x => x.CreationTime)?.FullName;
    }
    
    private async Task LoadAssetRegistries(CancellationToken cancellationToken = default)
    {
        var assetRegistries = Provider.Files.Where(x => x.Key.Contains("AssetRegistry", StringComparison.OrdinalIgnoreCase)).ToArray();
        
        foreach (var (path, file) in assetRegistries)
        {
            if (!path.EndsWith(".bin") || path.Contains("Plugin", StringComparison.OrdinalIgnoreCase) || path.Contains("Editor", StringComparison.OrdinalIgnoreCase)) continue;

            UpdateStatus($"Loading {file.Name}");
            var assetArchive = await file.SafeCreateReaderAsync();
            if (assetArchive is null) continue;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var assetRegistry = new FAssetRegistryState(assetArchive);
                AssetRegistry.AddRange(assetRegistry.PreallocatedAssetDataBuffers);
                Log.Information("Loaded Asset Registry: {FilePath}", file.Path);
            }
            catch (Exception e)
            {
                Log.Warning("Failed to load asset registry: {FilePath}", file.Path);
                Log.Error(e.ToString());
            }
        }
    }
    
    /* File IO ~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    public async Task Save()
    {
        Directory.CreateDirectory(Globals.ProfilesFolder.ToString());

        if (IsAutoDetected)
        {
            FileName = Name + ".json";
        }
        else
        {
            if (FileName.IsNullOrEmpty())
            {
                FileName = GenerateRandomHash() + ".json";
            }
        }

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(Globals.ProfilesFolder.ToString(), FileName), json);
    }

    public void Delete()
    {
        if (File.Exists(SavedFilePath))
        {
            File.Delete(SavedFilePath);
            Log.Information($"Deleted Profile file: {SavedFilePath}");
        }
        else
        {
            Log.Information($"Profile settings file not found: {SavedFilePath}");
        }
    }
    
    private static string GenerateRandomHash()
    {
        return Guid.NewGuid().ToString("N")[..5];
    }
    
    public Profile Clone()
    {
        return new Profile
        {
            Name = Name,
            TexturePlatform = TexturePlatform,
            AudioFormat = AudioFormat,
            ArchiveDirectory = ArchiveDirectory,
            MappingsContainer = MappingsContainer.Clone(),
            Encryption = Encryption.Clone(),
            Version = Version,
            FileName = FileName,
            AutoDetectedGameId = AutoDetectedGameId,
            PakFileEntries = [..PakFileEntries],
            Display = Display.Clone(),
            Status = Status,
            IsInitialized = true,
            SecondaryAssetTypes = [..SecondaryAssetTypes],
            EnableDisplayLinks = true
        };
    }
    
    public Profile LazyClone()
    {
        return new Profile
        {
            Name = Name,
            ArchiveDirectory = ArchiveDirectory,
            Version = Version,
            FileName = FileName
        };
    }
    
    public void CopyFrom(Profile other)
    {
        Name = other.Name;
        ArchiveDirectory = other.ArchiveDirectory;
        TexturePlatform = other.TexturePlatform;
        AudioFormat = other.AudioFormat;
        MappingsContainer = other.MappingsContainer;
        Encryption = other.Encryption;
        Version = other.Version;
        FileName = other.FileName;
        AutoDetectedGameId = other.AutoDetectedGameId;
        PakFileEntries = new List<BasePakFileEntry>(other.PakFileEntries);
        Display = other.Display;
        SecondaryAssetTypes.Clear();
        SecondaryAssetTypes.AddRange(other.SecondaryAssetTypes);
        EnableDisplayLinks = true;
    }
    
    public bool Compare(Profile other)
    {
        if (other is null) return false;

        return 
           /* Allows user to change name without restart
           string.Equals(Name, other.Name, StringComparison.Ordinal) && 
           */
           string.Equals(ArchiveDirectory, other.ArchiveDirectory, StringComparison.Ordinal)
           && Equals(MappingsContainer, other.MappingsContainer)
           && string.Equals(MappingsContainer.Path, other.MappingsContainer.Path, StringComparison.Ordinal)
           && Equals(MappingsContainer.Override, other.MappingsContainer.Override)
           && Equals(Encryption, other.Encryption)
           && Equals(TexturePlatform, other.TexturePlatform)
           && Equals(Version, other.Version);
    }
    
    public void DisposeProvider(bool setStatus = true)
    {
        if (Provider is not null)
        {
            Provider.Dispose();
            
            Log.Information($"Disposed Provider for {Name}");
        }
        
        Provider = null!;

        if (setStatus)
        {
            CheckStatusNotifies();
            Status.SetState(EProfileStatus.Idle);
            IsInitialized = false;
        }
    }
    
    public static async Task<List<Profile>> LoadAllAsync()
    {
        Directory.CreateDirectory(Globals.ProfilesFolder.ToString());

        var profileFiles = Directory.GetFiles(Globals.ProfilesFolder.ToString(), "*.json");

        var tasks = profileFiles.Select(async file =>
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var profileSetting = JsonSerializer.Deserialize<Profile>(json);

                if (profileSetting is null) return null;

                profileSetting.Display.Profile = profileSetting;
                profileSetting.Display.Splash.Profile = profileSetting;

                if (string.IsNullOrEmpty(profileSetting.Display.GradientStartColor) || string.IsNullOrEmpty(profileSetting.Display.GradientEndColor))
                {
                    profileSetting.Display.SetRandomGradient();
                }
                
                profileSetting.EnableDisplayLinks = true;
                
                Log.Information($"Loaded Profile {profileSetting.Name}");

                return profileSetting;
            }
            catch (Exception ex)
            {
                Log.Information($"Error loading profile settings from {file}: {ex.Message}");
                
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(profile => profile is not null).ToList()!;
    }
    
    public async void TryAutoFetchAesKeys()
    {
        if (AutoDetectedGameId != EDetectedGameId.Fortnite)
        {
            return;
        }

        try
        {
            await FetchEncryptionKeysAsync("https://fortnitecentral.genxgames.gg/api/v1/aes");
            
            _ = Save();
        }
        catch (Exception ex)
        {
            Log.Error($"[TryAutoFetchAesKeys] Failed to fetch AES keys: {ex.Message}");
        }
    }

    public async Task TryAutoFetchAesKeysUndetected()
    {
        var GEN_API_URL = $"https://fortnitecentral.genxgames.gg/api/v1/aes?version={Name}";
        var GIT_ARCHIVE_URL = $"https://raw.githubusercontent.com/Tectors/fortnite-aes-archive/refs/heads/master/api/archive/{Name}.json";
        
        /*
         * Quick Notes about AES Keys and fetching them
         *
         * GMatrix API: https://fortnitecentral.genxgames.gg/api/v1/aes?version={...}
         * . Contains all versions above 18.00 (as of June 30th, 2025)
         *
         * dippyshere/fortnite-aes-archive: https://github.com/Tectors/fortnite-aes-archive
         *          https://raw.githubusercontent.com/Tectors/fortnite-aes-archive/refs/heads/master/api/archive/{...}.json
         * . Has versions below 18.00 (reliable), but above that is unreliable
         *
         * Solution?
         *
         * Use fortnite-aes-archive if below 18.00
         * Use GM API if above version 18.00
         */
        
        /* Only for Fortnite */
        if (!ArchiveDirectory.Contains("Fortnite")) return;
        
        var normalized = Name;
        if (Name.Count(c => c == '.') == 2)
        {
            var parts = Name.Split('.');
            if (parts.Length == 3)
            {
                normalized = parts[0] + "." + parts[1] + parts[2];
            }
        }

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            Log.Information($"Could not parse Profile Name {Name}");
            return;
        }

        var useGenAPI = value >= 18.00;
        var API_URL = useGenAPI ? GEN_API_URL :  GIT_ARCHIVE_URL;
        
        Encryption.Keys.Clear();
        
        Log.Information($"TryAutoFetchAesKeysUndetected: API_URL {API_URL}");
        
        await FetchEncryptionKeysAsync(API_URL, true);

        if (value >= 15.20)
        {
            var mapping = await RestAPI.Central.FetchMappingAsync(Name);
        
            if (mapping is { LocalPath: not null })
            {
                MappingsContainer.Override = true;
                MappingsContainer.Path = mapping.LocalPath;
            }
        }
        
        await InitializeProvider();
        InitializeCache(false);
        
        Validate();
        
        DisposeProvider(false);
    }
    
    public async Task FetchEncryptionKeysAsync(string url = null!, bool isUnknown = false)
    {
        var aes = await RestAPI.Central.GetAesAsync(url, useBaseUrl: false);

        if (aes is null)
        {
            Log.Information("FetchEncryptionKeysAsync Failed");
            return;
        }

        if (!string.IsNullOrWhiteSpace(aes.MainKey))
        {
            Encryption.MainKey = aes.MainKey;
        }

        if (aes.DynamicKeys is not { Count: > 0 })
        {
            return;
        }

        var newKeys = new List<EncryptionKey>();
        var dynamicGUIDs = new HashSet<string>();

        foreach (var key in aes.DynamicKeys)
        {
            if (!EncryptionKey.IsValidKey(key.Key))
            {
                continue;
            }

            newKeys.Add(key);
            dynamicGUIDs.Add(key.Guid);
        }

        if (isUnknown)
        {
            Encryption.UnknownKeys.AddRange(newKeys);
        }
        else
        {
            Encryption.Keys.RemoveAll(k => k is null);
            Encryption.Keys.RemoveAll(k => dynamicGUIDs.Contains(k.Guid));
            Encryption.Keys.AddRange(newKeys);
        }
    }
    
    private async Task InitializeTextureStreaming()
    {
        if (!TexturesOnDemand || !IsAutoDetected) return;
        
        try
        {
            var tocPath = await GetTocPath();
            if (string.IsNullOrEmpty(tocPath)) return;

            var tocName = tocPath.SubstringAfterLast("/");
            var onDemandFile = new FileInfo(Path.Combine(Globals.OnDemandFolder.FullName, tocName));
            if (!onDemandFile.Exists || onDemandFile.Length == 0)
            {
                await RestAPI.DownloadFileAsync($"https://download.epicgames.com/{tocPath}", onDemandFile.FullName);
            }

            var options = new IoStoreOnDemandOptions
            {
                ChunkBaseUri = new Uri("https://download.epicgames.com/ias/fortnite/", UriKind.Absolute),
                ChunkCacheDirectory = Globals.OnDemandFolder,
                Authorization = new AuthenticationHeaderValue("Bearer", Settings.Application.EpicAuth?.Token),
                Timeout = TimeSpan.FromSeconds(10)
            };

            var chunkToc = new IoChunkToc(onDemandFile);
            await Provider.RegisterVfs(chunkToc, options);
            await Provider.MountAsync();
        }
        catch (Exception)
        {
            Log.Information("Failed to Initialize Texture Streaming");
        }
    }
    
    private async Task<string> GetTocPath()
    {
        var onDemandPath = Path.Combine(ArchiveDirectory, @"..\..\..\Cloud\IoStoreOnDemand.ini");
        if (!File.Exists(onDemandPath)) return string.Empty;

        var onDemandText = await File.ReadAllTextAsync(onDemandPath);
        if (string.IsNullOrWhiteSpace(onDemandText)) return string.Empty;

        var onDemandIni = new ConfigIni();
        onDemandIni.Read(new StringReader(onDemandText));

        return onDemandIni
            .Sections.FirstOrDefault(s => s.Name == "Endpoint")?
            .Tokens.OfType<InstructionToken>()
            .FirstOrDefault(t => t.Key == "TocPath")?
            .Value.Replace("\"", "") ?? string.Empty;
    }
    
    public void Validate()
    {
        ValidateAllProperties();
    }
    
    public async Task BrowseArchiveDirectoryPath()
    {
        if (await App.BrowseFolderDialog(ArchiveDirectory) is { } path)
        {
            ArchiveDirectory = path;
        }
    }
    
    public async Task BrowseMappingsPathFile()
    {
        if (await App.BrowseFileDialog(fileTypes: Globals.MappingsFileType, suggestedFileName: MappingsContainer.Path) is { } path)
        {
            MappingsContainer.Path = path;
        }
    }

    public void OpenEditor(Window window = null!)
    {
        if (window is null)
        {
            window = MainWM.Window;
        }

        var win = new ProfileEditorWindow(this);
        win.CenterToScreen(window);
        _ = win.ShowDialog(window);
    }
    
    public static void CreateNewProfile(Window window = null!)
    {
        var newProfile = new Profile
        {
            Status =
            {
                State = EProfileStatus.Uncompleted
            }
        };
        
        newProfile.Display.SetRandomGradient();
        newProfile.OpenEditor(window);
    }
    
    public static IRelayCommand<Window> CreateNewProfileCommand { get; } = new RelayCommand<Window>(CreateNewProfile!);
}
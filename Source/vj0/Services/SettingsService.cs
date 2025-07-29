using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Serilog;
using vj0.Application;
using vj0.Framework;
using vj0.ViewModels.Settings;
using vj0.ViewModels.Settings.View;

namespace vj0.Services;

public partial class SettingsService : ObservableObject, IService
{
    [ObservableProperty] private ApplicationSettingsViewModel _application = new();
    [ObservableProperty] private OnlineSettingsViewModel _online = new();
    [ObservableProperty] private CloudSettingsViewModel _cloud = new();
    [ObservableProperty] private SerializationSettingsViewModel _serialization = new();
    [ObservableProperty] private ModelSettingsViewModel _model = new();
    
    [ObservableProperty] private DebugSettingsViewModel _debug = new();
    
    /* Tab Specific */
    [ObservableProperty] private ExplorerViewSettingsViewModel _explorerView = new();
    
    private static readonly DirectoryInfo DirectoryPath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Globals.CODENAME));
    
#if DEBUG
    private static readonly FileInfo FilePath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Globals.CODENAME,
        "Settings_Debug.json"));
#else
    private static readonly FileInfo FilePath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Globals.CODENAME,
        "Settings.json"));
#endif

    public SettingsService()
    {
        DirectoryPath.Create();
    }

    public void Load()
    {
        if (!FilePath.Exists) return;
        
        try
        {
            var settings = JsonConvert.DeserializeObject<SettingsService>(File.ReadAllText(FilePath.FullName));
            if (settings is null) return;

            foreach (var property in settings.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                
                var value = property.GetValue(settings);
                property.SetValue(this, value);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load settings: {e}");
        }
    }
    
    public void Save()
    {
        try
        {
            File.WriteAllText(FilePath.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch (Exception e)
        {
            Log.Error($"Failed to save settings: {e}");
        }
    }
}
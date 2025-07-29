using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;

namespace vj0.ViewModels.Settings;

public partial class SerializationSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _readBlueprintBytecode;
    [ObservableProperty] private bool _readMaterialShaderMaps;

    partial void OnReadBlueprintBytecodeChanged(bool value)
    {
        if (MainWM.CurrentProfile is null || MainWM.CurrentProfile.Provider is null) return;

        MainWM.CurrentProfile.Provider.ReadScriptData = value;
    }

    partial void OnReadMaterialShaderMapsChanged(bool value)
    {
        if (MainWM.CurrentProfile is null || MainWM.CurrentProfile.Provider is null) return;

        MainWM.CurrentProfile.Provider.ReadShaderMaps = value;
    }
}
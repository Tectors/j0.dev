using CommunityToolkit.Mvvm.ComponentModel;

using vj0.Framework.Models;
using vj0.Services.Framework;

namespace vj0.ViewModels.Settings;

public partial class CloudSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _runHostedAPI = true;
    
    partial void OnRunHostedAPIChanged(bool value)
    {
        if (value)
        {
            AppServices.Cloud.Initialize();
        }
        else
        {
            AppServices.Cloud.Stop();
        }
        
        MainWM.UpdateAPIServiceEnabled();
    }
}
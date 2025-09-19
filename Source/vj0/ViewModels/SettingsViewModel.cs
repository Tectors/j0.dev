using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;
using vj0.Models.Profiles;

namespace vj0.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private Profile? currentProfile = new Profile();
}

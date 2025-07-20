using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;

namespace vj0.ViewModels.Settings.View;

public partial class ExplorerViewSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _showStats;
}
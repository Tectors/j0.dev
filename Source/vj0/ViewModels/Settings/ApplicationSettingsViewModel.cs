using System.Text.Json.Serialization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CUE4Parse.UE4.Versions;
using FluentAvalonia.UI.Media.Animation;
using vj0.Framework.Models;
using vj0.Models.API.Responses;

namespace vj0.ViewModels.Settings;

public partial class ApplicationSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _completedOnboarding = true;
    [ObservableProperty] private bool _loadRecentProfileOnLaunch = true;
    [ObservableProperty] private bool _saveWindowResolution = true;
    [ObservableProperty] private PixelSize _lastWindowResolution;
    
    [ObservableProperty] private ELanguage _gameLanguage = ELanguage.English;
    [ObservableProperty] private bool _useTabTransitions = true;
    
    [ObservableProperty] private EpicAuthResponse? _epicAuth;

    [JsonIgnore] public NavigationTransitionInfo Transition => UseTabTransitions
        ? new SlideNavigationTransitionInfo()
        : new SuppressNavigationTransitionInfo();

    partial void OnGameLanguageChanged(ELanguage value)
    {
        if (MainWM.CurrentProfile == null || MainWM.CurrentProfile.Provider == null) return;

        Tasks.Run(() => MainWM.CurrentProfile.SetLanguage(value));
    }
}
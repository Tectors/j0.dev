using Avalonia.Interactivity;

using vj0.Framework.Models;
using vj0.Services;
using vj0.ViewModels;
using vj0.Views.Profiles;

namespace vj0.Views;

public partial class HomeView : ViewBase<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
        
        ViewModel.StartRotation(ViewModel.TagLines, 2700, RotatingTaglineText, useRandom: true);
        ViewModel.StartRotation(ViewModel.Tips, 8000, TipText, TipContainer, true);
    }

    private void OpenDiscord(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DISCORD_LINK);
    }

    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(GITHUB_LINK);
    }

    private void OpenXAccount(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(X_LINK);
    }

    private void OpenKoFi(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DONATE_LINK);
    }

    private void GetStarted(object? sender, RoutedEventArgs e)
    {
        Navigation.App.Open(typeof(ProfileSelectionView));
    }

    private void ExploreFiles(object? sender, RoutedEventArgs e)
    {
        MainWM.NavigateToExplorer();
    }
}

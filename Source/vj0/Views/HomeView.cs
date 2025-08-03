using System;
using System.Threading.Tasks;
using Avalonia.Animation.Easings;
using Avalonia.Interactivity;
using Avalonia.Media;
using vj0.Application;
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
        AppService.OpenLink(Globals.DISCORD_LINK);
    }

    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(Globals.GITHUB_LINK);
    }

    private void OpenXAccount(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(Globals.X_LINK);
    }

    private void OpenKoFi(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(Globals.DONATE_LINK);
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

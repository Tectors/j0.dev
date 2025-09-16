using System.Threading.Tasks;

using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

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
        Donate_OnPointerPressed(sender);
    }

    private void GetStarted(object? sender, RoutedEventArgs e)
    {
        Navigation.App.Open(typeof(ProfileSelectionView));
    }

    private void ExploreFiles(object? sender, RoutedEventArgs e)
    {
        MainWM.NavigateToExplorer();
    }

    private async void Donate_OnPointerPressed(object? sender)
    {
        if (HeartScale?.RenderTransform is not ScaleTransform heartTransform)
        {
            return;
        }

        const double startScale = 1.5;
        const double endScale = 15.0;
        const double durationMs = 400;
        const int stepMs = 16;

        var startOpacity = HeartScale.Opacity;
        const double endOpacity = 0.0;

        double elapsed = 0;

        while (elapsed < durationMs)
        {
            var time = elapsed / durationMs;
            var eased = new CubicEaseOut().Ease(time);

            var currentScale = startScale + (endScale - startScale) * eased;
            var currentOpacity = startOpacity + (endOpacity - startOpacity) * eased;

            heartTransform.ScaleX = currentScale;
            heartTransform.ScaleY = currentScale;
            HeartScale.Opacity = currentOpacity;

            await Task.Delay(stepMs);
            
            elapsed += stepMs;
        }

        heartTransform.ScaleX = 1.0;
        heartTransform.ScaleY = 1.0;
        
        HeartScale.Opacity = 1.0;
    }

    private void Donate_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Donate_OnPointerPressed(sender);
    }
}

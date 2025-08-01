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
        StartLoopingGradientAnimation();
        
        ViewModel.StartRotation(ViewModel.TagLines, 2700, RotatingTaglineText, useRandom: true);
        ViewModel.StartRotation(ViewModel.Tips, 8000, TipText, TipContainer, true);
    }
    
    private LinearGradientBrush? _branchBrush;
    private bool _isReversed;

    private async void StartLoopingGradientAnimation()
    {
        if (_branchBrush is null)
        {
            if (Resources.TryGetValue("AnimatingTextBrush", out var brushObj) && brushObj is LinearGradientBrush brush)
            {
                _branchBrush = brush;
            }
            else
            {
                return;
            }
        }

        var darkColor = Color.Parse("#303030");
        var lightColor = Colors.White;
        
        var startColor = _isReversed ? lightColor : darkColor;
        var endColor = _isReversed ? darkColor : lightColor;
        
        _branchBrush.GradientStops[0].Color = startColor;
        _branchBrush.GradientStops[1].Color = startColor;
        
        const float duration_full = 0.5f;
        
        const int steps = 100;
        var easing = new SineEaseInOut();
        var duration = TimeSpan.FromSeconds(duration_full);
        var delay = duration.TotalMilliseconds / steps;
        
        for (var i = 0; i <= steps; i++)
        {
            var progress = easing.Ease(i / (double)steps);
            
            if (progress <= 0.5)
            {
                var localProgress = progress * 2;
                _branchBrush.GradientStops[0].Color = InterpolateColor(startColor, endColor, localProgress);
                _branchBrush.GradientStops[1].Color = startColor;
            }
            else
            {
                var localProgress = (progress - 0.5) * 2;
                _branchBrush.GradientStops[0].Color = endColor;
                _branchBrush.GradientStops[1].Color = InterpolateColor(startColor, endColor, localProgress);
            }
            
            await Task.Delay((int)delay);
        }
        
        _branchBrush.GradientStops[0].Color = endColor;
        _branchBrush.GradientStops[1].Color = endColor;
        
        await Task.Delay((int)(duration_full / 2 * 1000));
        
        _isReversed = !_isReversed;
        StartLoopingGradientAnimation();
    }

    private static Color InterpolateColor(Color from, Color to, double t)
    {
        var r = (byte)(from.R + (to.R - from.R) * t);
        var g = (byte)(from.G + (to.G - from.G) * t);
        var b = (byte)(from.B + (to.B - from.B) * t);
        var a = (byte)(from.A + (to.A - from.A) * t);
        
        return Color.FromArgb(a, r, g, b);
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

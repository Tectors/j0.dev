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
        
        ViewModel.StartRotation(ViewModel.TagLines, 3000, RotatingTaglineText, useRandom: true);
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

        var fromStart = _isReversed ? Color.Parse("#303030") : Colors.White;
        var fromEnd = _isReversed ? Colors.White : Color.Parse("#303030");

        var toStart = _isReversed ? Colors.White : Color.Parse("#303030");
        var toEnd = _isReversed ? Color.Parse("#303030") : Colors.White;

        const int steps = 150;
        var easing = new SineEaseInOut();
        var duration = TimeSpan.FromSeconds(0.2);
        var delay = duration.TotalMilliseconds / steps;

        for (var i = 0; i <= steps; i++)
        {
            var time = easing.Ease(i / (double)steps);

            var lerpedStart = InterpolateColor(fromStart, toStart, time);
            var lerpedEnd = InterpolateColor(fromEnd, toEnd, time);

            _branchBrush.GradientStops[0].Color = lerpedStart;
            _branchBrush.GradientStops[1].Color = lerpedEnd;

            await Task.Delay((int)delay);
        }

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

    private void OpenTwitterAccount(object? sender, RoutedEventArgs e)
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

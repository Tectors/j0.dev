using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

using vj0.Services;
using vj0.Services.Framework;
using vj0.Windows;

namespace vj0.Views;

public partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
        
        AttachedToVisualTree += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                AttachGlow(Profile, "ProfileHoverGlow");
            }, DispatcherPriority.Loaded);
        };
    }
    
    private T? FindDescendant<T>(Visual root, string name) where T : Control
    {
        foreach (var child in root.GetVisualDescendants())
        {
            if (child is T match && match.Name == name)
            {
                return match;
            }
        }
        return null;
    }
    
    private void AttachGlow(Button button, string glowElementName)
    {
        if (button == null) return;

        var glow = FindDescendant<Grid>(this, glowElementName);
        if (glow == null) return;

        button.PointerEntered += (_, _) => FadeOpacity(glow, 1.0);
        button.PointerExited  += (_, _) => FadeOpacity(glow, 0.0);
    }
    
    private readonly Dictionary<Visual, CancellationTokenSource> _fadeTokens = new();

    private async void FadeOpacity(Visual target, double targetOpacity, int durationMs = 350)
    {
        if (target == null) return;

        if (_fadeTokens.TryGetValue(target, out var existingCts))
        {
            existingCts.Cancel();
            _fadeTokens.Remove(target);
        }

        var start = await Dispatcher.UIThread.InvokeAsync(() => target.Opacity);
        var end = targetOpacity;

        if (Math.Abs(start - end) < 0.001)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        _fadeTokens[target] = cts;
        var token = cts.Token;

        const int stepMs = 16;
        var elapsed = 0;

        try
        {
            while (elapsed < durationMs && !token.IsCancellationRequested)
            {
                var time = elapsed / (double)durationMs;
                var eased = CubicEaseOut(time);
                var value = Math.Clamp(start + (end - start) * eased, 0, 1);

                target.Opacity = value;

                await Task.Delay(stepMs, token);
                elapsed += stepMs;
            }

            if (!token.IsCancellationRequested)
            {
                target.Opacity = end;
            }
        }
        catch (TaskCanceledException) { }
        
        finally
        {
            _fadeTokens.Remove(target);
        }
    }

    private static double CubicEaseOut(double t)
    {
        var p = t - 1;
        return p * p * p + 1;
    }
    
    private void EditProfile(object? sender, RoutedEventArgs e) => (VisualRoot as MainWindow)?.OnEditProfile(sender, e);

    private void RestartAPI(object? sender, RoutedEventArgs e)
    {
        AppServices.Cloud.Restart();
    }
    
    public void OpenGitHubLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink($"{GITHUB_COMMIT_LINK}/{COMMIT}");
    }
    
    public void OpenGitHubLicense(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink($"{GITHUB_LINK}/blob/main/LICENSE");
    }

    private void CopyGitCloneCommand(object? sender, RoutedEventArgs e)
    {
        App.CopyText(IS_COMMIT_AVAILABLE
            ? $"git clone --recurse-submodules {GITHUB_LINK}.git && cd {GITHUB_REPO_NAME} && git checkout {COMMIT} && git submodule update --init --recursive\n"
            : $"git clone --recurse-submodules {GITHUB_LINK}.git && cd {GITHUB_REPO_NAME} && git submodule update --init --recursive\n");
    }
}

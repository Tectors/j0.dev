using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;

namespace vj0.Controls;

public class ScrollTopShadow : Control
{
    private Border? _topShadow;
    private double _targetOpacity;
    private readonly DispatcherTimer _fadeTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private ScrollViewer? _scrollViewer;

    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<ScrollTopShadow, ScrollViewer, bool>(
            "Enable",
            defaultValue: false);

    public static void SetEnable(AvaloniaObject element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static bool GetEnable(AvaloniaObject element) =>
        element.GetValue(EnableProperty);

    static ScrollTopShadow()
    {
        EnableProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is not ScrollViewer sv) return;
            if (!args.NewValue.GetValueOrDefault<bool>()) return;
            
            var shadow = new ScrollTopShadow();
            shadow.AttachToScrollViewer(sv);
        });
    }

    public ScrollTopShadow()
    {
        _fadeTimer.Tick += (_, _) => AnimateOpacityStep();
    }

    private void AttachToScrollViewer(ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
        _scrollViewer.ScrollChanged += OnScrollChanged;

        _topShadow = new Border
        {
            Opacity = 0,
            Height = 40,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop { Color = Color.Parse("#80000000"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#00000000"), Offset = 1 }
                }
            },
            IsHitTestVisible = false,
            ZIndex = 0
        };

        if (scrollViewer.Parent is Panel panel)
        {
            var index = panel.Children.IndexOf(scrollViewer);
            panel.Children.Insert(index + 1, _topShadow);
        }
        else if (scrollViewer.Parent is ContentPresenter presenter)
        {
            if (presenter.Content is ScrollViewer)
            {
                var grid = new Grid();
                presenter.Content = null;
                grid.Children.Add(scrollViewer);
                grid.Children.Add(_topShadow);
                presenter.Content = grid;
            }
        }
    }

    private bool HasScrollableVerticalContent => _scrollViewer is { } sv && sv.Extent.Height > sv.Viewport.Height;

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer is null) return;

        var newTargetOpacity = _scrollViewer.Offset.Y > 0 && HasScrollableVerticalContent ? 1 : 0;
        if (Math.Abs(_targetOpacity - newTargetOpacity) <= 0.01) return;

        _targetOpacity = newTargetOpacity;
        _fadeTimer.Start();
    }

    private void AnimateOpacityStep()
    {
        if (_topShadow is null) return;

        const double fadeSpeed = 0.15;
        var current = _topShadow.Opacity;
        var delta = (_targetOpacity - current) * fadeSpeed;

        _topShadow.Opacity += delta;

        if (!(Math.Abs(_topShadow.Opacity - _targetOpacity) < 0.01)) return;
        
        _topShadow.Opacity = _targetOpacity;
        _fadeTimer.Stop();
    }
}

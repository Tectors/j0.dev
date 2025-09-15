using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace vj0.Controls;

public partial class ScrollTopShadow : UserControl
{
    private static readonly StyledProperty<ScrollViewer?> ScrollViewerProperty = AvaloniaProperty.Register<ScrollTopShadow, ScrollViewer?>(nameof(ScrollViewer));

    public ScrollViewer? ScrollViewer
    {
        get => GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }

    public ScrollTopShadow()
    {
        InitializeComponent();
        
        ScrollViewerProperty.Changed.AddClassHandler<ScrollTopShadow>((x, e) =>
        {
            if (e.NewValue is ScrollViewer newViewer)
            {
                newViewer.ScrollChanged += x.OnScrollChanged;
            }

            if (e.OldValue is ScrollViewer oldViewer)
            {
                oldViewer.ScrollChanged -= x.OnScrollChanged;
            }
        });
        
        _fadeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _fadeTimer.Tick += (_, _) => AnimateOpacityStep();
    }
    
    private double _targetOpacity;
    private readonly DispatcherTimer _fadeTimer;
    
    private bool HasScrollableVerticalContent => ScrollViewer is { } sv && sv.Extent.Height > sv.Viewport.Height;
    
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (ScrollViewer is null) return;

        var newTargetOpacity = ScrollViewer.Offset.Y > 0 && HasScrollableVerticalContent ? 1 : 0;
    
        if (Math.Abs(_targetOpacity - newTargetOpacity) <= 0.01) return;
    
        _targetOpacity = newTargetOpacity;
        _fadeTimer.Start();
    }
    
    private void AnimateOpacityStep()
    {
        if (TopShadow is null) return;

        const double fadeSpeed = 0.15;
        var current = TopShadow.Opacity;
        var delta = (_targetOpacity - current) * fadeSpeed;

        TopShadow.Opacity += delta;

        if (!(Math.Abs(TopShadow.Opacity - _targetOpacity) < 0.01)) return;
        
        TopShadow.Opacity = _targetOpacity;
        _fadeTimer.Stop();
    }
}

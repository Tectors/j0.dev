using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace vj0.Controls;

public partial class CursorEffectControl : UserControl
{
    private bool isMouseOver;
    private DispatcherTimer? _fadeTimer;
    private double targetOpacity;
    private double currentOpacity;
    private const double FADE_SPEED = 0.075;
    private const int FADE_FPS = 60;

    public CursorEffectControl()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            if (this.GetVisualParent() is not IInputElement parent) return;
            
            parent.PointerMoved += OnPointerMoved;
            parent.PointerEntered += OnPointerEntered;
            parent.PointerExited += OnPointerExited;
        };
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isMouseOver) return;

        var position = e.GetPosition(this);
        Canvas.SetLeft(CursorCircle, position.X - CursorCircle.Width / 2);
        Canvas.SetTop(CursorCircle, position.Y - CursorCircle.Height / 2);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        isMouseOver = true;
        CursorCircle.IsVisible = true;
        targetOpacity = 1;
        StartFadeTimer();

        var position = e.GetPosition(this);
        Canvas.SetLeft(CursorCircle, position.X - CursorCircle.Width / 2);
        Canvas.SetTop(CursorCircle, position.Y - CursorCircle.Height / 2);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        isMouseOver = false;
        targetOpacity = 0.0;
        
        StartFadeTimer();
    }

    private void StartFadeTimer()
    {
        _fadeTimer ??= new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / FADE_FPS)
        };
        
        _fadeTimer.Tick += OnFadeTick;
        _fadeTimer.Start();
    }

    private void OnFadeTick(object? sender, EventArgs e)
    {
        var diff = targetOpacity - currentOpacity;

        if (Math.Abs(diff) < 0.01)
        {
            currentOpacity = targetOpacity;
            CursorCircle.Opacity = currentOpacity;

            if (currentOpacity <= 0)
            {
                CursorCircle.IsVisible = false;
            }

            _fadeTimer?.Stop();
            return;
        }

        currentOpacity += diff * FADE_SPEED;
        CursorCircle.Opacity = currentOpacity;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        _fadeTimer?.Stop();
        _fadeTimer = null;
    }
}

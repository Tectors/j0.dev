using System;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace vj0.Extensions.Animations;

public class GradientAnimator(LinearGradientBrush targetBrush, Color color1, Color color2, double durationSeconds, int steps) : IDisposable
{
    private readonly LinearGradientBrush TargetBrush = targetBrush ?? throw new ArgumentNullException(nameof(targetBrush));
    private bool IsReversed;
    private CancellationTokenSource? CancellationTokenSource;

    public void StartAnimation()
    {
        if (CancellationTokenSource != null) return;

        CancellationTokenSource = new CancellationTokenSource();
        _ = LoopGradientAnimation(CancellationTokenSource.Token);
    }

    public void StopAnimation()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = null;
    }

    private async Task LoopGradientAnimation(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var startColor = IsReversed ? color2 : color1;
            var endColor = IsReversed ? color1 : color2;

            if (TargetBrush.GradientStops.Count < 2)
            {
                TargetBrush.GradientStops.Clear();
                TargetBrush.GradientStops.Add(new GradientStop(startColor, 0));
                TargetBrush.GradientStops.Add(new GradientStop(startColor, 1));
            }
            else
            {
                TargetBrush.GradientStops[0].Color = startColor;
                TargetBrush.GradientStops[1].Color = startColor;
            }

            var easing = new SineEaseInOut();
            var duration = TimeSpan.FromSeconds(durationSeconds);
            var delay = duration.TotalMilliseconds / steps;

            for (var i = 0; i <= steps; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;
                var progress = easing.Ease(i / (double)steps);

                if (progress <= 0.5)
                {
                    var localProgress = progress * 2;
                    TargetBrush.GradientStops[0].Color = InterpolateColor(startColor, endColor, localProgress);
                    TargetBrush.GradientStops[1].Color = startColor;
                }
                else
                {
                    var localProgress = (progress - 0.5) * 2;
                    TargetBrush.GradientStops[0].Color = endColor;
                    TargetBrush.GradientStops[1].Color = InterpolateColor(startColor, endColor, localProgress);
                }

                await Task.Delay((int)delay, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) return;
            TargetBrush.GradientStops[0].Color = endColor;
            TargetBrush.GradientStops[1].Color = endColor;

            await Task.Delay((int)(durationSeconds / 2 * 1000), cancellationToken);

            IsReversed = !IsReversed;
        }
    }

    private static Color InterpolateColor(Color from, Color to, double t)
    {
        var r = (byte)(from.R + (to.R - from.R) * t);
        var g = (byte)(from.G + (to.G - from.G) * t);
        var b = (byte)(from.B + (to.B - from.B) * t);
        var a = (byte)(from.A + (to.A - from.A) * t);

        return Color.FromArgb(a, r, g, b);
    }

    public void Dispose()
    {
        StopAnimation();
        GC.SuppressFinalize(this);
    }
}

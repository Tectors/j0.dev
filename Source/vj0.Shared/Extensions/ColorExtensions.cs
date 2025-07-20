using System;
using Avalonia.Media;

namespace vj0.Shared.Extensions;

public static class ColorExtensions
{
    public static Color Saturate(this Color color, float factor)
    {
        RgbToHsl(color, out var h, out var s, out var l);

        s = Math.Clamp(s * factor, 0f, 1f);
        var (r, g, b) = HslToRgb(h, s, l);

        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static void RgbToHsl(Color c, out float h, out float s, out float l)
    {
        float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        h = s = l = (max + min) / 2f;

        if (Math.Abs(max - min) < 0.01)
        {
            h = s = 0f;
        }
        else
        {
            var d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            h = max switch
            {
                _ when Math.Abs(max - r) < 0.01 => (g - b) / d + (g < b ? 6 : 0),
                _ when Math.Abs(max - g) < 0.01 => (b - r) / d + 2,
                _ => (r - g) / d + 4,
            };
            h /= 6f;
        }
    }

    private static (float r, float g, float b) HslToRgb(float h, float s, float l)
    {
        if (s == 0f) return (l, l, l);

        var q = l < 0.5f ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;
        var r = HueToRgb(p, q, h + 1f / 3f);
        var g = HueToRgb(p, q, h);
        var b = HueToRgb(p, q, h - 1f / 3f);
        return (r, g, b);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        t = (t + 1f) % 1f;
        
        if (!(t < 1f / 6f))
        {
            return t switch
            {
                < 1f / 2f => q,
                < 2f / 3f => p + (q - p) * (2f / 3f - t) * 6f,
                _ => p
            };
        }

        return p + (q - p) * 6f * t;
    }
}
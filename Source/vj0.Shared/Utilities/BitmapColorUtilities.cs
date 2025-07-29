using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using vj0.Shared.Extensions;

namespace vj0.Shared.Utilities;

public static class BitmapColorUtilities
{
    public static (Color Start, Color End) GetGradientFromBitmap(Bitmap bmp)
    {
        if (bmp is null) return default;
        
        var width = bmp.PixelSize.Width;
        var height = bmp.PixelSize.Height;
        var stride = width * 4;
        var bufferSize = height * stride;

        var ptr = Marshal.AllocHGlobal(bufferSize);
        
        try
        {
            bmp.CopyPixels(new PixelRect(0, 0, width, height), ptr, bufferSize, stride);

            var buffer = new byte[bufferSize];
            Marshal.Copy(ptr, buffer, 0, bufferSize);

            long totalR = 0, totalG = 0, totalB = 0;
            var pixelCount = width * height;

            for (var i = 0; i < buffer.Length; i += 4)
            {
                var b = buffer[i];
                var g = buffer[i + 1];
                var r = buffer[i + 2];

                totalR += r;
                totalG += g;
                totalB += b;
            }

            var avgR = (byte)(totalR / pixelCount);
            var avgG = (byte)(totalG / pixelCount);
            var avgB = (byte)(totalB / pixelCount);

            var brightR = Math.Min(255, avgR + 60);
            var brightG = Math.Min(255, avgG + 60);
            var brightB = Math.Min(255, avgB + 60);

            var brightColor = Color.FromRgb((byte)brightR, (byte)brightG, (byte)brightB);

            var darkerColor = Color.FromRgb(
                Darken(brightColor.R, 33),
                Darken(brightColor.G, 33),
                Darken(brightColor.B, 33)
            );
            
            brightColor = brightColor.Saturate(5.0f);
            darkerColor = darkerColor.Saturate(5.0f);

            return (brightColor, darkerColor);

            static byte Darken(byte c, int amount) => (byte)Math.Max(0, c - amount);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
using System;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Shared.Utilities;

namespace vj0.Models.Profiles.Display;

public class ProfileSplash : ObservableObject
{
    [JsonIgnore]
    public BaseProfileDisplay Profile { get; set; } = null!;

    [JsonIgnore]
    private Bitmap? _cachedBitmap;

    [JsonIgnore]
    public Bitmap? Bitmap => _cachedBitmap ??= LoadSplashBitmap();

    public void Reload()
    {
        _cachedBitmap = LoadSplashBitmap();
        
        OnPropertyChanged(nameof(Bitmap));
        OnPropertyChanged(nameof(Exists));

        _cachedBitmapGradient = null;
        OnPropertyChanged(nameof(BitmapGradient));
    }
    
    [JsonIgnore] public bool Exists => Bitmap is not null;

    public ProfileSplash() { }

    public ProfileSplash(BaseProfileDisplay profile)
    {
        Profile = profile;
    }

    private Bitmap? LoadSplashBitmap()
    {
        if (Profile is null)
        {
            return null;
        }
        
        var ContentFolder = Profile.FindContentFolder();

        var splashPath = Path.Combine(ContentFolder, "Splash", "Splash.bmp");

        if (!File.Exists(splashPath)) splashPath = Path.Combine(ContentFolder, "Splash", "Splash.png");
        if (!File.Exists(splashPath)) return null;
        
        try
        {
            using var stream = File.OpenRead(splashPath);
            var original = new Bitmap(stream);

            var aspectRatio = (double)original.PixelSize.Width / original.PixelSize.Height;
            var isRoughlySquare = aspectRatio is > 0.85 and < 1.15;

            if (!isRoughlySquare)
            {
                return original;
            }

            var scaled = original.CreateScaledBitmap(
                new PixelSize(150, 150)
            );

            return scaled;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    [JsonIgnore]
    private LinearGradientBrush? _cachedBitmapGradient;
    
    [JsonIgnore]
    public LinearGradientBrush? BitmapGradient => Exists ? _cachedBitmapGradient ??= CreateBrushFromSplash(Bitmap!) : null;

    private static LinearGradientBrush CreateBrushFromSplash(Bitmap bmp)
    {
        var (start, end) = BitmapColorUtilities.GetGradientFromBitmap(bmp);
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(end, 0.0),
                new GradientStop(start, 4)
            ]
        };
    }
}
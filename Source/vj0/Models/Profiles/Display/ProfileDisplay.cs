using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Avalonia;
using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

namespace vj0.Models.Profiles.Display;

/* Handles Splash / Gradients / Abbreviation */
public partial class ProfileDisplay : ObservableObject
{
    [JsonIgnore] public BaseProfileDisplay Profile { get; set; }
    
    [ObservableProperty]
    [JsonIgnore]
    private ProfileSplash _splash;
    
    [JsonIgnore]
    private string? _abbreviation;

    [JsonIgnore]
    public string Abbreviation
        => string.IsNullOrWhiteSpace(_abbreviation)
            ? _abbreviation = GetAbbreviation()
            : _abbreviation;
    
    [JsonIgnore] public LinearGradientBrush? GradientBrush => Splash.Exists ? Splash.BitmapGradient : RandomGradientBrush;

    [ObservableProperty] private DateTime? _lastUsed;

    [ObservableProperty] private string _gradientStartColor = "";
    [ObservableProperty] private string _gradientEndColor = "";
    
    public ProfileDisplay(BaseProfileDisplay profile)
    {
        Profile = profile;
        Splash = new ProfileSplash(profile);
    }

    public void Reload()
    {
        ReloadOnlySplash();
        ReloadOnlyAbbreviation();
    }

    public void ReloadOnlySplash()
    {
        Splash.Reload();
        
        OnPropertyChanged(nameof(GradientBrush));
    }
    
    public void ReloadOnlyAbbreviation()
    {
        _abbreviation = GetAbbreviation();
        
        OnPropertyChanged(nameof(Abbreviation));
    }
    
    private string GetAbbreviation()
    {
        var words = Profile.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(word => word.Any(char.IsLetter)).ToArray();

        return words.Length switch
        {
            >= 2 => $"{char.ToUpper(words[0][0])}{char.ToUpper(words[1][0])}",
            1 => words[0].Length < 2
                ? char.ToUpper(words[0][0]).ToString()
                : $"{char.ToUpper(words[0][0])}{char.ToUpper(words[0][words[0].Length / 2])}",
            _ => Profile.Name.Contains('.') ? Profile.Name.Split('.')[0] : Profile.Name
        };
    }
    
    [JsonIgnore]
    private LinearGradientBrush RandomGradientBrush => _cachedRandomGradientBrush ??= CreateRandomGradientBrush();

    [JsonIgnore]
    private LinearGradientBrush? _cachedRandomGradientBrush;
    
    private LinearGradientBrush CreateRandomGradientBrush() => new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        [
            new GradientStop(Color.Parse(GradientStartColor), 0),
            new GradientStop(Color.Parse(GradientEndColor), 1)
        ]
    };

    public void SetRandomGradient()
    {
        var colorCombos = new List<(string Start, string End)>
        {
            ("#2563EB", "#3B82F6"),
            ("#7C3AED", "#8B5CF6"),
            ("#0D9488", "#14B8A6"),
            ("#EF4444", "#F97316"),
            ("#10B981", "#22C55E"),
            ("#F59E0B", "#FBBF24"),
            ("#DB2777", "#EC4899"),
            ("#9333EA", "#D946EF"),
            ("#3B82F6", "#60A5FA"),
            ("#22C55E", "#86EFAC"),
            ("#E879F9", "#F472B6"),
            ("#38BDF8", "#0EA5E9"),
            ("#FACC15", "#FDE68A"),
            ("#A855F7", "#D8B4FE"),
            ("#34D399", "#6EE7B7"),
            ("#FB923C", "#FDBA74"),
            ("#F43F5E", "#FB7185"),
            ("#4ADE80", "#86EFAC"),
            ("#818CF8", "#A5B4FC"),
            ("#F87171", "#FCA5A5")
        };

        var random = new Random();
        var selectedCombo = colorCombos[random.Next(colorCombos.Count)];
    
        GradientStartColor = selectedCombo.Start;
        GradientEndColor = selectedCombo.End;
        
        _cachedRandomGradientBrush = null;
    }
    
    public ProfileDisplay Clone()
    {
        return new ProfileDisplay(Profile)
        {
            LastUsed = LastUsed,
            GradientStartColor = GradientStartColor,
            GradientEndColor = GradientEndColor
        };
    }
}
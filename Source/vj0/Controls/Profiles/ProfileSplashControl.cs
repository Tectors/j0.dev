using Avalonia;
using Avalonia.Controls;

namespace vj0.Controls.Profiles;

public partial class ProfileSplashControl : UserControl
{
    /* 30 default width and height */
    private static readonly StyledProperty<double> SplashScaleProperty = AvaloniaProperty.Register<ProfileSplashControl, double>(nameof(SplashScale), 1.0);

    public double SplashScale
    {
        get => GetValue(SplashScaleProperty);
        set => SetValue(SplashScaleProperty, value);
    }
    
    private static readonly StyledProperty<CornerRadius> RadiusProperty = AvaloniaProperty.Register<ProfileSplashControl, CornerRadius>(nameof(Radius), new CornerRadius(12));

    public CornerRadius Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public ProfileSplashControl()
    {
        InitializeComponent();
    }
    
    public ProfileSplashControl(float splashScale)
    {
        InitializeComponent();
        
        SplashScale = splashScale;
    }
}
